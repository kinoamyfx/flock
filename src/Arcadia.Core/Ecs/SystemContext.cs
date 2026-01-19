using Arcadia.Mdk.Ecs;

namespace Arcadia.Core.Ecs;

public sealed class SystemContext : ISystemContext
{
    public SystemContext(IWorld world, IParallelFor parallelFor, long tick)
    {
        World = world;
        ParallelFor = parallelFor;
        Tick = tick;
    }

    public IWorld World { get; }
    public IParallelFor ParallelFor { get; }
    public long Tick { get; }
}

