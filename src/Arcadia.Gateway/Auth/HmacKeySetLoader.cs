using Arcadia.Core.Auth;

namespace Arcadia.Gateway.Auth;

public static class HmacKeySetLoader
{
    public static HmacKeySet LoadFromEnv()
    {
        // Format: ARCADIA_AUTH_KEYS="kid1=secret1;kid2=secret2"
        var raw = Environment.GetEnvironmentVariable("ARCADIA_AUTH_KEYS");
        var activeKid = Environment.GetEnvironmentVariable("ARCADIA_AUTH_ACTIVE_KID");

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException("Missing env ARCADIA_AUTH_KEYS (format: kid1=secret1;kid2=secret2).");
        }

        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var part in raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var kv = part.Split('=', 2, StringSplitOptions.TrimEntries);
            if (kv.Length != 2 || string.IsNullOrWhiteSpace(kv[0]) || string.IsNullOrWhiteSpace(kv[1]))
            {
                throw new InvalidOperationException("Invalid ARCADIA_AUTH_KEYS entry; expected kid=secret.");
            }

            map[kv[0]] = kv[1];
        }

        if (map.Count == 0)
        {
            throw new InvalidOperationException("ARCADIA_AUTH_KEYS is empty.");
        }

        if (string.IsNullOrWhiteSpace(activeKid))
        {
            activeKid = map.Keys.First();
        }

        if (!map.ContainsKey(activeKid))
        {
            throw new InvalidOperationException("ARCADIA_AUTH_ACTIVE_KID not found in ARCADIA_AUTH_KEYS.");
        }

        return new HmacKeySet(activeKid, map);
    }
}

