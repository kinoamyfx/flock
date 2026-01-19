using Arcadia.Core.Logging;
using Arcadia.Core.Net.Zone;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Arcadia.Client.Net.Enet;

/// <summary>
/// ENet 客户端传输（MVP: LiteNetLib 可靠 UDP，协议: ZoneWireCodec）。
/// Why: 与服务端 `EnetServerTransport` 对齐，跑通“握手→意图→快照”的可验收闭环。
/// Context: macOS arm64 下 ENet 绑定不稳定，MVP 先用 LiteNetLib；对外仍沿用 Enet 命名避免上层改动。
/// Attention: 禁止在日志中输出 token；传输层替换不得影响权威逻辑与协议语义。
/// </summary>
public sealed class EnetClientTransport : IDisposable
{
    private const string ConnectionKey = "ArcadiaZone";

    private readonly EventBasedNetListener _listener = new();
    private NetManager? _client;
    private NetPeer? _peer;
    private bool _started;
    private string _authToken = string.Empty;
    private string _playerId = string.Empty;
    private IDisposable? _correlationScope;
    private long _moveSeq;

    /// <summary>
    /// 服务器快照回调。
    /// </summary>
    public Action<ZoneSnapshot>? OnSnapshot { get; set; }

    public EnetClientTransport()
    {
        _listener.PeerConnectedEvent += peer =>
        {
            _peer = peer;
            ArcadiaLog.Info(nameof(EnetClientTransport), nameof(Connect), "Connected", ("PeerId", peer.Id));
            SendHello();
        };

        _listener.PeerDisconnectedEvent += (_, info) =>
        {
            TryLogDisconnectError(info);
            ArcadiaLog.Info(nameof(EnetClientTransport), nameof(PollOnce), "Disconnected");
        };

        _listener.NetworkReceiveEvent += (_, reader, _, _) =>
        {
            try
            {
                var buf = reader.GetRemainingBytes();
                OnReceive(buf);
            }
            finally
            {
                reader.Recycle();
            }
        };
    }

    /// <summary>
    /// 连接 Zone Server（握手 token 走 Hello.AuthToken）。
    /// </summary>
    public void Connect(string host, ushort port, string playerId, string authToken)
    {
        // Why: 传输层只负责握手 token；playerId 必须由服务端从 token 中解出来并绑定，避免伪造 playerId。
        // Context: `EnetServerTransport` 在校验 token 后建立 playerId ↔ peer 的可信映射。
        // Attention: 禁止打印 authToken；后续如需诊断，请输出 kid/过期时间等可脱敏字段。
        _authToken = authToken;
        _playerId = playerId;
        _correlationScope?.Dispose();
        _correlationScope = ArcadiaLogContext.BeginCorrelation(playerId);

        _client = new NetManager(_listener);
        _client.Start();
        _started = true;

        ArcadiaLog.Info(
            nameof(EnetClientTransport),
            nameof(Connect),
            "Start",
            ("Host", host),
            ("Port", port),
            ("PlayerId", playerId));

        _client.Connect(host, port, ConnectionKey);
    }

    /// <summary>
    /// 单次网络轮询（需在主循环中高频调用）。
    /// </summary>
    public void PollOnce(int serviceTimeoutMs)
    {
        // Why: LiteNetLib 采用轮询驱动；超时参数保留以兼容未来 ENet/QUIC 的 Service 语义。
        // Context: 压测与冒烟会以 15ms tick 轮询，模拟客户端主循环。
        // Attention: PollOnce 不应阻塞；阻塞会导致快照延迟与误判断线。
        _ = serviceTimeoutMs;

        if (!_started || _client is null)
        {
            return;
        }

        _client.PollEvents();
    }

    public void SendMoveIntent(float dirX, float dirY)
    {
        var peer = _peer;
        if (peer is null)
        {
            return;
        }

        var intent = new ZoneMoveIntent(Seq: Interlocked.Increment(ref _moveSeq), Dir: new ZoneVec2(dirX, dirY));
        Send(peer, ZoneWireCodec.EncodeMoveIntent(intent), DeliveryMethod.Unreliable);
    }

    public void SendPickupIntent(Guid lootId)
    {
        var peer = _peer;
        if (peer is null)
        {
            return;
        }

        Send(peer, ZoneWireCodec.EncodePickupIntent(new ZonePickupIntent(lootId)), DeliveryMethod.ReliableOrdered);
    }

