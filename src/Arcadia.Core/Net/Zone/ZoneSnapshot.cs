namespace Arcadia.Core.Net.Zone;

public sealed record ZoneSnapshot(
    long Tick,
    ZoneVec2 PlayerPos,
    int Hp,
    int Spirit,
    IReadOnlyList<ZoneLootInfo> Loot
);

