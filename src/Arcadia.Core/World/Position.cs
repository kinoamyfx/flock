namespace Arcadia.Core.World;

/// <summary>
/// 2D 位置组件（世界坐标）。
/// Why: 权威位置由服务端管理，客户端插值渲染；AOI 基于位置过滤可见性。
/// Context: 秘境采用 2D Top-down 视角，Z 轴用于分层渲染（不影响 AOI）。
/// Attention: 位置更新必须先验证合法性（速度/传送/碰撞），避免作弊传送。
/// </summary>
public struct Position
{
    public float X { get; set; }
    public float Y { get; set; }

    public Position(float x, float y)
    {
        X = x;
        Y = y;
    }
}
