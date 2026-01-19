using Arcadia.Core.Logging;

namespace Arcadia.Core.Items;

public sealed class Inventory
{
    public const int SafeBoxSlotLimit = 9;

    private readonly Dictionary<ItemId, ItemStack> _carried = new();
    private readonly Dictionary<ItemId, ItemStack> _safeBox = new();

    public InventorySnapshot Snapshot()
    {
        return new InventorySnapshot(_carried.Values.ToArray(), _safeBox.Values.ToArray());
    }

    public void AddToCarried(ItemStack stack)
    {
        Upsert(_carried, stack);
    }

    public void AddToSafeBox(ItemStack stack)
    {
        // Why: 介子袋（安全箱）是“全掉落”规则下的经济阀门；必须限制容量避免变相无限仓库打穿风险曲线。
        // Context: 老板已定为“9格可制作、允许在秘境内存取整理”；这里先固化容量硬约束，制作/消耗后续单独演进。
        // Attention: slot 以“物品堆栈”为单位（同 ItemId 视为同一格），后续若引入合并/拆分堆栈需同步调整规则。
        if (!_safeBox.ContainsKey(stack.ItemId) && _safeBox.Count >= SafeBoxSlotLimit)
        {
            ArcadiaLog.Info(
                nameof(Inventory),
                nameof(AddToSafeBox),
                "SafeBoxFull",
                ("SlotLimit", SafeBoxSlotLimit),
                ("CurrentSlots", _safeBox.Count),
                ("TemplateId", stack.TemplateId));
            throw new InvalidOperationException($"Safe box is full (slot limit: {SafeBoxSlotLimit}).");
        }

        Upsert(_safeBox, stack);
    }

    public IReadOnlyList<ItemStack> DropAllCarried()
    {
        // Why: “死亡全掉落（安全箱除外）”是秘境夺宝的核心风险/收益曲线。
        // Context: 服务器权威背包，掉落必须不可伪造且可审计。
        // Attention: 该方法只负责“携带物”的转移；安全箱永不掉落必须由结构保证。
        var dropped = _carried.Values.ToArray();
        _carried.Clear();
        ArcadiaLog.Info(nameof(Inventory), nameof(DropAllCarried), "DropAllCarried", ("Count", dropped.Length));
        return dropped;
    }

    private static void Upsert(Dictionary<ItemId, ItemStack> bag, ItemStack stack)
    {
        if (string.IsNullOrWhiteSpace(stack.TemplateId))
        {
            throw new ArgumentException("TemplateId is required.", nameof(stack));
        }

        if (stack.Quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stack), "Quantity must be positive.");
        }

        bag[stack.ItemId] = stack;
    }
}
