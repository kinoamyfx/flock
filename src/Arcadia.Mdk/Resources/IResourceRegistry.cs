using Arcadia.Mdk.Modding;

namespace Arcadia.Mdk.Resources;

/// <summary>
/// 资源注册表（支持 Mod 覆盖）。
/// Why: 支持“更高优先级的 Mod 替换资源”的需求，并提供可追溯的覆盖链路。
/// Context: Core 注册默认资源；DLC/社区 Mod 按优先级覆盖；客户端渲染/服务端逻辑统一读取。
/// Attention: 资源内容存储形式由实现决定；MDK 仅定义覆盖规则与查询能力。
/// </summary>
public interface IResourceRegistry
{
    /// <summary>
    /// 注册一个资源候选项。
    /// </summary>
    void Register(ResourceKey key, ModId sourceModId, int priority, int loadOrder, object payload);

    /// <summary>
    /// 获取当前生效资源的 Payload 与描述；若不存在返回 false。
    /// </summary>
    bool TryGetEffective(ResourceKey key, out ResourceDescriptor descriptor, out object payload);

    /// <summary>
    /// 获取某资源 Key 的全部候选项（用于调试/审计）。
    /// </summary>
    IReadOnlyList<ResourceDescriptor> GetCandidates(ResourceKey key);
}

