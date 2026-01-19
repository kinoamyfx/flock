using Arcadia.Core.Aoi;
using Arcadia.Core.Audit;
using Arcadia.Core.Combat;
using Arcadia.Core.Items;
using Arcadia.Core.Logging;
using Arcadia.Core.World;
using Arcadia.Mdk.Ecs;
using Arcadia.Server.Zone;

namespace Arcadia.Server.Systems;

/// <summary>
/// 权威战斗系统（服务端验证 + 伤害计算 + 死亡触发）。
/// Why: 所有战斗逻辑必须服务端权威，避免客户端作弊（伤害/冷却/命中）。
/// Context: MVP 先实现近战（范围检测）+ 死亡全掉落；后续演进远程/技能/Buff。
/// Attention: 死亡触发必须原子化（扣血→死亡判定→掉落生成），避免"死而不僵"或"重复掉落"。
/// </summary>
public sealed class CombatSystem
{
    private readonly GridAoi _aoi;
    private readonly ZoneLootService _lootService;
    private readonly Dictionary<EntityId, Position> _positions;
    private readonly Dictionary<EntityId, Health> _healths;
    private readonly Dictionary<EntityId, CombatStats> _combatStats;
    private readonly Dictionary<EntityId, Inventory> _inventories;

    // Why: 死亡实体队列，每项记录（死者ID, 击杀者ID）用于拾取保护 10s。
    // Context: MVP 暂无队伍系统，用 EntityId 作为 PartyId（单人队伍）；后续接入队伍系统后替换。
    // Attention: 若后续引入尸体留存（例如可被复活），需调整为"死亡标记"而非直接移除。
    private readonly List<(EntityId VictimId, EntityId? KillerId)> _deathQueue = new();

    public CombatSystem(
        GridAoi aoi,
        ZoneLootService lootService,
        Dictionary<EntityId, Position> positions,
        Dictionary<EntityId, Health> healths,
        Dictionary<EntityId, CombatStats> combatStats,
        Dictionary<EntityId, Inventory> inventories)
    {
        _aoi = aoi;
        _lootService = lootService;
        _positions = positions;
        _healths = healths;
        _combatStats = combatStats;
        _inventories = inventories;
    }

    /// <summary>
    /// 处理攻击意图（来自客户端输入）。
    /// </summary>
    public void ProcessAttackIntent(EntityId attackerId, EntityId targetId, DateTimeOffset now)
    {
        // Why: 三重验证：冷却/范围/目标存活，避免客户端绕过限制。
        // Context: 客户端只发送"攻击意图"，服务端完全重新计算命中与伤害。
        // Attention: 验证失败不应断开连接（可能是网络延迟），仅拒绝攻击并日志记录（反作弊监控）。

        if (!_combatStats.TryGetValue(attackerId, out var stats) ||
            !_positions.TryGetValue(attackerId, out var attackerPos) ||
            !_positions.TryGetValue(targetId, out var targetPos) ||
            !_healths.TryGetValue(targetId, out var targetHealth))
        {
            ArcadiaLog.Info(
                nameof(CombatSystem),
                nameof(ProcessAttackIntent),
                "InvalidAttack",
                ("AttackerId", attackerId.Value),
                ("TargetId", targetId.Value),
                ("Reason", "MissingComponent"));
            return;
        }

        // 冷却检查
        if (!stats.CanAttackNow(now))
        {
            ArcadiaLog.Info(
                nameof(CombatSystem),
                nameof(ProcessAttackIntent),
                "AttackRejected",
                ("AttackerId", attackerId.Value),
                ("Reason", "Cooldown"));
            return;
        }

        // 范围检查（欧氏距离）
        var dx = targetPos.X - attackerPos.X;
        var dy = targetPos.Y - attackerPos.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance > stats.AttackRange)
        {
            ArcadiaLog.Info(
                nameof(CombatSystem),
                nameof(ProcessAttackIntent),
                "AttackRejected",
                ("AttackerId", attackerId.Value),
                ("Reason", "OutOfRange"),
                ("Distance", distance),
                ("Range", stats.AttackRange));
            return;
        }

        // 目标存活检查
        if (targetHealth.IsDead)
        {
            return; // 目标已死亡，静默忽略
        }

        // 扣血 + 记录攻击时间
        targetHealth.TakeDamage(stats.AttackDamage);
        _healths[targetId] = targetHealth;
        stats.RecordAttack(now);
        _combatStats[attackerId] = stats;

        ArcadiaLog.Info(
            nameof(CombatSystem),
            nameof(ProcessAttackIntent),
            "AttackSuccess",
            ("AttackerId", attackerId.Value),
            ("TargetId", targetId.Value),
            ("Damage", stats.AttackDamage),
            ("TargetHp", targetHealth.Current));

        // 死亡判定 + 加入死亡队列（记录击杀者）
        if (targetHealth.IsDead)
        {
            _deathQueue.Add((targetId, attackerId));
            ArcadiaLog.Info(
                nameof(CombatSystem),
                nameof(ProcessAttackIntent),
                "TargetDied",
                ("TargetId", targetId.Value),
                ("KillerId", attackerId.Value));
        }
    }

    /// <summary>
    /// Tick 结束后处理死亡队列（掉落 + 移除实体）。
    /// </summary>
    public void FlushDeathQueue(DateTimeOffset now)
    {
        foreach (var (deadEntity, killerId) in _deathQueue)
        {
            if (!_inventories.TryGetValue(deadEntity, out var inventory))
            {
                continue;
            }

            // Why: 死亡全掉落（安全箱除外），调用 ZoneLootService 生成掉落容器并审计。
            // Context: MVP 暂无队伍系统，用 EntityId.ToString() 作为 PartyId（单人队伍）；后续接入队伍后替换。
            // Attention: 掉落容器位置应为死亡实体位置，后续需传入 Position。
            var killerPartyId = killerId?.Value.ToString();
            _ = _lootService.DropAllCarriedOnDeath(deadEntity, killerPartyId, inventory, now);

            // 移除实体
            _positions.Remove(deadEntity);
            _healths.Remove(deadEntity);
            _combatStats.Remove(deadEntity);
            _inventories.Remove(deadEntity);
            _aoi.Remove(deadEntity);

            ArcadiaLog.Info(
                nameof(CombatSystem),
                nameof(FlushDeathQueue),
                "EntityRemoved",
                ("EntityId", deadEntity.Value));
        }

        _deathQueue.Clear();
    }
}
