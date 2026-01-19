using Arcadia.Mdk.Assets;

namespace Arcadia.Tests;

public sealed class AssetPipelineTests
{
    [Fact]
    public void AssetNamingRules_ShouldRejectUppercaseAndDash()
    {
        Assert.False(AssetNamingRules.IsValidSegment("Icon"));
        Assert.False(AssetNamingRules.IsValidSegment("icon-wood"));
        Assert.True(AssetNamingRules.IsValidSegment("icon_wood"));
    }

    [Fact]
    public void AssetPathMapper_ShouldMapPngToTextureKey()
    {
        var root = Path.Combine(Path.GetTempPath(), "arcadia_assets_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "items", "icons"));

        var file = Path.Combine(root, "items", "icons", "icon_wood.png");
        File.WriteAllText(file, "x");

        Assert.True(AssetPathMapper.TryMapToResourceKey(root, file, out var key, out var err), err);
        Assert.Equal("texture", key.Namespace);
        Assert.Equal("items/icons/icon_wood", key.Path.Replace('\\', '/'));
    }

    [Fact]
    public void AssetValidator_ShouldReportInvalidSegment()
    {
        var root = Path.Combine(Path.GetTempPath(), "arcadia_assets_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "items", "icons"));

        var file = Path.Combine(root, "items", "icons", "IconWood.png");
        File.WriteAllText(file, "x");

        var validator = new AssetValidator(root);
        var result = validator.Validate();
        Assert.False(result.IsOk);
        Assert.Contains(result.Issues, x => x.Code == "invalid_segment");
    }
}

