using System.Text.Json;
using Arcadia.Core.Audit;
using Arcadia.Core.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Arcadia.Server.Persistence.Postgres;

public sealed class PostgresAuditSink : IAuditSink
{
    private readonly PostgresDatabase _db;

    public PostgresAuditSink(PostgresDatabase db)
    {
        _db = db;
    }

    public void Record(AuditEvent evt)
    {
        // Why: IAuditSink 是同步接口；这里用 sync-over-async 的最小实现保证调用方简单。
        // Context: MVP 先把审计链落地，后续可改为队列异步批量写入提升性能。
        // Attention: 如果出现数据库抖动，必须保证服务端权威逻辑不被阻塞；后续要加缓冲与降级。
        RecordAsync(evt, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task RecordAsync(AuditEvent evt, CancellationToken cancellationToken)
    {
        var fieldsJson = JsonSerializer.Serialize(evt.Fields);

        const string sql = """
                           insert into audit_events(event_id, event_type, at_utc, fields)
                           values (@event_id, @event_type, @at_utc, @fields::jsonb);
                           """;

        try
        {
            await using var conn = await _db.OpenAsync(cancellationToken);
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("event_id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("event_type", evt.EventType);
            cmd.Parameters.AddWithValue("at_utc", evt.AtUtc.UtcDateTime);
            cmd.Parameters.Add(new NpgsqlParameter("fields", NpgsqlDbType.Jsonb) { Value = fieldsJson });

            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ArcadiaLog.Error(
                nameof(PostgresAuditSink),
                nameof(RecordAsync),
                "PersistAuditFailed",
                ex,
                ("EventType", evt.EventType),
                ("AtUtc", evt.AtUtc.ToString("O")));
            throw;
        }
    }
}

