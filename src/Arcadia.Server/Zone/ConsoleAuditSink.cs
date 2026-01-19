using Arcadia.Core.Audit;
using Arcadia.Core.Logging;

namespace Arcadia.Server.Zone;

public sealed class ConsoleAuditSink : IAuditSink
{
    public void Record(AuditEvent evt)
    {
        ArcadiaLog.Info(
            nameof(ConsoleAuditSink),
            nameof(Record),
            evt.EventType,
            ("AtUtc", evt.AtUtc.ToString("O")),
            ("Fields", string.Join(",", evt.Fields.Select(kv => $"{kv.Key}={kv.Value}"))));
    }
}

