using System.Threading;

namespace Arcadia.Core.Logging;

public static class ArcadiaLogContext
{
    // Why: 统一跨模块关联（client ↔ zone ↔ persistence）的最小机制，避免到处手动传参。
    // Context: MVP 先用 CorrelationId 串联日志；后续可升级为 Activity/traceparent。
    // Attention: 严禁把 token/密钥作为 CorrelationId（敏感信息），默认用 playerId/connId。
    private static readonly AsyncLocal<string?> CorrelationIdLocal = new();

    public static string? CorrelationId
    {
        get => CorrelationIdLocal.Value;
        set => CorrelationIdLocal.Value = value;
    }

    public static IDisposable BeginCorrelation(string correlationId)
    {
        var previous = CorrelationIdLocal.Value;
        CorrelationIdLocal.Value = correlationId;
        return new Scope(previous);
    }

    private sealed class Scope : IDisposable
    {
        private readonly string? _previous;
        private bool _disposed;

        public Scope(string? previous)
        {
            _previous = previous;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            CorrelationIdLocal.Value = _previous;
        }
    }
}

