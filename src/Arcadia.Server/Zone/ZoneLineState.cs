namespace Arcadia.Server.Zone;

public sealed class ZoneLineState
{
    public ZoneLineState(ZoneLineId lineId)
    {
        LineId = lineId;
    }

    public ZoneLineId LineId { get; }

    /// <summary>
    /// 线重置版本号。
    /// Why: 用一个单调递增版本号表达“是否发生过 reset”，重连只需对比版本即可判定入口恢复。
    /// Context: 老板确认规则：重连时若秘境已重置 => 入口恢复。
    /// Attention: 版本号必须只由服务端推进，不能从客户端输入。
    /// </summary>
    public long ResetVersion { get; private set; } = 0;

    public void Reset()
    {
        ResetVersion++;
    }
}

