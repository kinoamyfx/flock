namespace Arcadia.Core.Net.Zone;

public sealed record ZoneHello(
    string AuthToken,
    string ClientVersion
);
