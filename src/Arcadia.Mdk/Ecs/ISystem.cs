namespace Arcadia.Mdk.Ecs;

/// <summary>
/// ECS 系统接口。
/// Why: 将“行为”从数据中剥离，便于并行化与确定性控制。
/// Context: Core/Mod 都通过系统实现玩法逻辑，Server tick 驱动系统执行。
/// Attention: 系统实现必须避免直接做 IO；IO 通过事件/命令队列在边界处集中处理，便于回放与审计。
/// </summary>
public interface ISystem
{
    void Execute(ISystemContext context);
}

