namespace Arcadia.Mdk.Resources;

/// <summary>
/// 资源强类型 Key（逻辑路径）。
/// Why: 统一资源寻址（贴图/音效/配置/文本等），让 Mod 能覆盖资源而不耦合物理路径。
/// Context: Godot 渲染层/Server 逻辑层都通过同一 Key 访问资源或其元信息。
/// Attention: 建议使用小写与 “/” 分隔；namespace 用于避免不同系统键冲突。
/// </summary>
public readonly record struct ResourceKey(string Namespace, string Path)
{
    public override string ToString() => $"{Namespace}:{Path}";
}

