namespace Arcadia.Core.Net.Zone;

public sealed record ZoneLootPicked(
    Guid LootId,
    int ItemCountPicked
);

