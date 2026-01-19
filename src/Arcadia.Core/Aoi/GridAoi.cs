using Arcadia.Mdk.Ecs;

namespace Arcadia.Core.Aoi;

/// <summary>
/// Grid-based Area-of-Interest 管理器（九宫格）。
/// Why: 为 64 人/线动作战斗提供可见性过滤，减少不必要的消息广播与带宽开销。
/// Context: 秘境场景玩家相对分散，Grid-based 简单高效；后续可演进为 Cross-list 或 Quad-tree。
/// Attention: Grid 尺寸需权衡：太小导致频繁跨格，太大导致九宫格范围过大。推荐 32-64 单位/格。
/// </summary>
public sealed class GridAoi
{
    private readonly float _gridSize;
    private readonly Dictionary<GridCoord, HashSet<EntityId>> _grid = new();
    private readonly Dictionary<EntityId, GridCoord> _entityToGrid = new();

    public GridAoi(float gridSize = 64f)
    {
        // Why: 64 单位/格，假设玩家视野半径约 128-192 单位，九宫格覆盖 192x192 范围，足够且不过度。
        // Context: 后续可通过配置或动态调整（例如秘境大小/玩家密度）。
        // Attention: 修改 gridSize 需同步压测验证性能与可见性体验。
        _gridSize = gridSize;
    }

    /// <summary>
    /// 更新实体位置（跨格时自动迁移）。
    /// </summary>
    public void UpdatePosition(EntityId entityId, float x, float y)
    {
        var newCoord = ToGridCoord(x, y);

        if (_entityToGrid.TryGetValue(entityId, out var oldCoord))
        {
            if (oldCoord == newCoord)
            {
                return; // 未跨格，无需更新
            }

            // 跨格：从旧格移除
            if (_grid.TryGetValue(oldCoord, out var oldBucket))
            {
                oldBucket.Remove(entityId);
                if (oldBucket.Count == 0)
                {
                    _grid.Remove(oldCoord);
                }
            }
        }

        // 加入新格
        if (!_grid.TryGetValue(newCoord, out var newBucket))
        {
            newBucket = new HashSet<EntityId>();
            _grid[newCoord] = newBucket;
        }

        newBucket.Add(entityId);
        _entityToGrid[entityId] = newCoord;
    }

    /// <summary>
    /// 移除实体（断线/离开秘境）。
    /// </summary>
    public void Remove(EntityId entityId)
    {
        if (!_entityToGrid.TryGetValue(entityId, out var coord))
        {
            return;
        }

        if (_grid.TryGetValue(coord, out var bucket))
        {
            bucket.Remove(entityId);
            if (bucket.Count == 0)
            {
                _grid.Remove(coord);
            }
        }

        _entityToGrid.Remove(entityId);
    }

    /// <summary>
    /// 查询九宫格范围内的所有实体（可见性范围）。
    /// </summary>
    public IReadOnlyCollection<EntityId> QueryNineGrid(float x, float y)
    {
        var center = ToGridCoord(x, y);
        var result = new HashSet<EntityId>();

        // Why: 九宫格（3x3）覆盖中心格及其周围 8 格，确保视野范围内的实体都可见。
        // Context: 若玩家视野半径 > gridSize * 1.5，可能需要扩展为 25 宫格（5x5）。
        // Attention: 九宫格查询复杂度 O(9 * N_per_grid)，N_per_grid 应 < 10 以保证性能。
        for (var dx = -1; dx <= 1; dx++)
        {
            for (var dy = -1; dy <= 1; dy++)
            {
                var coord = new GridCoord(center.X + dx, center.Y + dy);
                if (_grid.TryGetValue(coord, out var bucket))
                {
                    result.UnionWith(bucket);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 广播消息到九宫格范围内的所有实体（AOI 过滤广播）。
    /// </summary>
    public IReadOnlyCollection<EntityId> GetBroadcastTargets(float x, float y, EntityId? excludeSelf = null)
    {
        var targets = QueryNineGrid(x, y);
        if (excludeSelf.HasValue)
        {
            return targets.Where(e => e != excludeSelf.Value).ToList();
        }

        return targets;
    }

    private GridCoord ToGridCoord(float x, float y)
    {
        return new GridCoord(
            (int)Math.Floor(x / _gridSize),
            (int)Math.Floor(y / _gridSize));
    }
}

/// <summary>
/// Grid 坐标（2D 整数坐标）。
/// </summary>
public readonly record struct GridCoord(int X, int Y);
