using Arcadia.Core.Logging;
using Arcadia.Core.Auth;
using Arcadia.Core.Net.Zone;
using Arcadia.Server.Zone;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Threading;

namespace Arcadia.Server.Net.Enet;

public sealed class EnetServerTransport : IDisposable
{
    private const string ConnectionKey = "ArcadiaZone";

    private readonly EventBasedNetListener _listener = new();
    private NetManager? _server;
    private readonly ZoneSessionManager _sessionManager;
    private readonly ZoneLineState _lineState;
    private readonly ZoneInstanceId _instanceId;
    private bool _started;
    private readonly Dictionary<int, ZonePlayerId> _peerToPlayer = new();
    private long _nextAvatarEntityId = 1;
    private int _maxClients;
    private readonly ZoneIntentRateLimiter _rateLimiter = new();
    private long _bytesIn;
    private long _bytesOut;

    public Action<ZonePlayerId, ZoneMoveIntent>? OnMoveIntent { get; set; }
    public Action<ZonePlayerId>? OnDebugKillSelf { get; set; }
    public Action<ZonePlayerId, ZonePickupIntent>? OnPickupIntent { get; set; }
    public Action<ZonePlayerId, ZoneEvacIntent>? OnEvacIntent { get; set; }

    public long TotalBytesIn => Interlocked.Read(ref _bytesIn);
    public long TotalBytesOut => Interlocked.Read(ref _bytesOut);
    public int ConnectedPlayers => _peerToPlayer.Count;

    public EnetServerTransport(ZoneSessionManager sessionManager, ZoneLineState lineState, ZoneInstanceId instanceId)
    {
        _sessionManager = sessionManager;
        _lineState = lineState;
        _instanceId = instanceId;
    }

    public void Start(ushort port, int maxClients)
    {
        // Why: 你要求“最大限度防作弊 + 未来可替换传输层”，但当前 macOS arm64 下 ENet 相关 C# 绑定不可用/不兼容。
        // Context: MVP 先用 LiteNetLib（纯 C# 可靠 UDP）跑通连接/断线/握手/重连语义；后续可替换为真正 ENet/QUIC。
        // Attention: 传输层替换不得影响权威判定与掉落/背包语义，协议与业务逻辑必须保持解耦。
        _maxClients = maxClients;
        _server = new NetManager(_listener);
        _server.Start(port);
        _started = true;

        ArcadiaLog.Info(nameof(EnetServerTransport), nameof(Start), "Start", ("Port", port), ("MaxClients", maxClients));

        _listener.ConnectionRequestEvent += request =>
        {
            if (_server is null)
            {
                request.Reject();
                return;
            }

            if (_server.ConnectedPeersCount >= _maxClients)
            {
                request.Reject();
                return;
            }

            request.AcceptIfKey(ConnectionKey);
        };

        _listener.PeerConnectedEvent += peer =>
        {
            ArcadiaLog.Info(nameof(EnetServerTransport), nameof(Start), "Connect", ("PeerId", peer.Id));
        };

        _listener.PeerDisconnectedEvent += (peer, _) =>
        {
            if (_peerToPlayer.TryGetValue(peer.Id, out var playerId))
            {
                using var correlationScope = ArcadiaLogContext.BeginCorrelation(playerId.Value);
                _sessionManager.OnDisconnect(playerId, DateTimeOffset.UtcNow);
                _peerToPlayer.Remove(peer.Id);
                _rateLimiter.DropPlayer(playerId);
            }

            ArcadiaLog.Info(nameof(EnetServerTransport), nameof(Start), "Disconnect", ("PeerId", peer.Id));
        };

        _listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
        {
            try
            {
                var buf = reader.GetRemainingBytes();
                OnReceive(peer, channel, buf);
            }
            finally
            {
                reader.Recycle();
            }
        };
    }

    public void PollOnce(int serviceTimeoutMs)
    {
        if (!_started)
        {
            return;
        }

        if (_server is null)
        {
            return;
        }

        _server.PollEvents();
    }

