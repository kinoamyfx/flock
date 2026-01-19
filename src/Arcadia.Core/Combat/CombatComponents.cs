namespace Arcadia.Core.Combat;

/// <summary>
/// 生命值组件（服务端权威）。
/// Why: 生命值必须由服务端管理，避免客户端作弊（无敌/秒杀）。
/// Context: MVP 先实现简单的 HP 上限与当前值；后续演进护盾/护甲/百分比伤害等。
/// Attention: 死亡判定必须在 CombatSystem 中完成，避免分散导致"死而不僵"。
/// </summary>
public struct Health
{
    public float Current { get; set; }
    public float Max { get; set; }

    public Health(float max)
    {
        Max = max;
        Current = max;
    }

    public bool IsDead => Current <= 0f;

    public void TakeDamage(float damage)
    {
        Current = Math.Max(0f, Current - damage);
    }

    public void Heal(float amount)
    {
        Current = Math.Min(Max, Current + amount);
    }
}

/// <summary>
/// 战斗属性组件（攻击力/范围/冷却）。
/// Why: 攻击参数由服务端权威，客户端只负责发送"攻击意图"。
/// Context: MVP 先实现基础近战（范围检测）；后续演进远程/技能/Buff 等。
/// Attention: 攻击冷却必须服务端强制，避免客户端发送高频攻击指令绕过限制。
/// </summary>
public struct CombatStats
{
    public float AttackDamage { get; set; }
    public float AttackRange { get; set; }
    public float AttackCooldownSeconds { get; set; }
    public DateTimeOffset LastAttackAt { get; set; }

    public CombatStats(float damage, float range, float cooldownSeconds)
    {
        AttackDamage = damage;
        AttackRange = range;
        AttackCooldownSeconds = cooldownSeconds;
        LastAttackAt = DateTimeOffset.MinValue;
    }

    public bool CanAttackNow(DateTimeOffset now)
    {
        return (now - LastAttackAt).TotalSeconds >= AttackCooldownSeconds;
    }

    public void RecordAttack(DateTimeOffset now)
    {
        LastAttackAt = now;
    }
}
