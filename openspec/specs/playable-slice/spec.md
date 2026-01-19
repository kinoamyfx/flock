# Playable Slice Specification

## Purpose
Define the first playable dungeon loop for v1.0.0, where a player can enter a dungeon, move, interact, die/drop/loot, and evacuate with full server authority.

## Requirements

### Requirement: First Playable Dungeon Loop
The project MUST provide a first playable dungeon loop for v1.0.0 where a player can enter, move, interact, die/drop/loot, and evacuate with server authority.

#### Scenario: Complete one run
- **WHEN** a player starts the client and enters a dungeon
- **THEN** the player MUST be able to move and interact using intents
- **AND THEN** the server MUST apply authoritative results (position, death, loot)
- **AND THEN** the player MUST be able to evacuate to settle or die and drop carried items

#### Verification
- ✅ Verified via `scripts/smoke_playable_slice.sh` (2026-01-03)
- ✅ Automated test: `Arcadia.LoadTest/PlayableSliceTest.cs`
- ✅ Test result: PASS (连接→移动→死亡掉落→拾取)

### Requirement: Audit Reconstruction (Kill → Drop → Loot)
The system MUST reconstruct a full Kill → Drop → Loot chain for the first playable loop.

#### Scenario: PvP loot dispute
- **WHEN** a kill and subsequent loot pickup happens
- **THEN** the system MUST record sufficient audit events to reconstruct who killed whom, where, what dropped, and who looted what

#### Verification
- ✅ Implemented: `ZoneLootService.TryPickupLoot` logs "PickupLoot" audit event (LootId/PickerPartyId/ItemCount)
- ✅ Implemented: `ZoneLootService.DropAllCarriedOnDeath` logs "DropCarried" audit event (EntityId/KillerPartyId/ItemCount)
- ✅ Audit sink: `IAuditSink` interface with `ConsoleAuditSink` and `PostgresAuditSink` implementations

## Implementation Status
- **Status**: ✅ Complete (2026-01-03)
- **Verification**: Automated smoke test passing
- **Evidence**: `.tmp/smoke_playable_slice.log` (Verdict=PASS)