    private void OnReceive(NetPeer peer, byte channelId, byte[] buf)
    {
        Interlocked.Add(ref _bytesIn, buf.Length);

        if (!ZoneWireCodec.TryDecode(buf, out var env))
        {
            ArcadiaLog.Info(nameof(EnetServerTransport), nameof(OnReceive), "ReceiveUnknown", ("PeerId", peer.Id), ("Length", buf.Length));
            return;
        }

        if (ZoneWireCodec.TryGetHello(env, out var hello))
        {
            if (!TryLoadKeySet(out var keySet, out var keySetError))
            {
                DisconnectPeerWithError(peer, "auth_config_error", keySetError);
                return;
            }

            if (!HmacTokenCodec.TryValidate(hello.AuthToken, kid => keySet.ResolveSecret(kid), DateTimeOffset.UtcNow, out var payload, out var errorCode))
            {
                DisconnectPeerWithError(peer, "auth_failed", errorCode);
                return;
            }

            // Why: Gateway token 校验通过后再绑定 playerId，避免伪造 playerId 劫持重连与掉落链路。
            // Context: 断线60s重连依赖稳定 playerId；此处是唯一可信入口。
            // Attention: MVP 仅校验签名+过期；封禁/风控/设备指纹等应在 Gateway 做。
            var playerId = new ZonePlayerId(payload.PlayerId);
            _peerToPlayer[peer.Id] = playerId;
            using var correlationScope = ArcadiaLogContext.BeginCorrelation(playerId.Value);

            if (!_sessionManager.TryGetSession(playerId, out _))
            {
                _sessionManager.CreateOrReplaceSession(
                    playerId,
                    _instanceId,
                    _lineState,
                    avatarEntityId: new Arcadia.Mdk.Ecs.EntityId(Interlocked.Increment(ref _nextAvatarEntityId)),
                    initialSnapshot: new ZoneAvatarSnapshot(0, 0));
            }

            var decision = _sessionManager.OnReconnect(playerId, DateTimeOffset.UtcNow, _instanceId, _lineState);
            SendWelcome(peer, decision);
            return;
        }

        // Why: MoveIntent 是最频繁的消息，需验证 playerId 后才允许处理，防止未认证客户端发送移动意图。
        // Context: 客户端 WASD 输入 → MoveIntent → 服务端权威计算位置 → Snapshot 广播。
        // Attention: 移动频率应限流（例如每 tick 最多1次），避免恶意客户端刷屏。
        if (!_peerToPlayer.TryGetValue(peer.Id, out var authenticatedPlayerId))
        {
            DisconnectPeerWithError(peer, "unauthenticated", "must send Hello first");
            return;
        }

        using var __ = ArcadiaLogContext.BeginCorrelation(authenticatedPlayerId.Value);
        if (!_rateLimiter.TryConsume(authenticatedPlayerId, env.Type, DateTimeOffset.UtcNow, out var rateReason))
        {
            ArcadiaLog.Info(
                nameof(EnetServerTransport),
                nameof(OnReceive),
                "RateLimited",
                ("PeerId", peer.Id),
                ("Type", env.Type.ToString()),
                ("Reason", rateReason));
            return;
        }

        if (ZoneWireCodec.TryGetMoveIntent(env, out var moveIntent))
        {
            OnMoveIntent?.Invoke(authenticatedPlayerId, moveIntent);
            return;
        }

        if (ZoneWireCodec.TryGetPickupIntent(env, out var pickupIntent))
        {
            OnPickupIntent?.Invoke(authenticatedPlayerId, pickupIntent);
            return;
        }

        if (ZoneWireCodec.IsDebugKillSelf(env))
        {
            OnDebugKillSelf?.Invoke(authenticatedPlayerId);
            return;
        }

        if (ZoneWireCodec.TryGetEvacIntent(env, out var evacIntent))
        {
            OnEvacIntent?.Invoke(authenticatedPlayerId, evacIntent);
            return;
        }

        ArcadiaLog.Info(nameof(EnetServerTransport), nameof(OnReceive), "ReceiveUnhandled", ("PeerId", peer.Id), ("Type", env.Type.ToString()), ("ChannelId", channelId));
    }

    private void DisconnectPeerWithError(NetPeer peer, string code, string message)
    {
        if (_server is null)
        {
            return;
        }

        var bytes = ZoneWireCodec.EncodeError(new ZoneError(code, message));
        var writer = new NetDataWriter();
        writer.Put(bytes);
        Interlocked.Add(ref _bytesOut, bytes.Length);
        _server.DisconnectPeer(peer, writer);
    }

