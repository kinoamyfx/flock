using ENet;
using Arcadia.Core.Net.Zone;
using System.Text;
using System.Text.Json;

namespace Arcadia.Client.Net;

/// <summary>
/// 网络客户端（ENet + ZoneWireCodec）
/// Why: 连接 Zone Server，复用服务端消息协议，避免重复实现。
/// Context: 使用 ENet-CSharp 库，对接服务端的 `EnetServerTransport`。
/// Attention: 消息编解码必须与服务端完全一致（ZoneWireCodec）。
/// </summary>
public sealed class NetworkClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private Host? _client;
    private Peer _peer;
    private bool _isConnected;

    public event Action? Connected;
    public event Action? Disconnected;
    public event Action<ZoneWireMessageType, JsonElement>? MessageReceived;

    // Why: 追踪 MoveIntent 序列号（客户端预测）。
    private long _moveSeq = 0;

    public NetworkClient()
    {
        Library.Initialize();
    }

    /// <summary>
    /// 连接到 Zone Server。
    /// </summary>
    public void Connect(string host, ushort port, string authToken = "")
    {
        Console.WriteLine($"[NetworkClient] Connecting to {host}:{port}...");

        _client = new Host();
        _client.Create();

        var address = new Address();
        address.SetHost(host);
        address.Port = port;

        _peer = _client.Connect(address);

        // Why: 连接成功后立即发送 Hello 消息（握手）。
        // Context: 服务端需要 authToken 验证（MVP 阶段留空即可）。
        SendHello(authToken);
    }

    /// <summary>
    /// 轮询网络事件（在主循环中调用）。
    /// </summary>
    public void Poll()
    {
        if (_client == null) return;

        Event netEvent;
        while (_client.Service(0, out netEvent) > 0)
        {
            switch (netEvent.Type)
            {
                case EventType.Connect:
                    _isConnected = true;
                    Console.WriteLine("[NetworkClient] Connected to server!");
                    Connected?.Invoke();
                    break;

                case EventType.Disconnect:
                    _isConnected = false;
                    Console.WriteLine("[NetworkClient] Disconnected from server.");
                    Disconnected?.Invoke();
                    break;

                case EventType.Receive:
                    OnReceive(netEvent.Packet);
                    netEvent.Packet.Dispose();
                    break;
            }
        }
    }

    /// <summary>
    /// 发送 Hello 消息（握手）。
    /// </summary>
    private void SendHello(string authToken)
    {
        var hello = new ZoneHello(authToken, "silknet-mvp-1.0.0");
        SendMessage(ZoneWireMessageType.Hello, hello);
        Console.WriteLine("[NetworkClient] Sent Hello message.");
    }

    /// <summary>
    /// 发送 MoveIntent 消息（移动输入）。
    /// </summary>
    public void SendMoveIntent(float dirX, float dirY)
    {
        var intent = new ZoneMoveIntent(++_moveSeq, new ZoneVec2(dirX, dirY));
        SendMessage(ZoneWireMessageType.MoveIntent, intent);
    }

    /// <summary>
    /// 发送消息到服务器（使用 ZoneWireCodec 编码）。
    /// </summary>
    private void SendMessage<T>(ZoneWireMessageType type, T payload)
    {
        // Why: 复用服务端 ZoneWireCodec 编码格式。
        var payloadElement = JsonSerializer.SerializeToElement(payload, JsonOptions);
        var envelope = new ZoneWireEnvelope(type, payloadElement);
        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        // Why: 使用可靠通道发送（ENet Reliable）。
        var packet = default(Packet);
        packet.Create(bytes, PacketFlags.Reliable);
        _peer.Send(0, ref packet);
    }

    /// <summary>
    /// 接收并解码消息。
    /// </summary>
    private void OnReceive(Packet packet)
    {
        var bytes = new byte[packet.Length];
        packet.CopyTo(bytes);

        var json = Encoding.UTF8.GetString(bytes);

        try
        {
            var envelope = JsonSerializer.Deserialize<ZoneWireEnvelope>(json, JsonOptions);
            if (envelope == null)
            {
                Console.WriteLine("[NetworkClient] Failed to deserialize message.");
                return;
            }

            Console.WriteLine($"[NetworkClient] Received: {envelope.Type}");
            MessageReceived?.Invoke(envelope.Type, envelope.Payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NetworkClient] Error parsing message: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_peer.IsSet)
        {
            _peer.DisconnectNow(0);
        }

        _client?.Dispose();
        Library.Deinitialize();
    }
}
