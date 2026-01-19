namespace Arcadia.Mdk.Ecs;

/// <summary>
/// 并行工具抽象。
/// Why: 让 ECS 系统能充分利用多核，同时允许在服务端切换不同调度实现（线程池/作业系统）。
/// Context: MVP 先用 ThreadPool/Parallel 实现；后续可替换为更细粒度的 job system。
/// Attention: 并行执行的系统必须保证线程安全；不要在并行循环里写共享状态，优先写组件分片或使用归约。
/// </summary>
public interface IParallelFor
{
    void For(int fromInclusive, int toExclusive, Action<int> body);
}

