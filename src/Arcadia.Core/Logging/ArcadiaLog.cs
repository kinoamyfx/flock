using System.Diagnostics;

namespace Arcadia.Core.Logging;

public static class ArcadiaLog
{
    public static void Info(string className, string methodName, string eventName, params (string Key, object? Value)[] kv)
    {
        Write(className, methodName, eventName, null, kv);
    }

    public static void Error(string className, string methodName, string eventName, Exception ex, params (string Key, object? Value)[] kv)
    {
        Write(className, methodName, eventName, ex, kv);
    }

    private static void Write(
        string className,
        string methodName,
        string eventName,
        Exception? ex,
        params (string Key, object? Value)[] kv)
    {
        // Why: 统一日志格式，保证能按一次请求/一次死亡/一次拾取完整还原链路。
        // Context: 服务端权威需要可审计；客户端也需要可诊断网络抖动与回滚纠正。
        // Attention: Value 中严禁直接输出敏感信息（token/密钥）；后续可做脱敏器。
        var parts = new List<string>(capacity: 4 + kv.Length)
        {
            $"{className}|{methodName}|{eventName}"
        };

        var correlationId = ArcadiaLogContext.CorrelationId;
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            parts.Add($"Cid={correlationId}");
        }

        foreach (var (key, value) in kv)
        {
            parts.Add($"{key}={value}");
        }

        if (ex is not null)
        {
            parts.Add($"RootCause={ex.GetType().Name}:{ex.Message}");
            parts.Add($"StackTrace={ex}");
        }

        // Why: 规范要求 `类名|方法名|事件|Key=Value|...`，避免在消息体里混入其它分隔符。
        // Context: 日志平台/采集器通常会附带时间戳与 level；MVP 先保持消息体干净可解析。
        // Attention: 若未来接入结构化日志（Serilog/OTel），需保证 message 格式仍可回放解析。
        var line = string.Join("|", parts);
        Console.WriteLine(line);

        Debug.WriteLine(line);
    }
}
