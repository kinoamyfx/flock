namespace Arcadia.Core.Auth;

public sealed class HmacKeySet
{
    public HmacKeySet(string activeKid, IReadOnlyDictionary<string, string> keys)
    {
        if (string.IsNullOrWhiteSpace(activeKid))
        {
            throw new ArgumentException("activeKid is required.", nameof(activeKid));
        }

        ActiveKid = activeKid;
        Keys = keys ?? throw new ArgumentNullException(nameof(keys));
    }

    public string ActiveKid { get; }
    public IReadOnlyDictionary<string, string> Keys { get; }

    public string? ResolveSecret(string kid) => Keys.TryGetValue(kid, out var secret) ? secret : null;
}

