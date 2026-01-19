using Arcadia.Mdk.Ecs;
using Arcadia.Server.Zone;

namespace Arcadia.Tests;

public sealed class DisconnectReconnectTests
{
    [Fact]
    public void ReconnectWithin60s_AndNotReset_ShouldRestoreAtLastPosition()
    {
        var sessionManager = new ZoneSessionManager();
        var line = new ZoneLineState(new ZoneLineId(1));
        var playerId = new ZonePlayerId("p1");
        var instanceId = ZoneInstanceId.New();

        sessionManager.CreateOrReplaceSession(playerId, instanceId, line, new EntityId(100), new ZoneAvatarSnapshot(1, 2));

        var t0 = DateTimeOffset.UtcNow;
        sessionManager.OnDisconnect(playerId, t0);

        var decision = sessionManager.OnReconnect(playerId, t0.AddSeconds(30), instanceId, line);
        Assert.Equal(ZoneReconnectDecision.RestoreAtLastPosition, decision);
    }

    [Fact]
    public void ReconnectWithin60s_ButReset_ShouldSpawnAtEntrance()
    {
        var sessionManager = new ZoneSessionManager();
        var line = new ZoneLineState(new ZoneLineId(1));
        var playerId = new ZonePlayerId("p1");
        var instanceId = ZoneInstanceId.New();

        sessionManager.CreateOrReplaceSession(playerId, instanceId, line, new EntityId(100), new ZoneAvatarSnapshot(1, 2));

        var t0 = DateTimeOffset.UtcNow;
        sessionManager.OnDisconnect(playerId, t0);

        line.Reset();

        var decision = sessionManager.OnReconnect(playerId, t0.AddSeconds(30), instanceId, line);
        Assert.Equal(ZoneReconnectDecision.SpawnAtEntrance, decision);
    }

    [Fact]
    public void ReconnectAfter60s_ShouldSpawnAtEntrance()
    {
        var sessionManager = new ZoneSessionManager();
        var line = new ZoneLineState(new ZoneLineId(1));
        var playerId = new ZonePlayerId("p1");
        var instanceId = ZoneInstanceId.New();

        sessionManager.CreateOrReplaceSession(playerId, instanceId, line, new EntityId(100), new ZoneAvatarSnapshot(1, 2));

        var t0 = DateTimeOffset.UtcNow;
        sessionManager.OnDisconnect(playerId, t0);

        var decision = sessionManager.OnReconnect(playerId, t0.AddSeconds(61), instanceId, line);
        Assert.Equal(ZoneReconnectDecision.SpawnAtEntrance, decision);
    }
}

