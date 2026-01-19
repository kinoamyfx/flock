namespace Arcadia.Core.Audit;

public sealed record AuditEvent(
    string EventType,
    DateTimeOffset AtUtc,
    IReadOnlyDictionary<string, string> Fields
);

