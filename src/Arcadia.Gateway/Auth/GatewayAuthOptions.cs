namespace Arcadia.Gateway.Auth;

public sealed record GatewayAuthOptions(
    string DevIssueKey,
    int TokenTtlSeconds
);

