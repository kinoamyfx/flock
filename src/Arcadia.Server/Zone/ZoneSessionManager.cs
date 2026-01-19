using Arcadia.Core.Logging;
using Arcadia.Mdk.Ecs;

namespace Arcadia.Server.Zone;

public sealed class ZoneSessionManager
{
    private static readonly TimeSpan DefaultDisconnectKeepDuration = TimeSpan.FromSeconds(60);

    private readonly Dictionary<ZonePlayerId, ZonePlayerSession> _sessions = new();

    public ZonePlayerSession CreateOrReplaceSession(
        ZonePlayerId playerId,
        ZoneInstanceId instanceId,
        ZoneLineState lineState,
        EntityId avatarEntityId,
        ZoneAvatarSnapshot initialSnapshot)
    {
        // Why: MVP 先用“同 playerId 单会话”保证简单与可审计；多端同时登录可后续再做。
        // Context: 断线重连与掉落归属都需要稳定 session 绑定。
        // Attention: 如果未来支持多端，需要引入 sessionId 并明确抢占策略。
        var session = new ZonePlayerSession(
            playerId,
            instanceId,
            lineState.LineId,
            avatarEntityId,
            lineResetVersionAtJoin: lineState.ResetVersion,
            lastSnapshot: initialSnapshot);

        _sessions[playerId] = session;
        return session;
    }

    public bool TryGetSession(ZonePlayerId playerId, out ZonePlayerSession session) => _sessions.TryGetValue(playerId, out session!);

    public IEnumerable<KeyValuePair<ZonePlayerId, ZonePlayerSession>> GetAllSessions() => _sessions;

    public void OnDisconnect(ZonePlayerId playerId, DateTimeOffset nowUtc)
    {
        if (!_sessions.TryGetValue(playerId, out var session))
        {
            return;
        }

        session.MarkDisconnected(nowUtc, DefaultDisconnectKeepDuration);

        ArcadiaLog.Info(
            nameof(ZoneSessionManager),
            nameof(OnDisconnect),
            "Disconnected",
            ("PlayerId", playerId.Value),
            ("KeepUntilUtc", session.DisconnectKeepUntilUtc?.ToString("O") ?? string.Empty),
            ("InstanceId", session.InstanceId.ToString()),
            ("LineId", session.LineId.Value));
    }

    public ZoneReconnectDecision OnReconnect(
        ZonePlayerId playerId,
        DateTimeOffset nowUtc,
        ZoneInstanceId currentInstanceId,
        ZoneLineState currentLineState)
    {
        if (!_sessions.TryGetValue(playerId, out var session))
        {
            // Why: 没有旧 session 的情况由更上层（Gateway/Match）处理：新进图/新建角色等。
            // Context: Zone 不凭空创建玩家状态，避免被伪造重连打穿。
            // Attention: 调用方应把这种情况记录为异常重连。
            return ZoneReconnectDecision.SpawnAtEntrance;
        }

        var keepUntil = session.DisconnectKeepUntilUtc;
        if (session.State != ZoneSessionState.Disconnected || keepUntil is null)
        {
            return ZoneReconnectDecision.RestoreAtLastPosition;
        }

        var withinKeep = nowUtc <= keepUntil.Value;
        if (!withinKeep)
        {
            // Why: 超过 60s 保留窗口后，允许 Zone 回收实体/释放资源；重连只能走入口恢复。
            // Context: 老板确认“断线留60s，之后不保证原地恢复”。
            // Attention: 入口恢复前仍需校验背包/安全箱权威状态，避免状态漂移。
            session.MarkReconnected();
            session.UpdateLineBinding(currentInstanceId, currentLineState.LineId, currentLineState.ResetVersion);
            return ZoneReconnectDecision.SpawnAtEntrance;
        }

        var resetHappened = currentLineState.ResetVersion != session.LineResetVersionAtJoin;
        if (resetHappened)
        {
            session.MarkReconnected();
            session.UpdateLineBinding(currentInstanceId, currentLineState.LineId, currentLineState.ResetVersion);
            return ZoneReconnectDecision.SpawnAtEntrance;
        }

        session.MarkReconnected();
        return ZoneReconnectDecision.RestoreAtLastPosition;
    }
}

