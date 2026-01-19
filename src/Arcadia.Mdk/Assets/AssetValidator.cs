using Arcadia.Mdk.Resources;

namespace Arcadia.Mdk.Assets;

/// <summary>
/// 资产校验器：扫描 assets 目录并校验命名/目录/ResourceKey 映射。
/// Why: v1.0.0 选择像素风后，最常见的“变丑”来源就是资产命名混乱、缩放漂移与覆盖不可追溯；先把最硬的规则自动化。
/// Context: 该校验器不依赖 Godot GUI，可在 CI 与本地快速运行；后续可扩展为校验 Godot import metadata。
/// Attention: 该组件不做“像素滤镜/导入设置”的实际执行，仅做可静态验证的约束；导入设置校验后续加到 Godot 项目侧。
/// </summary>
public sealed class AssetValidator
{
    private readonly string _assetsRootDir;

    public AssetValidator(string assetsRootDir)
    {
        if (string.IsNullOrWhiteSpace(assetsRootDir))
        {
            throw new ArgumentException("assetsRootDir is required.", nameof(assetsRootDir));
        }

        _assetsRootDir = assetsRootDir;
    }

    public AssetValidationResult Validate()
    {
        var issues = new List<AssetValidationIssue>();

        if (!Directory.Exists(_assetsRootDir))
        {
            issues.Add(new AssetValidationIssue("assets_root_missing", _assetsRootDir, "Assets root directory does not exist."));
            return new AssetValidationResult(issues);
        }

        var files = Directory.EnumerateFiles(_assetsRootDir, "*", SearchOption.AllDirectories)
            .Select(x => Path.GetFullPath(x))
            .ToArray();

        foreach (var file in files)
        {
            ValidateOne(file, issues);
        }

        return new AssetValidationResult(issues);
    }

    private void ValidateOne(string fullFilePath, List<AssetValidationIssue> issues)
    {
        var rel = Path.GetRelativePath(_assetsRootDir, fullFilePath).Replace('\\', '/');

        var extWithDot = Path.GetExtension(rel);
        if (string.IsNullOrWhiteSpace(extWithDot))
        {
            issues.Add(new AssetValidationIssue("missing_extension", rel, "File extension is required."));
            return;
        }

        var ext = extWithDot.TrimStart('.');
        if (!AssetNamingRules.IsValidExtension(ext))
        {
            issues.Add(new AssetValidationIssue("invalid_extension", rel, $"Extension must be lowercase alnum: {ext}"));
        }

        var withoutExt = rel[..^extWithDot.Length];
        var segments = withoutExt.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            issues.Add(new AssetValidationIssue("invalid_path", rel, "Path is invalid."));
            return;
        }

        foreach (var seg in segments)
        {
            if (!AssetNamingRules.IsValidSegment(seg))
            {
                issues.Add(new AssetValidationIssue("invalid_segment", rel, $"Invalid path segment: {seg}"));
                break;
            }
        }

        if (!AssetPathMapper.TryMapToResourceKey(_assetsRootDir, fullFilePath, out ResourceKey key, out var mapError))
        {
            issues.Add(new AssetValidationIssue("resource_key_map_failed", rel, mapError));
        }
        else
        {
            if (key.Namespace.Length == 0 || key.Path.Length == 0)
            {
                issues.Add(new AssetValidationIssue("invalid_resource_key", rel, "Mapped ResourceKey is invalid."));
            }
        }
    }
}

