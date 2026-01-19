namespace Arcadia.Core.Net.Zone;

public sealed record ZoneLootSpawned(
    Guid LootId,
    ZoneVec2 Pos,
    int ItemCount
);

