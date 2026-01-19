namespace Arcadia.Core.Auth;

public sealed record AuthTokenPayload(
    string PlayerId,
    long IssuedAtUnixSeconds,
    long ExpiresAtUnixSeconds,
    string Nonce
);