    private static bool TryLoadKeySet(out HmacKeySet keySet, out string error)
    {
        keySet = default!;
        error = string.Empty;

        try
        {
            // Format: ARCADIA_AUTH_KEYS="kid1=secret1;kid2=secret2", ARCADIA_AUTH_ACTIVE_KID optional
            var raw = Environment.GetEnvironmentVariable("ARCADIA_AUTH_KEYS");
            var activeKid = Environment.GetEnvironmentVariable("ARCADIA_AUTH_ACTIVE_KID");

            if (string.IsNullOrWhiteSpace(raw))
            {
                error = "missing_auth_keys";
                return false;
            }

            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var part in raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var kv = part.Split('=', 2, StringSplitOptions.TrimEntries);
                if (kv.Length != 2 || string.IsNullOrWhiteSpace(kv[0]) || string.IsNullOrWhiteSpace(kv[1]))
                {
                    error = "invalid_auth_keys";
                    return false;
                }

                map[kv[0]] = kv[1];
            }

            if (map.Count == 0)
            {
                error = "empty_auth_keys";
                return false;
            }

            if (string.IsNullOrWhiteSpace(activeKid))
            {
                activeKid = map.Keys.First();
            }

            keySet = new HmacKeySet(activeKid, map);
            return true;
        }
        catch (Exception ex)
        {
            error = $"keyset_error:{ex.GetType().Name}";
            return false;
        }
    }

    private void SendWelcome(NetPeer peer, ZoneReconnectDecision decision)
    {
        var msg = new ZoneWelcome(
            InstanceId: _instanceId.ToString(),
            LineId: _lineState.LineId.Value,
            ResetVersion: _lineState.ResetVersion,
            ReconnectDecision: decision == ZoneReconnectDecision.RestoreAtLastPosition ? "restore" : "entrance");

        var bytes = ZoneWireCodec.EncodeWelcome(msg);
        var writer = new NetDataWriter();
        writer.Put(bytes);
        Interlocked.Add(ref _bytesOut, bytes.Length);
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void BroadcastSnapshot(ZonePlayerId playerId, ZoneSnapshot snapshot)
    {
        // Why: MVP 先单播给对应玩家；后续接入 AOI 后按可见性过滤广播范围。
        // Context: 每 tick 广播一次，客户端用于插值渲染。
        // Attention: 广播频率应与 TickHz 一致（例如 20Hz），避免带宽浪费。
        if (_server is null)
        {
            return;
        }

        var bytes = ZoneWireCodec.EncodeSnapshot(snapshot);
        var writer = new NetDataWriter();
        writer.Put(bytes);

        foreach (var (peerId, pid) in _peerToPlayer)
        {
            if (pid == playerId)
            {
                var peer = _server.GetPeerById(peerId);
                peer?.Send(writer, DeliveryMethod.Unreliable);
                Interlocked.Add(ref _bytesOut, bytes.Length);
                break;
            }
        }
    }

    public void BroadcastEvacStatus(ZonePlayerId playerId, ZoneEvacStatus evacStatus)
    {
        // Why: 广播撤离状态给客户端，用于更新 HUD 撤离读条（进度 + 剩余时间）。
        // Context: 撤离中每 tick 广播一次状态，客户端据此更新读条 UI。
        // Attention: 仅在撤离中广播，避免无关消息刷屏；读条完成/打断后停止广播。
        if (_server is null)
        {
            return;
        }

        var bytes = ZoneWireCodec.EncodeEvacStatus(evacStatus);
        var writer = new NetDataWriter();
        writer.Put(bytes);

        foreach (var (peerId, pid) in _peerToPlayer)
        {
            if (pid == playerId)
            {
                var peer = _server.GetPeerById(peerId);
                peer?.Send(writer, DeliveryMethod.Unreliable);
                Interlocked.Add(ref _bytesOut, bytes.Length);
                break;
            }
        }
    }

    public void Flush()
    {
        // LiteNetLib 无需显式 Flush；保留接口以便未来替换真实 ENet/QUIC 时兼容。
    }

    public void Dispose()
    {
        try
        {
            if (_started)
            {
                _server?.Stop();
            }
        }
        finally
        {
            if (_started)
            {
                _server = null;
            }
        }
    }
}
