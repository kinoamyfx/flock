using Arcadia.Mdk.Resources;

namespace Arcadia.Mdk.Modding;

/// <summary>
/// Mod 生命周期上下文。
/// Why: 限定 Mod 能触达的能力范围，避免直接触碰底层实现导致兼容性与安全风险。
/// Context: Core 作为“核心玩法包”也通过该上下文注册资源与系统。
/// Attention: 该接口是版本兼容面，变更必须走 OpenSpec。
/// </summary>
public interface IModContext
{
    ModId ModId { get; }

    /// <summary>
    /// 资源注册表（支持优先级覆盖）。
    /// </summary>
    IResourceRegistry Resources { get; }
}

