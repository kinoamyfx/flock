using Arcadia.Server.Zone;

var options = new ZoneServerOptions(
    TickHz: int.TryParse(Environment.GetEnvironmentVariable("ARCADIA_TICK_HZ"), out var hz) ? hz : 30,
    LineSoftCap: int.TryParse(Environment.GetEnvironmentVariable("ARCADIA_LINE_SOFT_CAP"), out var cap) ? cap : 64
);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

await new ZoneServerHost(options).RunAsync(cts.Token);
