using Arcadia.Mdk.Ecs;

namespace Arcadia.Server.Zone;

public sealed class ZonePlayerSession
{
    public ZonePlayerSession(
        ZonePlayerId playerId,
        ZoneInstanceId instanceId,
        ZoneLineId lineId,
        EntityId avatarEntityId,
        long lineResetVersionAtJoin,
        ZoneAvatarSnapshot lastSnapshot)
    {
        PlayerId = playerId;
        InstanceId = instanceId;
        LineId = lineId;
        AvatarEntityId = avatarEntityId;
        LineResetVersionAtJoin = lineResetVersionAtJoin;
        LastSnapshot = lastSnapshot;
    }

    public ZonePlayerId PlayerId { get; }
    public ZoneInstanceId InstanceId { get; private set; }
    public ZoneLineId LineId { get; private set; }
    public EntityId AvatarEntityId { get; }

    public ZoneSessionState State { get; private set; } = ZoneSessionState.Connected;

    public DateTimeOffset? DisconnectedAtUtc { get; private set; }
    public DateTimeOffset? DisconnectKeepUntilUtc { get; private set; }

    public long LineResetVersionAtJoin { get; private set; }

    public ZoneAvatarSnapshot LastSnapshot { get; private set; }

    public void UpdateSnapshot(ZoneAvatarSnapshot snapshot)
    {
        LastSnapshot = snapshot;
    }

    public void MarkDisconnected(DateTimeOffset nowUtc, TimeSpan keepDuration)
    {
        State = ZoneSessionState.Disconnected;
        DisconnectedAtUtc = nowUtc;
        DisconnectKeepUntilUtc = nowUtc.Add(keepDuration);
    }

    public void MarkReconnected()
    {
        State = ZoneSessionState.Connected;
        DisconnectedAtUtc = null;
        DisconnectKeepUntilUtc = null;
    }

    public void UpdateLineBinding(ZoneInstanceId instanceId, ZoneLineId lineId, long lineResetVersion)
    {
        InstanceId = instanceId;
        LineId = lineId;
        LineResetVersionAtJoin = lineResetVersion;
    }
}

