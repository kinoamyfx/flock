using Arcadia.Core.Net.Zone;
using Arcadia.Server.Zone;

namespace Arcadia.Server.Net.Enet;

/// <summary>
/// Zone 意图限流器（按 playerId + 消息类型）。
/// Why: 限制 MoveIntent/拾取/撤离等高频消息，降低刷包与“多次意图叠加”造成的加速/瞬移风险。
/// Context: MVP 无复杂反外挂，先用低成本限流 + 明确日志证据。
/// Attention: 被限流时默认丢弃该条意图（不踢人）；如需封禁/断开应在 Gateway/风控层决策。
/// </summary>
public sealed class ZoneIntentRateLimiter
{
    private sealed class Window
    {
        public DateTimeOffset WindowStartUtc;
        public int MoveCount;
        public int PickupCount;
        public int EvacCount;
        public int DebugCount;
    }

    private readonly Dictionary<ZonePlayerId, Window> _windows = new();
    private readonly TimeSpan _windowSize;

    private readonly int _maxMovePerWindow;
    private readonly int _maxPickupPerWindow;
    private readonly int _maxEvacPerWindow;
    private readonly int _maxDebugPerWindow;

    public ZoneIntentRateLimiter(
        TimeSpan? windowSize = null,
        int maxMovePerWindow = 60,
        int maxPickupPerWindow = 6,
        int maxEvacPerWindow = 3,
        int maxDebugPerWindow = 1)
    {
        _windowSize = windowSize ?? TimeSpan.FromSeconds(1);
        _maxMovePerWindow = maxMovePerWindow;
        _maxPickupPerWindow = maxPickupPerWindow;
        _maxEvacPerWindow = maxEvacPerWindow;
        _maxDebugPerWindow = maxDebugPerWindow;
    }

    public bool TryConsume(ZonePlayerId playerId, ZoneWireMessageType type, DateTimeOffset nowUtc, out string reason)
    {
        reason = string.Empty;

        if (!_windows.TryGetValue(playerId, out var w))
        {
            w = new Window { WindowStartUtc = nowUtc };
            _windows[playerId] = w;
        }

        if (nowUtc - w.WindowStartUtc >= _windowSize)
        {
            w.WindowStartUtc = nowUtc;
            w.MoveCount = 0;
            w.PickupCount = 0;
            w.EvacCount = 0;
            w.DebugCount = 0;
        }

        switch (type)
        {
            case ZoneWireMessageType.MoveIntent:
                w.MoveCount++;
                if (w.MoveCount > _maxMovePerWindow)
                {
                    reason = $"move_rate_exceeded:{w.MoveCount}>{_maxMovePerWindow}";
                    return false;
                }

                return true;

            case ZoneWireMessageType.PickupIntent:
                w.PickupCount++;
                if (w.PickupCount > _maxPickupPerWindow)
                {
                    reason = $"pickup_rate_exceeded:{w.PickupCount}>{_maxPickupPerWindow}";
                    return false;
                }

                return true;

            case ZoneWireMessageType.EvacIntent:
                w.EvacCount++;
                if (w.EvacCount > _maxEvacPerWindow)
                {
                    reason = $"evac_rate_exceeded:{w.EvacCount}>{_maxEvacPerWindow}";
                    return false;
                }

                return true;

            case ZoneWireMessageType.DebugKillSelf:
                w.DebugCount++;
                if (w.DebugCount > _maxDebugPerWindow)
                {
                    reason = $"debug_rate_exceeded:{w.DebugCount}>{_maxDebugPerWindow}";
                    return false;
                }

                return true;

            default:
                // Why: 其它消息通常是服务端→客户端或低频握手，暂不计入限流。
                return true;
        }
    }

    public void DropPlayer(ZonePlayerId playerId)
    {
        _windows.Remove(playerId);
    }
}

