namespace Arcadia.Core.Items;

/// <summary>
/// 掉落容器（死亡全掉落后生成的拾取物）。
/// Why: 服务端权威管理掉落物；拾取保护 10s 避免"抢怪"纠纷，符合"击杀者优先"的公平规则。
/// Context: 容器生成时记录击杀者与保护过期时间；拾取前必须检查权限。
/// Attention: PartyId 当前为字符串简化 MVP；后续应改为强类型（PartyId value object）。
/// </summary>
public sealed record LootContainer(
    Guid LootId,
    IReadOnlyList<ItemStack> Items,
    string? KillerPartyId, // Why: 击杀者队伍 ID（null 表示无保护，例如环境掉落）
    DateTimeOffset ProtectedUntil // Why: 保护过期时间（UTC），过期后所有人可拾取
)
{
    /// <summary>
    /// 从掉落物列表创建容器（含拾取保护 10s）。
    /// </summary>
    public static LootContainer CreateFromDrops(
        IReadOnlyList<ItemStack> items,
        string? killerPartyId,
        DateTimeOffset createdAt,
        TimeSpan protectionDuration)
    {
        return new LootContainer(
            Guid.NewGuid(),
            items,
            killerPartyId,
            createdAt + protectionDuration);
    }

    /// <summary>
    /// 检查当前时间是否在拾取保护期内。
    /// </summary>
    public bool IsProtected(DateTimeOffset now) => now < ProtectedUntil;

    /// <summary>
    /// 检查指定队伍是否可以拾取（保护期内仅击杀者队伍可拾取；过期后所有人可拾取）。
    /// </summary>
    public bool CanPickup(string? pickerPartyId, DateTimeOffset now)
    {
        if (!IsProtected(now))
        {
            return true; // 保护期已过，所有人可拾取
        }

        if (string.IsNullOrEmpty(KillerPartyId))
        {
            return true; // 无保护（环境掉落），所有人可拾取
        }

        return pickerPartyId == KillerPartyId; // 保护期内仅击杀者队伍可拾取
    }
}

