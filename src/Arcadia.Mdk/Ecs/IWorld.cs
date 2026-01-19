namespace Arcadia.Mdk.Ecs;

/// <summary>
/// ECS 世界接口（最小能力集）。
/// Why: 为 Mod 提供稳定、可替换的世界访问 API。
/// Context: Core 的实现负责内存布局与并发策略；MDK 只定义语义。
/// Attention: 为了并发安全，组件访问应尽量走批处理/查询接口；本 MVP 先给最小增删查。
/// </summary>
public interface IWorld
{
    EntityId CreateEntity();
    bool DestroyEntity(EntityId entityId);

    void SetComponent<T>(EntityId entityId, T component) where T : struct;
    bool TryGetComponent<T>(EntityId entityId, out T component) where T : struct;
    bool RemoveComponent<T>(EntityId entityId) where T : struct;

    IReadOnlyList<EntityId> QueryWith<T>() where T : struct;
}

