using Arcadia.Core.Aoi;
using Arcadia.Mdk.Ecs;

namespace Arcadia.Tests;

public sealed class GridAoiTests
{
    [Fact]
    public void UpdatePosition_ShouldAddEntityToGrid()
    {
        var aoi = new GridAoi(gridSize: 64f);
        var entity = new EntityId(1);

        aoi.UpdatePosition(entity, 10f, 20f);

        var visible = aoi.QueryNineGrid(10f, 20f);
        Assert.Contains(entity, visible);
    }

    [Fact]
    public void UpdatePosition_CrossGrid_ShouldMigrateEntity()
    {
        var aoi = new GridAoi(gridSize: 64f);
        var entity = new EntityId(1);

        aoi.UpdatePosition(entity, 10f, 10f);   // Grid (0, 0)
        aoi.UpdatePosition(entity, 300f, 300f); // Grid (4, 4) - 远超九宫格范围

        var visibleAtOld = aoi.QueryNineGrid(10f, 10f);
        Assert.DoesNotContain(entity, visibleAtOld); // 不再可见于旧格

        var visibleAtNew = aoi.QueryNineGrid(300f, 300f);
        Assert.Contains(entity, visibleAtNew); // 可见于新格
    }

    [Fact]
    public void Remove_ShouldRemoveEntityFromGrid()
    {
        var aoi = new GridAoi(gridSize: 64f);
        var entity = new EntityId(1);

        aoi.UpdatePosition(entity, 10f, 20f);
        aoi.Remove(entity);

        var visible = aoi.QueryNineGrid(10f, 20f);
        Assert.DoesNotContain(entity, visible);
    }

    [Fact]
    public void QueryNineGrid_ShouldIncludeAdjacentGrids()
    {
        var aoi = new GridAoi(gridSize: 64f);
        var center = new EntityId(1);
        var adjacent = new EntityId(2);
        var farAway = new EntityId(3);

        aoi.UpdatePosition(center, 64f, 64f);   // Grid (1, 1)
        aoi.UpdatePosition(adjacent, 10f, 10f); // Grid (0, 0) - 九宫格内
        aoi.UpdatePosition(farAway, 300f, 300f); // Grid (4, 4) - 九宫格外

        var visible = aoi.QueryNineGrid(64f, 64f);
        Assert.Contains(center, visible);
        Assert.Contains(adjacent, visible);
        Assert.DoesNotContain(farAway, visible);
    }

    [Fact]
    public void GetBroadcastTargets_ShouldExcludeSelf()
    {
        var aoi = new GridAoi(gridSize: 64f);
        var self = new EntityId(1);
        var other = new EntityId(2);

        aoi.UpdatePosition(self, 10f, 10f);
        aoi.UpdatePosition(other, 20f, 20f);

        var targets = aoi.GetBroadcastTargets(10f, 10f, excludeSelf: self);
        Assert.DoesNotContain(self, targets);
        Assert.Contains(other, targets);
    }

    [Fact]
    public void GridBoundary_ShouldHandleNegativeCoordinates()
    {
        var aoi = new GridAoi(gridSize: 64f);
        var entity = new EntityId(1);

        aoi.UpdatePosition(entity, -10f, -20f); // Grid (-1, -1)

        var visible = aoi.QueryNineGrid(-10f, -20f);
        Assert.Contains(entity, visible);
    }
}
