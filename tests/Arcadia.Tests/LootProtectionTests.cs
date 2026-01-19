using Arcadia.Core.Items;

namespace Arcadia.Tests;

public sealed class LootProtectionTests
{
    [Fact]
    public void CanPickup_WithinProtection_OnlyKillerPartyCanPickup()
    {
        var now = DateTimeOffset.UtcNow;
        var loot = LootContainer.CreateFromDrops(
            new[] { new ItemStack(ItemId.New(), "sword", 1) },
            killerPartyId: "party-a",
            createdAt: now,
            protectionDuration: TimeSpan.FromSeconds(10));

        // 击杀者队伍可拾取
        Assert.True(loot.CanPickup("party-a", now));

        // 其他队伍不可拾取
        Assert.False(loot.CanPickup("party-b", now));
        Assert.False(loot.CanPickup(null, now));
    }

    [Fact]
    public void CanPickup_AfterProtection_EveryoneCanPickup()
    {
        var now = DateTimeOffset.UtcNow;
        var loot = LootContainer.CreateFromDrops(
            new[] { new ItemStack(ItemId.New(), "sword", 1) },
            killerPartyId: "party-a",
            createdAt: now,
            protectionDuration: TimeSpan.FromSeconds(10));

        var after11s = now.AddSeconds(11);

        // 保护期过后，所有人可拾取
        Assert.True(loot.CanPickup("party-a", after11s));
        Assert.True(loot.CanPickup("party-b", after11s));
        Assert.True(loot.CanPickup(null, after11s));
    }

    [Fact]
    public void CanPickup_NoProtection_EveryoneCanPickup()
    {
        var now = DateTimeOffset.UtcNow;
        var loot = LootContainer.CreateFromDrops(
            new[] { new ItemStack(ItemId.New(), "wood", 10) },
            killerPartyId: null, // 无保护（例如环境掉落）
            createdAt: now,
            protectionDuration: TimeSpan.FromSeconds(10));

        // 无保护时，所有人立即可拾取
        Assert.True(loot.CanPickup("party-a", now));
        Assert.True(loot.CanPickup("party-b", now));
        Assert.True(loot.CanPickup(null, now));
    }

    [Fact]
    public void IsProtected_WithinDuration_ShouldReturnTrue()
    {
        var now = DateTimeOffset.UtcNow;
        var loot = LootContainer.CreateFromDrops(
            new[] { new ItemStack(ItemId.New(), "sword", 1) },
            killerPartyId: "party-a",
            createdAt: now,
            protectionDuration: TimeSpan.FromSeconds(10));

        Assert.True(loot.IsProtected(now));
        Assert.True(loot.IsProtected(now.AddSeconds(5)));
        Assert.True(loot.IsProtected(now.AddSeconds(9.9)));
        Assert.False(loot.IsProtected(now.AddSeconds(10.1)));
    }
}
