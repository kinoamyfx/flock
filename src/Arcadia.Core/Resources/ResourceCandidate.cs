using Arcadia.Mdk.Modding;
using Arcadia.Mdk.Resources;

namespace Arcadia.Core.Resources;

internal sealed record ResourceCandidate(
    ResourceKey Key,
    ModId SourceModId,
    int Priority,
    int LoadOrder,
    object Payload
);

