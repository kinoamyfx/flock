using Arcadia.Core.Resources;
using Arcadia.Mdk.Modding;
using Arcadia.Mdk.Resources;

namespace Arcadia.Tests;

public sealed class ResourceRegistryTests
{
    [Fact]
    public void HigherPriorityMod_ShouldOverride()
    {
        var registry = new ResourceRegistry();
        var key = new ResourceKey("text", "ui/title");

        registry.Register(key, new ModId("core"), priority: 0, loadOrder: 0, payload: "桃源牧歌");
        registry.Register(key, new ModId("dlc1"), priority: 10, loadOrder: 1, payload: "桃源牧歌·DLC");

        Assert.True(registry.TryGetEffective(key, out var descriptor, out var payload));
        Assert.Equal(new ModId("dlc1"), descriptor.SourceModId);
        Assert.Equal("桃源牧歌·DLC", payload);
    }

    [Fact]
    public void SamePriority_LaterLoadOrder_ShouldOverride()
    {
        var registry = new ResourceRegistry();
        var key = new ResourceKey("text", "ui/title");

        registry.Register(key, new ModId("a"), priority: 5, loadOrder: 1, payload: "A");
        registry.Register(key, new ModId("b"), priority: 5, loadOrder: 2, payload: "B");

        Assert.True(registry.TryGetEffective(key, out var descriptor, out var payload));
        Assert.Equal(new ModId("b"), descriptor.SourceModId);
        Assert.Equal("B", payload);
    }
}
