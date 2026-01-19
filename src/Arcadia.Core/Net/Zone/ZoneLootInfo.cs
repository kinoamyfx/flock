namespace Arcadia.Core.Net.Zone;

public sealed record ZoneLootInfo(
    Guid LootId,
    ZoneVec2 Pos,
    int ItemCount,
    int ProtectedMsRemaining,
    bool CanPick
);

