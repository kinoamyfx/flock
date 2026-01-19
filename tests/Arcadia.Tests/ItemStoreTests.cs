using Arcadia.Core.Items;
using Arcadia.Server.Persistence.InMemory;

namespace Arcadia.Tests;

public sealed class ItemStoreTests
{
    [Fact]
    public async Task DropAllCarriedToNewLoot_ShouldNotMoveSafeBox()
    {
        var store = new InMemoryItemStore();
        var playerId = "p1";

        var carried = new ItemStack(ItemId.New(), "wood", 10);
        var safe = new ItemStack(ItemId.New(), "heirloom", 1);
        await store.PutPlayerInventoryAsync(playerId, new InventorySnapshot(new[] { carried }, new[] { safe }), CancellationToken.None);

        var (lootId, dropped) = await store.DropAllCarriedToNewLootAsync(playerId, DateTimeOffset.UtcNow, CancellationToken.None);

        var after = await store.GetPlayerInventoryAsync(playerId, CancellationToken.None);
        Assert.Empty(after.Carried);
        Assert.Single(after.SafeBox);
        Assert.Equal("heirloom", after.SafeBox[0].TemplateId);

        Assert.NotEqual(Guid.Empty, lootId);
        Assert.Single(dropped);
        Assert.Equal("wood", dropped[0].TemplateId);
    }
}
