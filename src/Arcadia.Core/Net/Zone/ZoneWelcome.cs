namespace Arcadia.Core.Net.Zone;

public sealed record ZoneWelcome(
    string InstanceId,
    int LineId,
    long ResetVersion,
    string ReconnectDecision
);

