using Arcadia.Mdk.Ecs;

namespace Arcadia.Core.Ecs;

public sealed class SystemRunner
{
    private readonly List<ISystem> _systems = new();

    public void AddSystem(ISystem system)
    {
        _systems.Add(system);
    }

    public void ExecuteAll(IWorld world, long tick)
    {
        var context = new SystemContext(world, new ParallelFor(), tick);
        foreach (var system in _systems)
        {
            system.Execute(context);
        }
    }
}

