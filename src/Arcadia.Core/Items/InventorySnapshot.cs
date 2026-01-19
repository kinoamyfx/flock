namespace Arcadia.Core.Items;

public sealed record InventorySnapshot(
    IReadOnlyList<ItemStack> Carried,
    IReadOnlyList<ItemStack> SafeBox
);

