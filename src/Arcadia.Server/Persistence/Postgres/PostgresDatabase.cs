using Arcadia.Core.Logging;
using Npgsql;

namespace Arcadia.Server.Persistence.Postgres;

public sealed class PostgresDatabase
{
    private readonly string _connectionString;

    public PostgresDatabase(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("PostgreSQL connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public async Task<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        return conn;
    }

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        // Why: MVP 阶段先用 idempotent DDL 拉起最小表结构，避免引入迁移工具导致复杂度飙升。
        // Context: 你已确认必须最大限度防作弊；权威落库与审计链必须先可用。
        // Attention: 正式环境应替换为版本化迁移（例如 Flyway/DbUp 等），本方法只用于 MVP。
        const string sql = """
                           create table if not exists audit_events (
                             event_id uuid primary key,
                             event_type text not null,
                             at_utc timestamptz not null,
                             fields jsonb not null
                           );

                           create index if not exists idx_audit_events_type_time on audit_events(event_type, at_utc desc);

                           create table if not exists loot_containers (
                             loot_id uuid primary key,
                             created_at_utc timestamptz not null
                           );

                           create table if not exists item_instances (
                             item_id uuid primary key,
                             template_id text not null,
                             quantity int not null,
                             owner_kind smallint not null, -- 0=player, 1=loot
                             owner_id text not null,       -- playerId or lootId (uuid as text)
                             slot_kind smallint not null   -- 0=carried, 1=safe, 2=loot
                           );

                           create index if not exists idx_item_owner on item_instances(owner_kind, owner_id);
                           """;

        try
        {
            await using var conn = await OpenAsync(cancellationToken);
            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            ArcadiaLog.Info(nameof(PostgresDatabase), nameof(EnsureSchemaAsync), "SchemaReady");
        }
        catch (Exception ex)
        {
            ArcadiaLog.Error(nameof(PostgresDatabase), nameof(EnsureSchemaAsync), "SchemaError", ex);
            throw;
        }
    }
}
