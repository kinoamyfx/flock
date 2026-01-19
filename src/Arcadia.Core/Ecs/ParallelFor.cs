using Arcadia.Mdk.Ecs;

namespace Arcadia.Core.Ecs;

public sealed class ParallelFor : IParallelFor
{
    public void For(int fromInclusive, int toExclusive, Action<int> body)
    {
        System.Threading.Tasks.Parallel.For(fromInclusive, toExclusive, body);
    }
}

