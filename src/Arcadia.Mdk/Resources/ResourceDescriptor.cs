using Arcadia.Mdk.Modding;

namespace Arcadia.Mdk.Resources;

/// <summary>
/// 资源候选项描述（用于决策与审计）。
/// </summary>
public sealed record ResourceDescriptor(
    ResourceKey Key,
    ModId SourceModId,
    int Priority,
    int LoadOrder
);

