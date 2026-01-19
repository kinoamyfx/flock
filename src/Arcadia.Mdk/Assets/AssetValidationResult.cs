namespace Arcadia.Mdk.Assets;

/// <summary>
/// 资产校验结果。
/// Why: CLI 工具需要稳定的结果格式用于 CI 与本地开发反馈。
/// Context: `Arcadia.AssetTool` 会将问题打印并用 exit code 表达是否通过。
/// Attention: Issues 顺序不保证稳定；若需要稳定排序，请在展示层排序。
/// </summary>
public sealed record AssetValidationResult(IReadOnlyList<AssetValidationIssue> Issues)
{
    public bool IsOk => Issues.Count == 0;
}

