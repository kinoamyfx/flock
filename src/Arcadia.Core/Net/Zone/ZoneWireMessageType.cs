namespace Arcadia.Core.Net.Zone;

public enum ZoneWireMessageType
{
    Hello = 1,
    Welcome = 2,
    Error = 3,

    // Client → Server intents (authoritative server applies results).
    MoveIntent = 10,
    PickupIntent = 11,
    EvacIntent = 12,
    DebugKillSelf = 19,

    // Server → Client state/events.
    Snapshot = 20,
    LootSpawned = 21,
    LootPicked = 22,
    EvacStatus = 23
}
