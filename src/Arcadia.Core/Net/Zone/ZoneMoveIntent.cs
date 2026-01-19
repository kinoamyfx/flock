namespace Arcadia.Core.Net.Zone;

public sealed record ZoneMoveIntent(
    long Seq,
    ZoneVec2 Dir
);

