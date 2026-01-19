using Arcadia.Core.Net.Zone;
using Arcadia.Core.World;

namespace Arcadia.Server.Zone;

public static class ZoneMovement
{
    public static bool TryApplyMove(
        Position current,
        ZoneMoveIntent intent,
        float moveSpeed,
        int tickHz,
        out Position next,
        out string reason)
    {
        next = current;
        reason = string.Empty;

        if (tickHz <= 0)
        {
            reason = "invalid_tick_hz";
            return false;
        }

        if (moveSpeed <= 0 || float.IsNaN(moveSpeed) || float.IsInfinity(moveSpeed))
        {
            reason = "invalid_move_speed";
            return false;
        }

        var dirX = intent.Dir.X;
        var dirY = intent.Dir.Y;

        // Why: 防止 NaN/Infinity 污染位置状态（会导致 AOI/渲染/持久化全部异常）。
        // Context: 恶意客户端可能构造非法 float 破坏服务端状态。
        // Attention: 一旦出现非法数据，必须拒绝并记录（但 MVP 不直接踢人）。
        if (float.IsNaN(dirX) || float.IsNaN(dirY) || float.IsInfinity(dirX) || float.IsInfinity(dirY))
        {
            reason = "invalid_dir";
            return false;
        }

        var length = MathF.Sqrt(dirX * dirX + dirY * dirY);
        if (length <= 0)
        {
            return true;
        }

        if (length > 1.0f)
        {
            dirX /= length;
            dirY /= length;
        }

        var dt = 1.0f / tickHz;
        var newX = current.X + dirX * moveSpeed * dt;
        var newY = current.Y + dirY * moveSpeed * dt;

        if (float.IsNaN(newX) || float.IsNaN(newY) || float.IsInfinity(newX) || float.IsInfinity(newY))
        {
            reason = "invalid_position";
            return false;
        }

        next = new Position(newX, newY);
        return true;
    }
}

