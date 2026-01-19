using System.Text.Json.Serialization;

namespace Arcadia.Mdk.Modding;

/// <summary>
/// Mod 清单（可由 mdk 生成/校验）。
/// Why: 将 Mod 的身份、依赖、优先级与可加载入口固化为可审计的输入。
/// Context: Server/Client 以统一方式加载 Core 与第三方 Mod，支持未来 DLC=官方 Mod。
/// Attention: 若要支持热更新/卸载，需要额外设计版本/状态机，本结构先满足 MVP。
/// </summary>
public sealed record ModManifest(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("priority")] int Priority,
    [property: JsonPropertyName("entryAssembly")] string? EntryAssembly,
    [property: JsonPropertyName("dependencies")] IReadOnlyList<string>? Dependencies
);

