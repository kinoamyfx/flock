using Arcadia.Mdk.Resources;

namespace Arcadia.Mdk.Assets;

/// <summary>
/// 物理路径 → ResourceKey 映射。
/// Why: Mod 覆盖必须基于逻辑 Key，而不是物理路径；否则难以跨平台、难以审计、也难以做 atlas/打包。
/// Context: OpenSpec 约定 `assets/.../*.png` → `texture:<relative-without-ext>`。
/// Attention: namespace 映射是“管线契约”；一旦变更必须升级 MDK 并提供迁移策略。
/// </summary>
public static class AssetPathMapper
{
    public static bool TryMapToResourceKey(string assetsRootDir, string filePath, out ResourceKey key, out string error)
    {
        key = default;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(assetsRootDir))
        {
            error = "assetsRootDir is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            error = "filePath is required.";
            return false;
        }

        var fullRoot = Path.GetFullPath(assetsRootDir);
        var fullFile = Path.GetFullPath(filePath);

        if (!fullFile.StartsWith(fullRoot, StringComparison.Ordinal))
        {
            error = $"File must be under assets root. Root={fullRoot}, File={fullFile}";
            return false;
        }

        var rel = Path.GetRelativePath(fullRoot, fullFile);
        rel = rel.Replace('\\', '/');

        var ext = Path.GetExtension(rel);
        if (string.IsNullOrWhiteSpace(ext))
        {
            error = "File extension is required.";
            return false;
        }

        ext = ext.TrimStart('.').ToLowerInvariant();

        var ns = ext switch
        {
            "png" or "webp" or "jpg" or "jpeg" => "texture",
            "wav" or "ogg" or "mp3" => "audio",
            "json" or "yml" or "yaml" => "data",
            "ttf" or "otf" => "font",
            _ => "file"
        };

        var withoutExt = rel[..^("." + ext).Length];
        withoutExt = withoutExt.TrimStart('/');

        if (string.IsNullOrWhiteSpace(withoutExt))
        {
            error = "Invalid relative path.";
            return false;
        }

        key = new ResourceKey(ns, withoutExt);
        return true;
    }
}

