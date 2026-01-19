using Arcadia.Core.Net.Zone;
using Arcadia.Server.Net.Enet;
using Arcadia.Server.Zone;

namespace Arcadia.Tests;

public sealed class ZoneIntentRateLimiterTests
{
    [Fact]
    public void Limits_move_intents_within_window()
    {
        var limiter = new ZoneIntentRateLimiter(windowSize: TimeSpan.FromSeconds(1), maxMovePerWindow: 3);
        var playerId = new ZonePlayerId("p1");
        var now = DateTimeOffset.UtcNow;

        Assert.True(limiter.TryConsume(playerId, ZoneWireMessageType.MoveIntent, now, out _));
        Assert.True(limiter.TryConsume(playerId, ZoneWireMessageType.MoveIntent, now, out _));
        Assert.True(limiter.TryConsume(playerId, ZoneWireMessageType.MoveIntent, now, out _));

        Assert.False(limiter.TryConsume(playerId, ZoneWireMessageType.MoveIntent, now, out var reason));
        Assert.Contains("move_rate_exceeded", reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Resets_window_after_window_size()
    {
        var limiter = new ZoneIntentRateLimiter(windowSize: TimeSpan.FromSeconds(1), maxPickupPerWindow: 1);
        var playerId = new ZonePlayerId("p1");
        var t0 = DateTimeOffset.UtcNow;

        Assert.True(limiter.TryConsume(playerId, ZoneWireMessageType.PickupIntent, t0, out _));
        Assert.False(limiter.TryConsume(playerId, ZoneWireMessageType.PickupIntent, t0, out _));

        Assert.True(limiter.TryConsume(playerId, ZoneWireMessageType.PickupIntent, t0.AddSeconds(2), out _));
    }
}

