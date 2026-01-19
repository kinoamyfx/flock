using System.Net.Http.Json;

namespace Arcadia.LoadTest.Gateway;

public sealed class GatewayTokenClient
{
    private readonly HttpClient _http;
    private readonly string _issueKey;

    public GatewayTokenClient(HttpClient http, string issueKey)
    {
        _http = http;
        _issueKey = issueKey;
    }

    public sealed record IssueTokenResponse(string Token, long ExpiresAtUnixSeconds, string Kid);

    private sealed record IssueTokenRequest(string PlayerId);

    public async Task<IssueTokenResponse> IssueTokenAsync(string playerId, CancellationToken cancellationToken)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "/auth/token");
        req.Headers.Add("X-Arcadia-Issue-Key", _issueKey);
        req.Content = JsonContent.Create(new IssueTokenRequest(playerId));

        using var resp = await _http.SendAsync(req, cancellationToken);
        resp.EnsureSuccessStatusCode();
        var parsed = await resp.Content.ReadFromJsonAsync<IssueTokenResponse>(cancellationToken: cancellationToken);
        return parsed ?? throw new InvalidOperationException("Empty response.");
    }
}
