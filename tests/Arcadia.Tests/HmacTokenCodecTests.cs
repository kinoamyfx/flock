using Arcadia.Core.Auth;

namespace Arcadia.Tests;

public sealed class HmacTokenCodecTests
{
    [Fact]
    public void Validate_ValidToken_ShouldSucceed()
    {
        var secret = "secret";
        var kid = "k1";
        var now = DateTimeOffset.UtcNow;
        var token = HmacTokenCodec.Create(
            new AuthTokenPayload(
                PlayerId: "p1",
                IssuedAtUnixSeconds: now.ToUnixTimeSeconds(),
                ExpiresAtUnixSeconds: now.AddMinutes(5).ToUnixTimeSeconds(),
                Nonce: "n1"),
            kid,
            secret);

        Assert.True(HmacTokenCodec.TryValidate(token, k => k == kid ? secret : null, now, out var payload, out var error));
        Assert.Equal(string.Empty, error);
        Assert.Equal("p1", payload.PlayerId);
    }

    [Fact]
    public void Validate_TamperedToken_ShouldFail()
    {
        var secret = "secret";
        var kid = "k1";
        var now = DateTimeOffset.UtcNow;
        var token = HmacTokenCodec.Create(
            new AuthTokenPayload(
                PlayerId: "p1",
                IssuedAtUnixSeconds: now.ToUnixTimeSeconds(),
                ExpiresAtUnixSeconds: now.AddMinutes(5).ToUnixTimeSeconds(),
                Nonce: "n1"),
            kid,
            secret);

        // 篡改 payload 段的一个字符（保持结构不变，确保签名校验会失败）
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
        var payloadB64 = parts[1];
        var last = payloadB64[^1];
        var flipped = last == 'A' ? 'B' : 'A';
        var tampered = $"{parts[0]}.{payloadB64[..^1]}{flipped}.{parts[2]}";
        Assert.False(HmacTokenCodec.TryValidate(tampered, k => k == kid ? secret : null, now, out _, out var error));
        Assert.Equal("invalid_signature", error);
    }

    [Fact]
    public void Validate_ExpiredToken_ShouldFail()
    {
        var secret = "secret";
        var kid = "k1";
        var now = DateTimeOffset.UtcNow;
        var token = HmacTokenCodec.Create(
            new AuthTokenPayload(
                PlayerId: "p1",
                IssuedAtUnixSeconds: now.AddMinutes(-10).ToUnixTimeSeconds(),
                ExpiresAtUnixSeconds: now.AddMinutes(-1).ToUnixTimeSeconds(),
                Nonce: "n1"),
            kid,
            secret);

        Assert.False(HmacTokenCodec.TryValidate(token, k => k == kid ? secret : null, now, out _, out var error));
        Assert.Equal("expired", error);
    }
}