    public void SendDebugKillSelf()
    {
        var peer = _peer;
        if (peer is null)
        {
            return;
        }

        Send(peer, ZoneWireCodec.EncodeDebugKillSelf(), DeliveryMethod.ReliableOrdered);
    }

    public void Flush()
    {
        // LiteNetLib 无需显式 Flush；保留接口以便未来替换真实 ENet/QUIC 时兼容。
    }

    private void SendHello()
    {
        var peer = _peer;
        if (peer is null)
        {
            return;
        }

        // Why: Hello 是唯一握手入口，服务端据此校验 token 并绑定 playerId。
        // Context: loadtest 与真实客户端都复用该握手语义。
        // Attention: authToken 属于敏感信息，严禁写日志。
        var hello = new ZoneHello(AuthToken: _authToken, ClientVersion: "loadtest-mvp-1.0.0");
        Send(peer, ZoneWireCodec.EncodeHello(hello), DeliveryMethod.ReliableOrdered);
        ArcadiaLog.Info(nameof(EnetClientTransport), nameof(SendHello), "HelloSent");
    }

    private static void Send(NetPeer peer, byte[] bytes, DeliveryMethod delivery)
    {
        var writer = new NetDataWriter();
        writer.Put(bytes);
        peer.Send(writer, delivery);
    }

    private void OnReceive(byte[] buf)
    {
        if (!ZoneWireCodec.TryDecode(buf, out var env))
        {
            ArcadiaLog.Info(nameof(EnetClientTransport), nameof(OnReceive), "Unknown", ("Length", buf.Length));
            return;
        }

        if (ZoneWireCodec.TryGetWelcome(env, out var welcome))
        {
            ArcadiaLog.Info(
                nameof(EnetClientTransport),
                nameof(OnReceive),
                "Welcome",
                ("InstanceId", welcome.InstanceId),
                ("Decision", welcome.ReconnectDecision));
            return;
        }

        if (ZoneWireCodec.TryGetSnapshot(env, out var snapshot))
        {
            OnSnapshot?.Invoke(snapshot);
            return;
        }

        if (ZoneWireCodec.TryGetError(env, out var error))
        {
            ArcadiaLog.Info(
                nameof(EnetClientTransport),
                nameof(OnReceive),
                "Error",
                ("Code", error.Code),
                ("Message", error.Message));
            return;
        }

        ArcadiaLog.Info(nameof(EnetClientTransport), nameof(OnReceive), "Unhandled", ("Type", env.Type.ToString()));
    }

    private static void TryLogDisconnectError(DisconnectInfo info)
    {
        // Why: 服务端通过 Disconnect additional data 返回 ZoneError（用于 auth_failed 等负向冒烟）。
        // Context: `scripts/smoke_enet.sh` 依赖该日志关键字验收负向路径。
        // Attention: 若 additional data 不是 ZoneWireEnvelope，必须兜底记录 length，避免静默吞错。
        var reader = info.AdditionalData;
        if (reader is null)
        {
            return;
        }

        try
        {
            var bytes = reader.GetRemainingBytes();
            if (bytes.Length == 0)
            {
                return;
            }

            if (!ZoneWireCodec.TryDecode(bytes, out var env))
            {
                ArcadiaLog.Info(nameof(EnetClientTransport), nameof(OnReceive), "DisconnectDataUnknown", ("Length", bytes.Length));
                return;
            }

            if (ZoneWireCodec.TryGetError(env, out var error))
            {
                ArcadiaLog.Info(
                    nameof(EnetClientTransport),
                    nameof(OnReceive),
                    "Error",
                    ("Code", error.Code),
                    ("Message", error.Message));
            }
        }
        catch (Exception ex)
        {
            ArcadiaLog.Error(nameof(EnetClientTransport), nameof(OnReceive), "DisconnectDataParseFailed", ex);
        }
    }

    public void Dispose()
    {
        try
        {
            if (_started)
            {
                _client?.Stop();
            }
        }
        finally
        {
            _client = null;
            _peer = null;
            _started = false;
            _playerId = string.Empty;
            _correlationScope?.Dispose();
            _correlationScope = null;
        }
    }
}
