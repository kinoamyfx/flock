using Arcadia.Mdk.Modding;
using Arcadia.Mdk.Resources;

namespace Arcadia.Core.Modding;

public sealed class ModContext : IModContext
{
    public ModContext(ModId modId, IResourceRegistry resources)
    {
        ModId = modId;
        Resources = resources;
    }

    public ModId ModId { get; }
    public IResourceRegistry Resources { get; }
}

