using Arcadia.Core.Items;

namespace Arcadia.Server.Persistence;

/// <summary>
/// 物品权威存储接口。
/// Why: 将“去重/归属/移动”作为权威约束集中在存储边界，避免复制/回滚/并发拾取打穿经济。
/// Context: Zone Server 权威判定后调用该接口提交库存变更；后续 ENet/协议层替换不影响存储语义。
/// Attention: 该接口是反作弊关键面，任何语义调整必须同步 OpenSpec。
/// </summary>
public interface IItemStore
{
    Task PutPlayerInventoryAsync(string playerId, InventorySnapshot snapshot, CancellationToken cancellationToken);
    Task<InventorySnapshot> GetPlayerInventoryAsync(string playerId, CancellationToken cancellationToken);

    /// <summary>
    /// 将玩家“携带物”全部转移到新建掉落容器中（安全箱不受影响）。
    /// </summary>
    Task<(Guid LootId, IReadOnlyList<ItemStack> Dropped)> DropAllCarriedToNewLootAsync(
        string playerId,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken);
}
