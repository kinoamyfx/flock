using Arcadia.Core.Auth;
using Arcadia.Core.Logging;
using Arcadia.Gateway.Auth;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/", () => "Arcadia.Gateway");

var options = new GatewayAuthOptions(
    DevIssueKey: Environment.GetEnvironmentVariable("ARCADIA_DEV_ISSUE_KEY") ?? string.Empty,
    TokenTtlSeconds: int.TryParse(Environment.GetEnvironmentVariable("ARCADIA_TOKEN_TTL_S"), out var ttl) ? ttl : 600
);

app.MapPost("/auth/token", (HttpRequest request, IssueTokenRequest body) =>
{
    // Why: MVP Gateway 只提供 token 签发能力；客户端不得再持有签名密钥，避免伪造身份劫持重连与掉落链路。
    // Context: 后续会替换为真实账号体系与更严格的鉴权/风控，本端点作为最小可用脚手架。
    // Attention: DevIssueKey 为空时默认拒绝签发，避免误开。
    var issueKey = request.Headers["X-Arcadia-Issue-Key"].ToString();
    if (string.IsNullOrWhiteSpace(options.DevIssueKey) || issueKey != options.DevIssueKey)
    {
        ArcadiaLog.Info(nameof(Program), "POST /auth/token", "Forbidden");
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(body.PlayerId))
    {
        return Results.BadRequest(new { error = "missing_player_id" });
    }

    var keySet = HmacKeySetLoader.LoadFromEnv();
    var now = DateTimeOffset.UtcNow;
    var exp = now.AddSeconds(options.TokenTtlSeconds);

    var payload = new AuthTokenPayload(
        PlayerId: body.PlayerId,
        IssuedAtUnixSeconds: now.ToUnixTimeSeconds(),
        ExpiresAtUnixSeconds: exp.ToUnixTimeSeconds(),
        Nonce: Guid.NewGuid().ToString("N"));

    var secret = keySet.ResolveSecret(keySet.ActiveKid)!;
    var token = HmacTokenCodec.Create(payload, keySet.ActiveKid, secret);

    ArcadiaLog.Info(nameof(Program), "POST /auth/token", "Issued", ("PlayerId", body.PlayerId), ("Kid", keySet.ActiveKid), ("Exp", payload.ExpiresAtUnixSeconds));

    return Results.Ok(new IssueTokenResponse(token, payload.ExpiresAtUnixSeconds, keySet.ActiveKid));
});

app.Run();
