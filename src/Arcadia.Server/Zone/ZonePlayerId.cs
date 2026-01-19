namespace Arcadia.Server.Zone;

/// <summary>
/// 玩家强类型 ID（服务端视角）。
/// Why: 断线重连、掉落归属、审计链路都需要稳定身份，避免用 string 到处传导致混乱。
/// Context: MVP 先用 string 包一层，后续可替换为 long/uuid 并保持兼容。
/// Attention: 该 ID 必须来源于鉴权层（Gateway），Zone 不信任客户端自报。
/// </summary>
public readonly record struct ZonePlayerId(string Value)
{
    public override string ToString() => Value;
}

