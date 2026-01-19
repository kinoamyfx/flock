using Arcadia.Core.Items;

namespace Arcadia.Server.Persistence.InMemory;

public sealed class InMemoryItemStore : IItemStore
{
    private sealed record ItemRow(ItemId ItemId, string TemplateId, int Quantity, ItemOwnerKind OwnerKind, string OwnerId, ItemSlotKind SlotKind);

    private readonly object _gate = new();
    private readonly Dictionary<ItemId, ItemRow> _items = new();

    public Task PutPlayerInventoryAsync(string playerId, InventorySnapshot snapshot, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            foreach (var stack in snapshot.Carried)
            {
                _items[stack.ItemId] = new ItemRow(stack.ItemId, stack.TemplateId, stack.Quantity, ItemOwnerKind.Player, playerId, ItemSlotKind.Carried);
            }

            foreach (var stack in snapshot.SafeBox)
            {
                _items[stack.ItemId] = new ItemRow(stack.ItemId, stack.TemplateId, stack.Quantity, ItemOwnerKind.Player, playerId, ItemSlotKind.SafeBox);
            }
        }

        return Task.CompletedTask;
    }

    public Task<InventorySnapshot> GetPlayerInventoryAsync(string playerId, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var carried = _items.Values
                .Where(x => x.OwnerKind == ItemOwnerKind.Player && x.OwnerId == playerId && x.SlotKind == ItemSlotKind.Carried)
                .Select(x => new ItemStack(x.ItemId, x.TemplateId, x.Quantity))
                .ToArray();

            var safe = _items.Values
                .Where(x => x.OwnerKind == ItemOwnerKind.Player && x.OwnerId == playerId && x.SlotKind == ItemSlotKind.SafeBox)
                .Select(x => new ItemStack(x.ItemId, x.TemplateId, x.Quantity))
                .ToArray();

            return Task.FromResult(new InventorySnapshot(carried, safe));
        }
    }

    public Task<(Guid LootId, IReadOnlyList<ItemStack> Dropped)> DropAllCarriedToNewLootAsync(
        string playerId,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var lootId = Guid.NewGuid();

            var carried = _items.Values
                .Where(x => x.OwnerKind == ItemOwnerKind.Player && x.OwnerId == playerId && x.SlotKind == ItemSlotKind.Carried)
                .Select(x => new ItemStack(x.ItemId, x.TemplateId, x.Quantity))
                .ToArray();

            foreach (var stack in carried)
            {
                if (!_items.TryGetValue(stack.ItemId, out var row))
                {
                    continue;
                }

                _items[stack.ItemId] = row with { OwnerKind = ItemOwnerKind.LootContainer, OwnerId = lootId.ToString("N"), SlotKind = ItemSlotKind.Loot };
            }

            return Task.FromResult<(Guid, IReadOnlyList<ItemStack>)>((lootId, carried));
        }
    }
}
