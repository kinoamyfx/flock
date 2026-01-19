using Arcadia.Core.Aoi;
using Arcadia.Core.Ecs;
using Arcadia.Core.Logging;
using Arcadia.Core.World;

namespace Arcadia.Server.Systems;

/// <summary>
/// AOI 更新系统（Grid-based）。
/// Why: 每个 tick 更新实体位置到 AOI 索引，为消息广播提供可见性过滤。
/// Context: 只更新有 Position 组件的实体（玩家/怪物/掉落物等）；静态物体不参与 AOI。
/// Attention: AOI 更新开销应 < 5ms（64 玩家基线）；若超标需优化为"仅移动时更新"或"降频更新"。
/// </summary>
public sealed class AoiUpdateSystem
{
    private readonly GridAoi _aoi;

    public AoiUpdateSystem(GridAoi aoi)
    {
        _aoi = aoi;
    }

    public void Execute(World world, long tick)
    {
        // Why: 遍历所有有 Position 组件的实体，更新到 AOI 索引。
        // Context: 当前 ECS 没有 Query API，暂用 placeholder；后续补齐 world.Query<Position>()。
        // Attention: 若实体数量 > 1000，需要优化为增量更新（仅移动的实体）。

        // TODO: 实现 world.Query<Position>() 后替换此 placeholder
        // foreach (var (entity, pos) in world.Query<Position>())
        // {
        //     _aoi.UpdatePosition(entity, pos.X, pos.Y);
        // }

        ArcadiaLog.Info(
            nameof(AoiUpdateSystem),
            nameof(Execute),
            "UpdateAoi",
            ("Tick", tick));
    }
}
