using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Arcadia.Core.Auth;

public static class HmacTokenCodec
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string Create(AuthTokenPayload payload, string kid, string secret)
    {
        if (string.IsNullOrWhiteSpace(kid))
        {
            throw new ArgumentException("kid is required", nameof(kid));
        }

        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException("secret is required", nameof(secret));
        }

        if (string.IsNullOrWhiteSpace(payload.PlayerId))
        {
            throw new ArgumentException("payload.PlayerId is required", nameof(payload));
        }

        // Why: MVP 模拟 Gateway token，下发给客户端后用于 Zone 握手鉴权。
        // Context: 防止客户端伪造 playerId 劫持重连与掉落链路。
        // Attention: 该 token 仅用于鉴权与绑定身份；具体权限（封禁、白名单、风控）在 Gateway 层实现。
        var headerJson = JsonSerializer.Serialize(new { alg = "HS256", typ = "ARCADIA", kid }, JsonOptions);
        var payloadJson = JsonSerializer.Serialize(payload, JsonOptions);

        var headerB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        var payloadB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var signingInput = $"{headerB64}.{payloadB64}";
        var sig = ComputeHmacSha256(secret, signingInput);
        var sigB64 = Base64UrlEncode(sig);

        return $"{signingInput}.{sigB64}";
    }

    public static bool TryValidate(
        string token,
        Func<string, string?> resolveSecretByKid,
        DateTimeOffset nowUtc,
        out AuthTokenPayload payload,
        out string errorCode)
    {
        payload = default!;
        errorCode = string.Empty;

        if (string.IsNullOrWhiteSpace(token))
        {
            errorCode = "missing_token";
            return false;
        }

        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            errorCode = "invalid_format";
            return false;
        }

        string kid;
        try
        {
            var headerJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[0]));
            using var doc = JsonDocument.Parse(headerJson);
            if (!doc.RootElement.TryGetProperty("kid", out var kidProp))
            {
                errorCode = "missing_kid";
                return false;
            }

            kid = kidProp.GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(kid))
            {
                errorCode = "missing_kid";
                return false;
            }
        }
        catch
        {
            errorCode = "invalid_header";
            return false;
        }

        var secret = resolveSecretByKid(kid);
        if (string.IsNullOrWhiteSpace(secret))
        {
            errorCode = "unknown_kid";
            return false;
        }

        var signingInput = $"{parts[0]}.{parts[1]}";
        var expectedSig = ComputeHmacSha256(secret, signingInput);
        if (!TimingSafeEquals(expectedSig, Base64UrlDecode(parts[2])))
        {
            errorCode = "invalid_signature";
            return false;
        }

        try
        {
            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            var decoded = JsonSerializer.Deserialize<AuthTokenPayload>(payloadJson, JsonOptions);
            if (decoded is null || string.IsNullOrWhiteSpace(decoded.PlayerId))
            {
                errorCode = "invalid_payload";
                return false;
            }

            var now = nowUtc.ToUnixTimeSeconds();
            if (decoded.ExpiresAtUnixSeconds < now)
            {
                errorCode = "expired";
                return false;
            }

            payload = decoded;
            return true;
        }
        catch
        {
            errorCode = "invalid_payload";
            return false;
        }
    }

    private static byte[] ComputeHmacSha256(string secret, string signingInput)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        using var h = new HMACSHA256(keyBytes);
        return h.ComputeHash(Encoding.ASCII.GetBytes(signingInput));
    }

    private static bool TimingSafeEquals(byte[] a, byte[] b)
    {
        // Why: 防止时序侧信道暴露签名信息。
        // Context: token 校验是高频路径；实现应简单且稳定。
        // Attention: 长度不一致也要走完整比较，避免长度泄漏。
        var max = Math.Max(a.Length, b.Length);
        var diff = 0;
        for (var i = 0; i < max; i++)
        {
            var av = i < a.Length ? a[i] : (byte)0;
            var bv = i < b.Length ? b[i] : (byte)0;
            diff |= av ^ bv;
        }

        return diff == 0 && a.Length == b.Length;
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string s)
    {
        var padded = s.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2:
                padded += "==";
                break;
            case 3:
                padded += "=";
                break;
        }

        return Convert.FromBase64String(padded);
    }
}
