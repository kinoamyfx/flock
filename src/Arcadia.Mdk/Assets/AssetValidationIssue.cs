namespace Arcadia.Mdk.Assets;

/// <summary>
/// 资产校验问题。
/// Why: 资产管线必须可自动化校验并给出可行动的诊断，否则画风与资源会在迭代中逐步漂移。
/// Context: v1.0.0 选择 Option A（高像素密度 Pixel Art）；需要稳定的命名/目录/Key 映射与 Mod 覆盖可追溯。
/// Attention: 这里只承载“问题描述”，不承载修复逻辑；修复由美术/程序按规则处理。
/// </summary>
public sealed record AssetValidationIssue(string Code, string Path, string Message);

