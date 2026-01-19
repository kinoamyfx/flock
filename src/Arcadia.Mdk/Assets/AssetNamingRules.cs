namespace Arcadia.Mdk.Assets;

/// <summary>
/// 资产命名规则（可执行约束）。
/// Why: 资产命名是 pipeline 的稳定性基础；不约束将导致资源重复、覆盖不可解释与跨平台问题。
/// Context: OpenSpec 要求只使用小写/数字/下划线，且可推导 ResourceKey。
/// Attention: 本规则覆盖“目录段”和“文件名（不含扩展名）”；扩展名必须小写。
/// </summary>
public static class AssetNamingRules
{
    public static bool IsValidSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
        {
            return false;
        }

        foreach (var ch in segment)
        {
            var ok = (ch is >= 'a' and <= 'z') || (ch is >= '0' and <= '9') || ch == '_';
            if (!ok)
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsValidExtension(string extensionWithoutDot)
    {
        if (string.IsNullOrWhiteSpace(extensionWithoutDot))
        {
            return false;
        }

        foreach (var ch in extensionWithoutDot)
        {
            var ok = (ch is >= 'a' and <= 'z') || (ch is >= '0' and <= '9');
            if (!ok)
            {
                return false;
            }
        }

        return true;
    }
}

