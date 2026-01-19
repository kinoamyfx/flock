using Arcadia.Core.Logging;

namespace Arcadia.Core.Timing;

public sealed class FixedTickLoop
{
    private readonly TimeSpan _tickInterval;

    public FixedTickLoop(int tickHz)
    {
        if (tickHz <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tickHz));
        }

        _tickInterval = TimeSpan.FromSeconds(1.0 / tickHz);
    }

    public async Task RunAsync(Func<long, Task> onTickAsync, CancellationToken cancellationToken)
    {
        // Why: 权威服务端需要固定 tick 节奏，避免因帧率抖动导致判定不一致。
        // Context: 动作战斗与拾取/掉落需要可回放与可审计。
        // Attention: 若 tick 超时，应记录慢 tick 并允许降载/分线扩容，而不是 silently drift。
        var tick = 0L;
        var next = DateTimeOffset.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            tick++;
            var startedAt = DateTimeOffset.UtcNow;
            try
            {
                await onTickAsync(tick);
            }
            catch (Exception ex)
            {
                ArcadiaLog.Error(nameof(FixedTickLoop), nameof(RunAsync), "TickException", ex, ("Tick", tick));
            }

            next = next.Add(_tickInterval);
            var delay = next - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
                continue;
            }

            ArcadiaLog.Info(
                nameof(FixedTickLoop),
                nameof(RunAsync),
                "TickOverrun",
                ("Tick", tick),
                ("ElapsedMs", (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds),
                ("OverrunMs", (-delay).TotalMilliseconds));

            next = DateTimeOffset.UtcNow;
        }
    }
}

