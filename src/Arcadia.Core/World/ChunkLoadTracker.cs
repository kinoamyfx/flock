using Arcadia.Core.Logging;

namespace Arcadia.Core.World;

/// <summary>
/// Chunk 激活追踪器：玩家进入/离开与世界锚（World Anchor）共同决定哪些 chunk 需要推进 tick。
/// Why: “只在有角色激活的区块才运作”是反作弊与成本控制的关键规则；必须在机芯层可测试、可回放。
/// Context: MVP 先用纯内存状态机跑通语义，后续可把状态持久化或做分布式分片。
/// Attention: 该组件不做资源扣费与支付校验；支付属于更外层的 gameplay/biz 决策。
/// </summary>
public sealed class ChunkLoadTracker
{
    private sealed record Anchor(Guid AnchorId, DateTimeOffset ExpiresAtUtc, IReadOnlyList<ChunkCoord> Chunks);

    private readonly Dictionary<ChunkCoord, int> _playerLoaders = new();
    private readonly Dictionary<Guid, Anchor> _anchors = new();
    private readonly Dictionary<ChunkCoord, int> _anchorLoaders = new();

    public bool IsLoaded(ChunkCoord chunk) => _playerLoaders.ContainsKey(chunk) || _anchorLoaders.ContainsKey(chunk);

    public IReadOnlyCollection<ChunkCoord> GetLoadedChunks()
    {
        // Why: 调试与压测时需要快速可视化“哪些 chunk 在推进”。
        // Context: AOI/同步层会以此作为可选输入（仅对 loaded chunk 做快照/事件）。
        // Attention: 返回的是快照集合，不要在外部长期缓存引用。
        var set = new HashSet<ChunkCoord>(_playerLoaders.Keys);
        set.UnionWith(_anchorLoaders.Keys);
        return set;
    }

    public void AddPlayerLoader(ChunkCoord chunk)
    {
        _playerLoaders[chunk] = _playerLoaders.TryGetValue(chunk, out var n) ? n + 1 : 1;
    }

    public void RemovePlayerLoader(ChunkCoord chunk)
    {
        if (!_playerLoaders.TryGetValue(chunk, out var n))
        {
            return;
        }

        n--;
        if (n <= 0)
        {
            _playerLoaders.Remove(chunk);
            return;
        }

        _playerLoaders[chunk] = n;
    }

    public Guid ActivateAnchor(IReadOnlyList<ChunkCoord> chunks, DateTimeOffset expiresAtUtc)
    {
        // Why: 世界锚是“花费大量资源生产有时间限制的世界锚”的规则落点。
        // Context: 该方法只负责把锚作为 loader 叠加到指定 chunk 集合，并在过期时自动释放。
        // Attention: expiresAtUtc 必须使用 UTC；时间来源应来自服务端权威时钟。
        if (chunks.Count == 0)
        {
            throw new ArgumentException("Anchor chunks cannot be empty.", nameof(chunks));
        }

        var anchorId = Guid.NewGuid();
        var anchor = new Anchor(anchorId, expiresAtUtc, chunks.ToArray());
        _anchors.Add(anchorId, anchor);

        foreach (var c in anchor.Chunks)
        {
            _anchorLoaders[c] = _anchorLoaders.TryGetValue(c, out var n) ? n + 1 : 1;
        }

        ArcadiaLog.Info(nameof(ChunkLoadTracker), nameof(ActivateAnchor), "AnchorActivated", ("AnchorId", anchorId.ToString("N")), ("ChunkCount", chunks.Count));
        return anchorId;
    }

    public bool TryDeactivateAnchor(Guid anchorId)
    {
        if (!_anchors.TryGetValue(anchorId, out var anchor))
        {
            return false;
        }

        _anchors.Remove(anchorId);
        foreach (var c in anchor.Chunks)
        {
            if (!_anchorLoaders.TryGetValue(c, out var n))
            {
                continue;
            }

            n--;
            if (n <= 0)
            {
                _anchorLoaders.Remove(c);
            }
            else
            {
                _anchorLoaders[c] = n;
            }
        }

        ArcadiaLog.Info(nameof(ChunkLoadTracker), nameof(TryDeactivateAnchor), "AnchorDeactivated", ("AnchorId", anchorId.ToString("N")));
        return true;
    }

    public int ExpireAnchors(DateTimeOffset nowUtc)
    {
        // Why: 固定 tick 内做过期清理，避免用 Timer 引入非确定性。
        // Context: 机芯以 tick 推进；过期逻辑必须可回放。
        // Attention: nowUtc 必须是 UTC；调用方应统一时钟源（例如 tickLoop 的 TimeProvider）。
        var expired = _anchors.Values.Where(x => x.ExpiresAtUtc <= nowUtc).Select(x => x.AnchorId).ToArray();
        foreach (var id in expired)
        {
            _ = TryDeactivateAnchor(id);
        }

        return expired.Length;
    }
}

