using Arcadia.Core.Aoi;
using Arcadia.Core.Audit;
using Arcadia.Core.Combat;
using Arcadia.Core.Items;
using Arcadia.Core.World;
using Arcadia.Mdk.Ecs;
using Arcadia.Server.Systems;
using Arcadia.Server.Zone;

namespace Arcadia.Tests;

public sealed class CombatSystemTests
{
    [Fact]
    public void ProcessAttackIntent_WithinRange_ShouldDealDamage()
    {
        var (system, attacker, target, healths, _) = Setup();

        system.ProcessAttackIntent(attacker, target, DateTimeOffset.UtcNow);

        var targetHealth = healths[target];
        Assert.Equal(50f, targetHealth.Current); // 100 - 50 damage
    }

    [Fact]
    public void ProcessAttackIntent_OutOfRange_ShouldReject()
    {
        var (system, attacker, target, healths, positions) = Setup();

        // 将目标移动到超出范围
        positions[target] = new Position(200f, 200f);

        system.ProcessAttackIntent(attacker, target, DateTimeOffset.UtcNow);

        var targetHealth = healths[target];
        Assert.Equal(100f, targetHealth.Current); // 未扣血
    }

    [Fact]
    public void ProcessAttackIntent_OnCooldown_ShouldReject()
    {
        var (system, attacker, target, healths, _) = Setup();

        var now = DateTimeOffset.UtcNow;
        system.ProcessAttackIntent(attacker, target, now); // 第一次攻击成功
        system.ProcessAttackIntent(attacker, target, now.AddSeconds(0.5)); // 冷却中，应拒绝

        var targetHealth = healths[target];
        Assert.Equal(50f, targetHealth.Current); // 只扣了一次血
    }

    [Fact]
    public void ProcessAttackIntent_KillTarget_ShouldAddToDeathQueue()
    {
        var (system, attacker, target, healths, _) = Setup();

        // 两次攻击杀死目标（50 * 2 = 100）
        var now = DateTimeOffset.UtcNow;
        system.ProcessAttackIntent(attacker, target, now);
        system.ProcessAttackIntent(attacker, target, now.AddSeconds(1.5)); // 冷却后再攻击

        var targetHealth = healths[target];
        Assert.True(targetHealth.IsDead);
    }

    [Fact]
    public void FlushDeathQueue_ShouldDropLootAndRemoveEntity()
    {
        var (system, attacker, target, healths, positions) = Setup();

        // 杀死目标
        var now = DateTimeOffset.UtcNow;
        system.ProcessAttackIntent(attacker, target, now);
        system.ProcessAttackIntent(attacker, target, now.AddSeconds(1.5));

        // 处理死亡队列
        system.FlushDeathQueue(now.AddSeconds(2));

        // 实体应被移除
        Assert.False(positions.ContainsKey(target));
        Assert.False(healths.ContainsKey(target));
    }

    private static (CombatSystem System, EntityId Attacker, EntityId Target, Dictionary<EntityId, Health> Healths, Dictionary<EntityId, Position> Positions) Setup()
    {
        var aoi = new GridAoi();
        var auditSink = new TestAuditSink();
        var lootService = new ZoneLootService(auditSink);

        var positions = new Dictionary<EntityId, Position>();
        var healths = new Dictionary<EntityId, Health>();
        var combatStats = new Dictionary<EntityId, CombatStats>();
        var inventories = new Dictionary<EntityId, Inventory>();

        var attacker = new EntityId(1);
        var target = new EntityId(2);

        positions[attacker] = new Position(10f, 10f);
        positions[target] = new Position(20f, 20f); // 距离约 14.14，在范围内

        healths[attacker] = new Health(100f);
        healths[target] = new Health(100f);

        combatStats[attacker] = new CombatStats(damage: 50f, range: 20f, cooldownSeconds: 1f);
        combatStats[target] = new CombatStats(damage: 10f, range: 10f, cooldownSeconds: 1f);

        inventories[attacker] = new Inventory();
        inventories[target] = new Inventory();
        inventories[target].AddToCarried(new ItemStack(ItemId.New(), "loot", 1));

        var system = new CombatSystem(aoi, lootService, positions, healths, combatStats, inventories);

        return (system, attacker, target, healths, positions);
    }

    private sealed class TestAuditSink : IAuditSink
    {
        public void Record(AuditEvent auditEvent)
        {
            // 测试用空实现
        }
    }
}
