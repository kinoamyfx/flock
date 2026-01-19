namespace Arcadia.Mdk.Ecs;

/// <summary>
/// Entity 的强类型 ID。
/// Why: 统一 ECS 实体标识，避免裸 int 在跨系统/网络/持久化时混淆。
/// Context: Server 的权威战斗与 AOI 都会以实体为中心组织数据。
/// Attention: 本类型仅表示身份，不承诺连续性；不要把它当数组下标使用。
/// </summary>
public readonly record struct EntityId(long Value)
{
    public override string ToString() => Value.ToString();
}

