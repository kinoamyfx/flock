namespace Arcadia.Gateway.Auth;

public sealed record IssueTokenResponse(
    string Token,
    long ExpiresAtUnixSeconds,
    string Kid
);

