using Arcadia.Core.World;

namespace Arcadia.Tests;

public sealed class ChunkLoadTrackerTests
{
    [Fact]
    public void PlayerEnterLeave_ShouldActivateDeactivateChunk()
    {
        var tracker = new ChunkLoadTracker();
        var c = new ChunkCoord(1, 2);

        Assert.False(tracker.IsLoaded(c));

        tracker.AddPlayerLoader(c);
        Assert.True(tracker.IsLoaded(c));

        tracker.RemovePlayerLoader(c);
        Assert.False(tracker.IsLoaded(c));
    }

    [Fact]
    public void Anchor_ShouldKeepChunkLoaded_UntilExpired()
    {
        var tracker = new ChunkLoadTracker();
        var c = new ChunkCoord(0, 0);

        var now = DateTimeOffset.UtcNow;
        _ = tracker.ActivateAnchor(new[] { c }, expiresAtUtc: now.AddSeconds(5));

        Assert.True(tracker.IsLoaded(c));

        var expiredCount = tracker.ExpireAnchors(now.AddSeconds(6));
        Assert.Equal(1, expiredCount);
        Assert.False(tracker.IsLoaded(c));
    }
}

