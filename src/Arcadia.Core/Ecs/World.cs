using System.Collections.Concurrent;
using Arcadia.Mdk.Ecs;

namespace Arcadia.Core.Ecs;

public sealed class World : IWorld
{
    private long _nextEntityId = 1;
    private readonly ConcurrentDictionary<long, ConcurrentDictionary<Type, object>> _entityComponents = new();

    public EntityId CreateEntity()
    {
        var id = Interlocked.Increment(ref _nextEntityId);
        _entityComponents.TryAdd(id, new ConcurrentDictionary<Type, object>());
        return new EntityId(id);
    }

    public bool DestroyEntity(EntityId entityId)
    {
        return _entityComponents.TryRemove(entityId.Value, out _);
    }

    public void SetComponent<T>(EntityId entityId, T component) where T : struct
    {
        if (!_entityComponents.TryGetValue(entityId.Value, out var components))
        {
            throw new InvalidOperationException($"Entity not found: {entityId}");
        }

        components[typeof(T)] = component;
    }

    public bool TryGetComponent<T>(EntityId entityId, out T component) where T : struct
    {
        component = default;
        if (!_entityComponents.TryGetValue(entityId.Value, out var components))
        {
            return false;
        }

        if (!components.TryGetValue(typeof(T), out var boxed))
        {
            return false;
        }

        component = (T)boxed;
        return true;
    }

    public bool RemoveComponent<T>(EntityId entityId) where T : struct
    {
        if (!_entityComponents.TryGetValue(entityId.Value, out var components))
        {
            return false;
        }

        return components.TryRemove(typeof(T), out _);
    }

    public IReadOnlyList<EntityId> QueryWith<T>() where T : struct
    {
        var type = typeof(T);
        var result = new List<EntityId>();

        foreach (var (id, components) in _entityComponents)
        {
            if (components.ContainsKey(type))
            {
                result.Add(new EntityId(id));
            }
        }

        return result;
    }
}

