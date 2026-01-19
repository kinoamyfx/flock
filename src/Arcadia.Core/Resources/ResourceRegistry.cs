using System.Collections.Concurrent;
using Arcadia.Core.Logging;
using Arcadia.Mdk.Modding;
using Arcadia.Mdk.Resources;

namespace Arcadia.Core.Resources;

/// <summary>
/// 资源注册表实现：更高 Priority 覆盖，更高 LoadOrder（后加载）在同优先级下覆盖。
/// </summary>
public sealed class ResourceRegistry : IResourceRegistry
{
    private readonly ConcurrentDictionary<ResourceKey, List<ResourceCandidate>> _candidates = new();
    private readonly ConcurrentDictionary<ResourceKey, ResourceCandidate> _effective = new();

    public void Register(ResourceKey key, ModId sourceModId, int priority, int loadOrder, object payload)
    {
        if (string.IsNullOrWhiteSpace(key.Namespace) || string.IsNullOrWhiteSpace(key.Path))
        {
            throw new ArgumentException("ResourceKey is invalid.", nameof(key));
        }

        var candidate = new ResourceCandidate(key, sourceModId, priority, loadOrder, payload);

        var list = _candidates.GetOrAdd(key, _ => new List<ResourceCandidate>());
        lock (list)
        {
            list.Add(candidate);
        }

        _effective.AddOrUpdate(
            key,
            _ => candidate,
            (_, existing) =>
            {
                var chosen = ChooseEffective(existing, candidate);
                if (!ReferenceEquals(existing, chosen))
                {
                    ArcadiaLog.Info(
                        nameof(ResourceRegistry),
                        nameof(Register),
                        "Override",
                        ("ResourceKey", key.ToString()),
                        ("FromMod", existing.SourceModId.Value),
                        ("FromPriority", existing.Priority),
                        ("FromLoadOrder", existing.LoadOrder),
                        ("ToMod", candidate.SourceModId.Value),
                        ("ToPriority", candidate.Priority),
                        ("ToLoadOrder", candidate.LoadOrder));
                }

                return chosen;
            });
    }

    public bool TryGetEffective(ResourceKey key, out ResourceDescriptor descriptor, out object payload)
    {
        if (_effective.TryGetValue(key, out var candidate))
        {
            descriptor = new ResourceDescriptor(candidate.Key, candidate.SourceModId, candidate.Priority, candidate.LoadOrder);
            payload = candidate.Payload;
            return true;
        }

        descriptor = new ResourceDescriptor(key, new ModId(string.Empty), Priority: int.MinValue, LoadOrder: int.MinValue);
        payload = null!;
        return false;
    }

    public IReadOnlyList<ResourceDescriptor> GetCandidates(ResourceKey key)
    {
        if (!_candidates.TryGetValue(key, out var list))
        {
            return Array.Empty<ResourceDescriptor>();
        }

        lock (list)
        {
            return list
                .Select(x => new ResourceDescriptor(x.Key, x.SourceModId, x.Priority, x.LoadOrder))
                .OrderByDescending(x => x.Priority)
                .ThenByDescending(x => x.LoadOrder)
                .ToArray();
        }
    }

    private static ResourceCandidate ChooseEffective(ResourceCandidate existing, ResourceCandidate challenger)
    {
        // Why: 资源覆盖必须稳定且可解释；否则 Mod 叠加会出现“玄学”覆盖，难以排错。
        // Context: 需求要求“更高优先级覆盖”；同优先级下用加载顺序（后加载覆盖）满足资源包常见心智。
        // Attention: 若未来引入依赖图与“显式覆盖声明”，需要调整此规则并同步升级 MDK 版本。
        if (challenger.Priority != existing.Priority)
        {
            return challenger.Priority > existing.Priority ? challenger : existing;
        }

        return challenger.LoadOrder >= existing.LoadOrder ? challenger : existing;
    }
}
