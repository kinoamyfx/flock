using Arcadia.Core.Audit;
using Arcadia.Core.Items;
using Arcadia.Core.Logging;
using Arcadia.Mdk.Ecs;

namespace Arcadia.Server.Zone;

public sealed class ZoneLootService
{
    private readonly IAuditSink _auditSink;

    public ZoneLootService(IAuditSink auditSink)
    {
        _auditSink = auditSink;
    }

    public LootContainer DropAllCarriedOnDeath(
        EntityId victimEntityId,
        string? killerPartyId,
        Inventory inventory,
        DateTimeOffset dropTime)
    {
        var drops = inventory.DropAllCarried();
        var loot = LootContainer.CreateFromDrops(
            drops,
            killerPartyId,
            dropTime,
            protectionDuration: TimeSpan.FromSeconds(10)); // Why: 10s 击杀者队伍独享拾取保护

        // Why: 全链路还原一次夺宝事件，方便定位作弊/纠纷。
        // Context: "全掉落（安全箱除外）"对经济与玩家信任是高风险点。
        // Attention: killerPartyId 未来应替换为强类型（PartyId）并脱敏展示。
        _auditSink.Record(
            new AuditEvent(
                EventType: "DropOnDeath",
                AtUtc: dropTime,
                Fields: new Dictionary<string, string>
                {
                    ["VictimEntityId"] = victimEntityId.Value.ToString(),
                    ["KillerPartyId"] = killerPartyId ?? "none",
                    ["LootId"] = loot.LootId.ToString("N"),
                    ["ItemCount"] = loot.Items.Count.ToString(),
                    ["ProtectedUntil"] = loot.ProtectedUntil.ToString("O")
                }));

        ArcadiaLog.Info(
            nameof(ZoneLootService),
            nameof(DropAllCarriedOnDeath),
            "DropOnDeath",
            ("VictimEntityId", victimEntityId.Value),
            ("KillerPartyId", killerPartyId ?? "none"),
            ("LootId", loot.LootId.ToString("N")),
            ("ItemCount", loot.Items.Count),
            ("ProtectedUntil", loot.ProtectedUntil.ToString("O")));

        return loot;
    }

    public bool TryPickupLoot(
        Guid lootId,
        string? pickerPartyId,
        Inventory pickerInventory,
        Dictionary<Guid, LootContainer> activeLoot,
        DateTimeOffset pickupTime,
        out List<ItemStack> pickedItems)
    {
        pickedItems = new List<ItemStack>();

        // Why: 检查掉落容器是否存在且拾取者有权限（10s保护期内仅击杀者队伍可拾取）。
        // Context: LootContainer.CanPickup 已包含保护期检查逻辑。
        // Attention: MVP 暂不做"部分拾取"，要么全拿要么不拿（后续可改为按格子拾取）。
        if (!activeLoot.TryGetValue(lootId, out var loot))
        {
            return false; // Loot not found or already picked
        }

        if (!loot.CanPickup(pickerPartyId, pickupTime))
        {
            ArcadiaLog.Info(
                nameof(ZoneLootService),
                nameof(TryPickupLoot),
                "PickupDenied",
                ("LootId", lootId.ToString("N")),
                ("PickerPartyId", pickerPartyId ?? "none"),
                ("KillerPartyId", loot.KillerPartyId ?? "none"),
                ("ProtectedUntil", loot.ProtectedUntil.ToString("O")),
                ("Now", pickupTime.ToString("O")));
            return false; // Protection active, only killer party can pickup
        }

        // Why: MVP 先全部拾取到携带物，后续可改为按背包空间限制。
        // Context: 如果背包满了，拾取失败（不会溢出丢失）。
        // Attention: 后续需加入"溢出提示"与"部分拾取"逻辑。
        pickedItems = loot.Items.ToList();
        foreach (var item in pickedItems)
        {
            pickerInventory.AddToCarried(item);
        }

        // Why: 审计日志记录拾取事件，用于复盘掉落物流转。
        // Context: "Kill → Drop → Loot" 三段审计链路。
        // Attention: LootId 应可追溯到 VictimEntityId（通过 DropOnDeath 事件）。
        _auditSink.Record(
            new AuditEvent(
                EventType: "PickupLoot",
                AtUtc: pickupTime,
                Fields: new Dictionary<string, string>
                {
                    ["LootId"] = lootId.ToString("N"),
                    ["PickerPartyId"] = pickerPartyId ?? "none",
                    ["ItemCount"] = pickedItems.Count.ToString()
                }));

        ArcadiaLog.Info(
            nameof(ZoneLootService),
            nameof(TryPickupLoot),
            "PickupSuccess",
            ("LootId", lootId.ToString("N")),
            ("PickerPartyId", pickerPartyId ?? "none"),
            ("ItemCount", pickedItems.Count));

        // Remove loot from active list
        activeLoot.Remove(lootId);
        return true;
    }
}

