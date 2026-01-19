namespace Arcadia.Mdk.Modding;

/// <summary>
/// Mod 的强类型标识。
/// Why: 避免字符串在全链路传递时产生拼写错误与大小写歧义。
/// Context: MDK/Core/Server/Client 都会用它做索引与审计。
/// Attention: 任何持久化/网络传输建议使用 <see cref="Value"/> 的规范化字符串。
/// </summary>
public readonly record struct ModId(string Value)
{
    public override string ToString() => Value;
}

