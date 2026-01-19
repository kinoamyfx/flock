using Arcadia.Client.Net.Enet;
using Arcadia.Core.Logging;
using Arcadia.LoadTest;
using Arcadia.LoadTest.Gateway;

var mode = Environment.GetEnvironmentVariable("ARCADIA_LOADTEST_MODE") ?? "stress";
var host = Environment.GetEnvironmentVariable("ARCADIA_LOADTEST_HOST") ?? "127.0.0.1";
var port = ushort.TryParse(Environment.GetEnvironmentVariable("ARCADIA_LOADTEST_PORT"), out var p) ? p : (ushort)7777;
var clients = int.TryParse(Environment.GetEnvironmentVariable("ARCADIA_LOADTEST_CLIENTS"), out var c) ? c : 64;
var durationSeconds = int.TryParse(Environment.GetEnvironmentVariable("ARCADIA_LOADTEST_DURATION_S"), out var d) ? d : 10;
var prefix = Environment.GetEnvironmentVariable("ARCADIA_LOADTEST_PLAYER_PREFIX") ?? "loadtest";
var gatewayUrl = Environment.GetEnvironmentVariable("ARCADIA_GATEWAY_URL") ?? "http://127.0.0.1:8080";
var tokenMode = Environment.GetEnvironmentVariable("ARCADIA_LOADTEST_TOKEN_MODE") ?? "gateway";
var issueKey = Environment.GetEnvironmentVariable("ARCADIA_DEV_ISSUE_KEY") ?? string.Empty;

// Why: playable-slice 模式用于验收 v1-playable-dungeon-slice（单客户端完整流程）。
// Context: stress 模式用于性能压测（多客户端并发）；playable-slice 用于功能验收（单客户端逻辑闭环）。
// Attention: 两种模式的验收标准不同：stress 看并发能力，playable-slice 看逻辑正确性。
if (string.Equals(mode, "playable-slice", StringComparison.OrdinalIgnoreCase))
{
    if (string.Equals(tokenMode, "gateway", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(issueKey))
    {
        ArcadiaLog.Info(nameof(Program), "Main", "DevIssueKeyMissing");
        return;
    }

    ArcadiaLog.Info(
        nameof(Program),
        "Main",
        "PlayableSliceMode",
        ("Host", host),
        ("Port", port),
        ("PlayerId", $"{prefix}-0"));

    await PlayableSliceTest.RunAsync(
        host,
        port,
        playerId: $"{prefix}-0",
        gatewayUrl,
        issueKey,
        CancellationToken.None);

    return;
}

// Original stress test mode
if (string.Equals(tokenMode, "gateway", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(issueKey))
{
    // Why: 正向压测需要先向 Gateway 获取签发 token；缺少 issueKey 无法完成闭环。
    // Context: MVP 阶段暂不做交互式登录 UI，用 dev issue key 保护签发接口。
    // Attention: 负向冒烟（invalid token）不依赖 issueKey。
    ArcadiaLog.Info(nameof(Program), "Main", "DevIssueKeyMissing");
    return;
}

ArcadiaLog.Info(
    nameof(Program),
    "Main",
    "Start",
    ("Host", host),
    ("Port", port),
    ("Clients", clients),
    ("DurationS", durationSeconds),
    ("TokenMode", tokenMode));

var transports = new List<EnetClientTransport>(clients);
GatewayTokenClient? tokenClient = null;
HttpClient? http = null;
if (string.Equals(tokenMode, "gateway", StringComparison.OrdinalIgnoreCase))
{
    http = new HttpClient { BaseAddress = new Uri(gatewayUrl) };
    tokenClient = new GatewayTokenClient(http, issueKey);
}

try
{
    for (var i = 0; i < clients; i++)
    {
        var playerId = $"{prefix}-{i}";
        // Why: 冒烟需要覆盖 auth failure 路径；通过无效 token 触发 Zone 的鉴权踢出与错误提示。
        // Context: `scripts/smoke_enet.sh` 会先跑 invalid 再跑 gateway，确保正负路径都可验收。
        // Attention: token 只用于握手；后续消息仍需基于 session 绑定的 playerId 做权威校验。
        var token = tokenClient is null
            ? "invalid-token"
            : (await tokenClient.IssueTokenAsync(playerId, CancellationToken.None)).Token;
        var t = new EnetClientTransport();
        t.Connect(host, port, playerId: playerId, authToken: token);
        transports.Add(t);
    }

    var until = DateTimeOffset.UtcNow.AddSeconds(durationSeconds);
    while (DateTimeOffset.UtcNow < until)
    {
        foreach (var t in transports)
        {
            t.PollOnce(serviceTimeoutMs: 0);
        }

        await Task.Delay(15);
    }

    foreach (var t in transports)
    {
        t.Flush();
    }
}
finally
{
    http?.Dispose();
    foreach (var t in transports)
    {
        t.Dispose();
    }
}

ArcadiaLog.Info(nameof(Program), "Main", "Done");
