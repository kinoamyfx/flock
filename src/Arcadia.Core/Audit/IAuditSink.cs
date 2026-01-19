namespace Arcadia.Core.Audit;

/// <summary>
/// 审计事件落地接口。
/// Why: 满足“Kill→Drop→Loot 可还原”的硬要求，且不与具体存储（Postgres/文件/ELK）绑定。
/// Context: 服务器权威判定必须可审计，方便追责/反作弊/纠纷处理。
/// Attention: MVP 可以先落文件/控制台；上线必须落 Postgres 并接入告警与查询。
/// </summary>
public interface IAuditSink
{
    void Record(AuditEvent evt);
}

