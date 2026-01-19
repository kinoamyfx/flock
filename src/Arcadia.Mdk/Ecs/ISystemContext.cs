namespace Arcadia.Mdk.Ecs;

/// <summary>
/// 系统执行上下文（最小能力集）。
/// Why: 限制系统能力面，保证可测试、可并行、可审计。
/// Context: 未来可扩展（时间、随机源、事件总线等），但必须保持向后兼容。
/// Attention: 任何新增能力应以接口扩展/版本化的方式引入，避免破坏现有 Mod。
/// </summary>
public interface ISystemContext
{
    IWorld World { get; }
    IParallelFor ParallelFor { get; }
    long Tick { get; }
}

