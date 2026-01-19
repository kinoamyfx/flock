using Arcadia.Core.Aoi;
using Arcadia.Core.Ecs;
using Arcadia.Core.Items;
using Arcadia.Core.Logging;
using Arcadia.Core.Net.Zone;
using Arcadia.Core.Resources;
using Arcadia.Core.Timing;
using Arcadia.Core.World;
using Arcadia.Mdk.Modding;
using Arcadia.Server.Net.Enet;
using Arcadia.Server.Persistence;
using Arcadia.Server.Persistence.InMemory;
using Arcadia.Server.Persistence.Postgres;
using System.Diagnostics;

namespace Arcadia.Server.Zone;

public sealed class ZoneServerHost
{
    private readonly ZoneServerOptions _options;

    public ZoneServerHost(ZoneServerOptions options)
    {
        _options = options;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        // Why: MVP 先把权威 tick 与核心系统骨架跑起来，再逐步填充网络/背包/掉落等模块。
        // Context: 你已确认“断线60s、全掉落仅排除安全箱、单线64软上限、自动分线”的规则。
        // Attention: 现阶段不做 ENet 具体实现，先接口化；避免协议变更导致大规模返工。
        var resources = new ResourceRegistry();

        // Core 作为“官方 Mod”加载（loadOrder=0，priority=0）。
        var coreModId = new ModId("core");
        resources.Register(new Arcadia.Mdk.Resources.ResourceKey("text", "ui/title"), coreModId, priority: 0, loadOrder: 0, payload: "桃源牧歌");

        var world = new World();
        var systems = new SystemRunner();
        var aoi = new GridAoi(gridSize: 64f); // Why: 64 单位/格，适合秘境场景（玩家分散）
        var playerPositions = new Dictionary<Mdk.Ecs.EntityId, Position>(); // Why: MVP 暂用字典管理位置，后续接入 ECS 组件系统
        var activeLoot = new Dictionary<Guid, LootContainer>(); // Why: 管理场景内所有掉落容器（LootId → LootContainer）
        var playerInventories = new Dictionary<ZonePlayerId, Inventory>(); // Why: MVP 暂用内存管理背包，后续接入 itemStore

        // Why: 撤离点机制（长读条 + 可打断 + 高成本）是 v1.0.0 Must 项。
        // Context: 玩家发送 EvacIntent → 服务端开始 10s 读条 → 期间移动/受击会打断 → 读条完成后标记为"已撤离"。
        // Attention: MVP 暂不做传送出秘境的实际逻辑（仅标记状态），后续接入场景切换与结算流程。
        var evacuationStates = new Dictionary<ZonePlayerId, EvacuationState>();
        const int EvacuationDurationMs = 10_000; // 10 seconds cast time
        const int EvacuationCostGold = 100; // Placeholder: high cost (后续可改为消耗撤离符)

        var db = TryCreatePostgresDatabase(cancellationToken);
        Arcadia.Core.Audit.IAuditSink auditSink = db is null ? new ConsoleAuditSink() : new PostgresAuditSink(db);
        var lootService = new ZoneLootService(auditSink);
        var sessionManager = new ZoneSessionManager();
        var instanceId = ZoneInstanceId.New();
        var lineState = new ZoneLineState(new ZoneLineId(1));
        IItemStore itemStore = db is null ? new InMemoryItemStore() : new PostgresItemStore(db);

        ArcadiaLog.Info(
            nameof(ZoneServerHost),
            nameof(RunAsync),
            "Start",
            ("TickHz", _options.TickHz),
            ("LineSoftCap", _options.LineSoftCap));

        var port = ushort.TryParse(Environment.GetEnvironmentVariable("ARCADIA_ENET_PORT"), out var p) ? p : (ushort)7777;
        using var transport = new EnetServerTransport(sessionManager, lineState, instanceId);
        transport.Start(port, maxClients: Math.Max(_options.LineSoftCap, 64));

        // Why: 玩家移动意图 → 权威位置计算（速度限制 + 碰撞检测占位）→ 更新 playerPositions。
        // Context: 客户端 WASD 输入 → MoveIntent（Dir归一化向量）→ 服务端应用速度与dt计算新位置。
        // Attention: MVP 暂不做碰撞检测与寻路，后续接入物理引擎/导航网格。
        var moveSpeed = 100f; // Units per second (placeholder)
        var lastMoveSeq = new Dictionary<ZonePlayerId, long>();
        var lastMoveTick = new Dictionary<ZonePlayerId, long>();
        var currentTick = 0L;
        var totalDrops = 0L;
        var totalPickups = 0L;

        transport.OnMoveIntent = (playerId, intent) =>
        {
            // Why: 每 tick 最多处理 1 次 MoveIntent，避免客户端在单 tick 内刷多次意图叠加造成“加速/瞬移”。
            // Context: FixedTickLoop 在每 tick 开始 PollOnce，会把本 tick 内到达的消息集中处理。
            // Attention: 该限流是“服务端权威修正”，不依赖客户端自律。
            if (lastMoveTick.TryGetValue(playerId, out var lastTick) && lastTick == currentTick)
            {
                return;
            }

            // Why: Seq 序列号防止乱序/重放；仅处理更新的序列号。
            // Context: 网络不保证顺序（即使可靠传输），客户端必须递增 Seq。
            // Attention: 后续需加入"Seq 回退检测"（防重放攻击）。
            if (lastMoveSeq.TryGetValue(playerId, out var lastSeq) && intent.Seq <= lastSeq)
            {
                ArcadiaLog.Info(
                    nameof(ZoneServerHost),
                    nameof(transport.OnMoveIntent),
                    "MoveSeqRejected",
                    ("PlayerId", playerId.Value),
                    ("Seq", intent.Seq),
                    ("LastSeq", lastSeq));
                return; // Ignore old/duplicate intent
            }

            lastMoveSeq[playerId] = intent.Seq;
            lastMoveTick[playerId] = currentTick;

            if (!sessionManager.TryGetSession(playerId, out var session))
            {
                return;
            }

            var entityId = session.AvatarEntityId;
            if (!playerPositions.TryGetValue(entityId, out var currentPos))
            {
                currentPos = new Position(0, 0); // Default spawn position
                playerPositions[entityId] = currentPos;
            }

            if (!ZoneMovement.TryApplyMove(currentPos, intent, moveSpeed, _options.TickHz, out var newPos, out var reason))
            {
                ArcadiaLog.Info(
                    nameof(ZoneServerHost),
                    nameof(transport.OnMoveIntent),
                    "MoveRejected",
                    ("PlayerId", playerId.Value),
                    ("Seq", intent.Seq),
                    ("Reason", reason));
                return;
            }

            playerPositions[entityId] = newPos;

            // Why: 移动时打断撤离读条（玩家必须静止 10s 才能完成撤离）。
            // Context: 撤离中玩家发送 MoveIntent → 检测位置变化 → 打断撤离。
            // Attention: 仅判断"是否有移动意图"即打断，无需等位置实际变化（防止客户端作弊）。
            if (evacuationStates.TryGetValue(playerId, out var evacState) && !evacState.Interrupted && !evacState.Completed)
            {
                evacuationStates[playerId] = evacState with { Interrupted = true };
                ArcadiaLog.Info(
                    nameof(ZoneServerHost),
                    "OnMoveIntent",
                    "EvacuationInterrupted",
                    ("PlayerId", playerId.Value),
                    ("Reason", "Movement"));
            }
        };

        transport.OnDebugKillSelf = playerId =>
        {
            // Why: MVP 调试用；后续替换为"CombatSystem 判定死亡"。
            // Context: 客户端按键触发 DebugKillSelf → 服务端触发死亡掉落。
            // Attention: 生产环境必须禁用此消息（或需 admin 权限）。
            if (!sessionManager.TryGetSession(playerId, out var session))
            {
                return;
            }

            var entityId = session.AvatarEntityId;
            var inventory = playerInventories.GetValueOrDefault(playerId) ?? new Inventory(); // Placeholder: should load from itemStore
            var carried1 = new ItemStack(ItemId.New(), "debug_loot", 1);
            inventory.AddToCarried(carried1);

            var loot = lootService.DropAllCarriedOnDeath(entityId, killerPartyId: null, inventory, DateTimeOffset.UtcNow);
            activeLoot[loot.LootId] = loot; // Add to active loot list
            totalDrops++;
            playerPositions.Remove(entityId); // Remove from position map
        };

        transport.OnPickupIntent = (playerId, intent) =>
        {
            // Why: 检查拾取权限（10s保护期）+ 将物品添加到背包 + 移除掉落容器。
            // Context: 客户端按 E 键 → PickupIntent → 服务端验证权限并执行拾取。
            // Attention: MVP 暂无队伍系统，pickerPartyId 固定为 null（后续接入组队功能）。
            if (!sessionManager.TryGetSession(playerId, out var session))
            {
                return;
            }

            var inventory = playerInventories.GetValueOrDefault(playerId);
            if (inventory is null)
            {
                inventory = new Inventory();
                playerInventories[playerId] = inventory;
            }

            var pickerPartyId = null as string; // Placeholder: should query from party system
            if (lootService.TryPickupLoot(intent.LootId, pickerPartyId, inventory, activeLoot, DateTimeOffset.UtcNow, out var pickedItems))
            {
                totalPickups++;
                ArcadiaLog.Info(
                    nameof(ZoneServerHost),
                    "OnPickupIntent",
                    "PickupSuccess",
                    ("PlayerId", playerId.Value),
                    ("LootId", intent.LootId.ToString("N")),
                    ("ItemCount", pickedItems.Count));
            }
            else
            {
                ArcadiaLog.Info(
                    nameof(ZoneServerHost),
                    "OnPickupIntent",
                    "PickupFailed",
                    ("PlayerId", playerId.Value),
                    ("LootId", intent.LootId.ToString("N")));
            }
        };

        transport.OnEvacIntent = (playerId, intent) =>
        {
            // Why: 玩家请求撤离 → 服务端开始 10s 读条 → 期间不移动/不受击 → 完成后标记为"已撤离"。
            // Context: v1.0.0 Must 项；撤离是玩家"安全退出秘境"的唯一方式（除死亡外）。
            // Attention: MVP 暂不校验"是否在撤离点附近"（后续加入传送阵位置检测）；高成本占位为 100 金币（后续改为撤离符）。
            if (!sessionManager.TryGetSession(playerId, out var session))
            {
                return;
            }

            // Check if already evacuating
            if (evacuationStates.TryGetValue(playerId, out var existing) && !existing.Interrupted && !existing.Completed)
            {
                ArcadiaLog.Info(
                    nameof(ZoneServerHost),
                    "OnEvacIntent",
                    "AlreadyEvacuating",
                    ("PlayerId", playerId.Value));
                return;
            }

            var entityId = session.AvatarEntityId;
            if (!playerPositions.TryGetValue(entityId, out var currentPos))
            {
                currentPos = new Position(0, 0);
            }

            // MVP: 不校验金币余额，仅记录高成本语义（后续接入经济系统）
            var now = DateTimeOffset.UtcNow;
            evacuationStates[playerId] = new EvacuationState(now, currentPos);

            ArcadiaLog.Info(
                nameof(ZoneServerHost),
                "OnEvacIntent",
                "EvacuationStarted",
                ("PlayerId", playerId.Value),
                ("Reason", intent.Reason),
                ("Cost", EvacuationCostGold),
                ("DurationMs", EvacuationDurationMs));
        };

        // MVP 示例：创建一个实体并模拟"死亡全掉落（安全箱除外）"
        var victim = world.CreateEntity();
        var inventory = new Inventory();
        var carried1 = new ItemStack(ItemId.New(), "wood", 10);
        inventory.AddToCarried(carried1);
        inventory.AddToSafeBox(new ItemStack(ItemId.New(), "heirloom", 1));
        _ = lootService.DropAllCarriedOnDeath(victim, killerPartyId: null, inventory, DateTimeOffset.UtcNow);

        // MVP 示例：把背包/安全箱与掉落容器按“权威存储语义”落地（若配置了 Postgres 则写入，否则写内存）。
        var demoPlayerId = "demo-player";
        await itemStore.PutPlayerInventoryAsync(
            demoPlayerId,
            new InventorySnapshot(new[] { carried1 }, new[] { new ItemStack(ItemId.New(), "heirloom", 1) }),
            cancellationToken);
        _ = await itemStore.DropAllCarriedToNewLootAsync(demoPlayerId, DateTimeOffset.UtcNow, cancellationToken);

        var tickLoop = new FixedTickLoop(_options.TickHz);
        var lastMetricsAtUtc = DateTimeOffset.UtcNow;
        var lastBytesIn = 0L;
        var lastBytesOut = 0L;
        await tickLoop.RunAsync(
            tick =>
            {
                currentTick = tick;
                var tickStartedAt = Stopwatch.GetTimestamp();

                transport.PollOnce(serviceTimeoutMs: 0);

                // Why: AOI 更新必须在 systems.ExecuteAll 之前，确保消息广播能拿到最新可见性。
                // Context: MVP 暂无玩家移动，后续接入移动系统后此处会更新 playerPositions。
                // Attention: AOI 更新开销应 < 5ms；若超标需优化为"仅移动时更新"。
                foreach (var (entity, pos) in playerPositions)
                {
                    aoi.UpdatePosition(entity, pos.X, pos.Y);
                }

                systems.ExecuteAll(world, tick);

                // Why: 每 tick 检查撤离进度：超时完成 → 标记已撤离；被打断 → 清除状态。
                // Context: 撤离读条 10s，期间移动/受击会打断（OnMoveIntent/CombatSystem 已处理打断逻辑）。
                // Attention: 完成后仅标记状态，不实际传送（后续接入场景切换）。
                var now = DateTimeOffset.UtcNow;
                var completedEvacuations = new List<ZonePlayerId>();
                foreach (var (playerId, evacState) in evacuationStates)
                {
                    if (evacState.Completed || evacState.Interrupted)
                    {
                        completedEvacuations.Add(playerId);
                        continue;
                    }

                    var elapsedMs = (now - evacState.StartedAt).TotalMilliseconds;
                    if (elapsedMs >= EvacuationDurationMs)
                    {
                        evacuationStates[playerId] = evacState with { Completed = true };
                        ArcadiaLog.Info(
                            nameof(ZoneServerHost),
                            "EvacTick",
                            "EvacuationCompleted",
                            ("PlayerId", playerId.Value),
                            ("ElapsedMs", (int)elapsedMs));
                    }
                }

                // Clean up completed/interrupted evacuations
                foreach (var playerId in completedEvacuations)
                {
                    evacuationStates.Remove(playerId);
                }

                // Why: 每 tick 广播 Snapshot 给所有玩家，客户端用于插值渲染。
                // Context: Snapshot 包含位置/HP/精力/可见掉落物，客户端据此更新画面。
                // Attention: 后续接入 AOI 后按可见性过滤（仅广播九宫格内玩家）。
                foreach (var (playerId, session) in sessionManager.GetAllSessions())
                {
                    var entityId = session.AvatarEntityId;
                    if (!playerPositions.TryGetValue(entityId, out var pos))
                    {
                        pos = new Position(0, 0); // Default position
                    }

                    // Why: 收集玩家可见范围内的掉落物（MVP 先返回所有掉落，后续接入 AOI 九宫格过滤）。
                    // Context: 客户端根据 Loot 列表显示地面掉落物提示。
                    // Attention: 掉落物数量过多时应限制返回数量（例如最近 20 个），避免 Snapshot 过大。
                    var pickerPartyId = null as string; // Placeholder: should query from party system
                    var visibleLoot = activeLoot.Values
                        .Select(loot =>
                        {
                            var protectedMs = loot.IsProtected(now)
                                ? (int)(loot.ProtectedUntil - now).TotalMilliseconds
                                : 0;
                            var canPick = loot.CanPickup(pickerPartyId, now);
                            return new ZoneLootInfo(
                                loot.LootId,
                                new ZoneVec2(0, 0), // Placeholder: should include actual position
                                loot.Items.Count,
                                protectedMs,
                                canPick);
                        })
                        .ToList();

                    var snapshot = new ZoneSnapshot(
                        Tick: tick,
                        PlayerPos: new ZoneVec2((float)pos.X, (float)pos.Y),
                        Hp: 100, // Placeholder: should query from Health component
                        Spirit: 100, // Placeholder: should query from Spirit component
                        Loot: visibleLoot
                    );

                    transport.BroadcastSnapshot(playerId, snapshot);

                    // Why: 广播撤离状态给客户端（读条进度 + 是否完成/打断）。
                    // Context: 客户端据此更新 HUD 撤离读条（进度条 + 剩余时间）。
                    // Attention: 仅在撤离中才广播 EvacStatus，避免无关消息刷屏。
                    if (evacuationStates.TryGetValue(playerId, out var evacState))
                    {
                        var elapsedMs = (int)(now - evacState.StartedAt).TotalMilliseconds;
                        var remainingMs = Math.Max(0, EvacuationDurationMs - elapsedMs);
                        var status = evacState.Completed ? "completed" : evacState.Interrupted ? "interrupted" : "casting";
                        var evacStatus = new ZoneEvacStatus(status, remainingMs);
                        transport.BroadcastEvacStatus(playerId, evacStatus);
                    }
                }

                // Why: 每 10 tick 日志一次 AOI 统计，用于监控可见性过滤是否生效。
                // Context: 后续接入 Metrics 时替换为专用监控指标。
                // Attention: 避免高频日志刷屏；仅保留关键统计（分线人口/AOI格数/可见实体数）。
                if (tick % 10 == 0 && playerPositions.Count > 0)
                {
                    var samplePos = playerPositions.Values.First();
                    var visibleCount = aoi.QueryNineGrid(samplePos.X, samplePos.Y).Count;
                    ArcadiaLog.Info(
                        nameof(ZoneServerHost),
                        "AoiTick",
                        "Stats",
                        ("Tick", tick),
                        ("PlayerCount", playerPositions.Count),
                        ("SampleVisibleCount", visibleCount));
                }

                // Why: 输出最小可观测性指标（人口/带宽/tick耗时/掉落拾取事件数）。
                // Context: 后续可接入 Metrics 系统（Meter/Prometheus），MVP 先用结构化日志保证可追溯。
                // Attention: 频率控制在 1s 级别，避免刷屏。
                var nowUtc = DateTimeOffset.UtcNow;
                if ((nowUtc - lastMetricsAtUtc).TotalSeconds >= 1)
                {
                    var bytesIn = transport.TotalBytesIn;
                    var bytesOut = transport.TotalBytesOut;
                    var inDelta = bytesIn - lastBytesIn;
                    var outDelta = bytesOut - lastBytesOut;

                    lastBytesIn = bytesIn;
                    lastBytesOut = bytesOut;
                    lastMetricsAtUtc = nowUtc;

                    var tickElapsedMs = (Stopwatch.GetTimestamp() - tickStartedAt) * 1000.0 / Stopwatch.Frequency;
                    ArcadiaLog.Info(
                        nameof(ZoneServerHost),
                        "MetricsTick",
                        "ZoneMetrics",
                        ("Tick", tick),
                        ("ConnectedPlayers", transport.ConnectedPlayers),
                        ("Sessions", sessionManager.GetAllSessions().Count()),
                        ("PlayerPositions", playerPositions.Count),
                        ("ActiveLoot", activeLoot.Count),
                        ("Evacuating", evacuationStates.Count),
                        ("BytesInPerSec", inDelta),
                        ("BytesOutPerSec", outDelta),
                        ("TickCostMs", tickElapsedMs.ToString("F2")),
                        ("DropsTotal", totalDrops),
                        ("PickupsTotal", totalPickups));
                }

                return Task.CompletedTask;
            },
            cancellationToken);
    }

    private static PostgresDatabase? TryCreatePostgresDatabase(CancellationToken cancellationToken)
    {
        var conn = Environment.GetEnvironmentVariable("ARCADIA_PG_CONN");
        if (string.IsNullOrWhiteSpace(conn))
        {
            return null;
        }

        var db = new PostgresDatabase(conn);

        var autoMigrate = string.Equals(
            Environment.GetEnvironmentVariable("ARCADIA_PG_AUTO_MIGRATE"),
            "1",
            StringComparison.OrdinalIgnoreCase);

        if (autoMigrate)
        {
            db.EnsureSchemaAsync(cancellationToken).GetAwaiter().GetResult();
        }

        return db;
    }
}

// Why: 管理玩家撤离状态（读条进度 + 打断条件检测）。
// Context: 撤离需要 10s 读条，期间移动/受击会打断；读条完成后标记为"已撤离"。
// Attention: MVP 暂不做实际传送逻辑，仅记录状态；后续接入场景切换与结算。
internal sealed record EvacuationState(
    DateTimeOffset StartedAt,
    Position StartPosition,
    bool Interrupted = false,
    bool Completed = false
);
