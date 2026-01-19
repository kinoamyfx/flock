using Arcadia.Core.Net.Zone;
using Arcadia.Core.World;
using Arcadia.Server.Zone;

namespace Arcadia.Tests;

public sealed class ZoneMovementDeterminismTests
{
    [Fact]
    public void Same_inputs_produce_same_positions()
    {
        var tickHz = 30;
        var speed = 100f;
        var start = new Position(0, 0);

        var intents = new[]
        {
            new ZoneMoveIntent(1, new ZoneVec2(1, 0)),
            new ZoneMoveIntent(2, new ZoneVec2(0, 1)),
            new ZoneMoveIntent(3, new ZoneVec2(-1, 0)),
            new ZoneMoveIntent(4, new ZoneVec2(0, -1)),
            new ZoneMoveIntent(5, new ZoneVec2(0.6f, 0.8f))
        };

        var a = ApplyAll(start, intents, speed, tickHz);
        var b = ApplyAll(start, intents, speed, tickHz);

        Assert.Equal(a.X, b.X);
        Assert.Equal(a.Y, b.Y);
    }

    [Fact]
    public void Rejects_invalid_direction()
    {
        var ok = ZoneMovement.TryApplyMove(
            new Position(0, 0),
            new ZoneMoveIntent(1, new ZoneVec2(float.NaN, 0)),
            moveSpeed: 100f,
            tickHz: 30,
            out _,
            out var reason);

        Assert.False(ok);
        Assert.Equal("invalid_dir", reason);
    }

    private static Position ApplyAll(Position start, ZoneMoveIntent[] intents, float speed, int tickHz)
    {
        var pos = start;
        foreach (var intent in intents)
        {
            var ok = ZoneMovement.TryApplyMove(pos, intent, speed, tickHz, out var next, out _);
            Assert.True(ok);
            pos = next;
        }

        return pos;
    }
}

