using Arcadia.Core.Items;

namespace Arcadia.Tests;

public sealed class InventoryDropTests
{
    [Fact]
    public void DropAllCarried_ShouldNotAffectSafeBox()
    {
        var inventory = new Inventory();
        inventory.AddToCarried(new ItemStack(ItemId.New(), "wood", 10));
        inventory.AddToCarried(new ItemStack(ItemId.New(), "stone", 5));
        inventory.AddToSafeBox(new ItemStack(ItemId.New(), "heirloom", 1));

        var dropped = inventory.DropAllCarried();
        Assert.Equal(2, dropped.Count);

        var snapshot = inventory.Snapshot();
        Assert.Empty(snapshot.Carried);
        Assert.Single(snapshot.SafeBox);
        Assert.Equal("heirloom", snapshot.SafeBox[0].TemplateId);
    }

    [Fact]
    public void AddToSafeBox_WhenFull_ShouldThrow()
    {
        var inventory = new Inventory();
        for (var i = 0; i < Inventory.SafeBoxSlotLimit; i++)
        {
            inventory.AddToSafeBox(new ItemStack(ItemId.New(), $"safe-{i}", 1));
        }

        Assert.Throws<InvalidOperationException>(() => inventory.AddToSafeBox(new ItemStack(ItemId.New(), "overflow", 1)));
    }
}
