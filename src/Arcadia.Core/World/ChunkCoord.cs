namespace Arcadia.Core.World;

/// <summary>
/// Chunk 坐标（2D）。
/// Why: world-tick 需要按 chunk 激活推进；必须用强类型避免用 string/tuple 带来的隐性 bug。
/// Context: 桃源牧歌为 2D 俯视；服务端 headless 模拟以 chunk 为最小激活单位。
/// Attention: 后续若引入 z/层级或不同 chunkSize，应新增版本化字段而非复用此类型语义。
/// </summary>
public readonly record struct ChunkCoord(int X, int Y);

