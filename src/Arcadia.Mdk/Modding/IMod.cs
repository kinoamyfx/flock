namespace Arcadia.Mdk.Modding;

/// <summary>
/// Mod 主入口接口。
/// Why: 允许 Core/DLC/社区 Mod 使用同一生命周期注册资源与系统。
/// Context: MDK 提供最小可依赖的 API 面；具体实现由 Core/Server/Client 决定。
/// Attention: 禁止在构造函数做昂贵初始化，应该放到 <see cref="OnLoad"/>，便于可控与可诊断。
/// </summary>
public interface IMod
{
    /// <summary>
    /// Mod 被加载时调用。
    /// </summary>
    void OnLoad(IModContext context);
}

