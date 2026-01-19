# Dungeon Zone Specification

## Purpose
Define the server-authoritative dungeon zone system, including combat, death, loot generation, inventory mutations, and evacuation settlement.

## Requirements

### Requirement: Dungeon Zone Server Authority
The system MUST process dungeon combat, death, loot generation, inventory mutations, and exit settlement on a trusted server.

#### Scenario: Server-authoritative movement
- **WHEN** a client submits movement intent
- **THEN** the server MUST validate and apply authoritative position updates
- **AND THEN** the server MUST normalize direction vectors and enforce speed limits (100 units/s)
- **AND THEN** the server MUST use sequence numbers to prevent replay attacks

#### Verification
- ✅ Implemented: `ZoneServerHost.OnMoveIntent` handler (lines 76-131)
- ✅ Speed enforcement: `moveSpeed = 100f` units/second
- ✅ Replay protection: `lastMoveSeq` dictionary tracks sequence numbers
- ✅ Smoke test: `scripts/smoke_playable_slice.sh` verifies movement intent handling

#### Scenario: Server-authoritative evacuation
- **WHEN** a client submits an evacuation intent
- **THEN** the server MUST validate and apply the evacuation rules (cast, interrupt, cost placeholder) and settle the run
- **AND THEN** the server MUST start a 10-second cast timer
- **AND THEN** the server MUST interrupt evacuation if the player moves
- **AND THEN** the server MUST broadcast evacuation status (casting/completed/interrupted) every tick

#### Verification
- ✅ Implemented: `ZoneServerHost.OnEvacIntent` handler (lines 192-231)
- ✅ Cast duration: 10 seconds (10,000ms)
- ✅ Movement interruption: Handled in `OnMoveIntent` (lines 118-130)
- ✅ Status broadcasting: `transport.BroadcastEvacStatus` called every tick (lines 339-349)
- ✅ High cost placeholder: 100 gold (to be replaced with evacuation item)

#### Scenario: Death and loot drop
- **WHEN** a player dies in a dungeon
- **THEN** the server MUST drop all carried items (safe box excluded)
- **AND THEN** the server MUST create a loot container with 10-second killer-party protection
- **AND THEN** the server MUST broadcast loot spawn to all players

#### Verification
- ✅ Implemented: `ZoneLootService.DropAllCarriedOnDeath` (atomic operation)
- ✅ Safe box protection: `Inventory.SafeBoxSlotLimit = 9` (hard constraint)
- ✅ Loot protection: `LootContainer.CanPickup` enforces 10s killer-party exclusive window
- ✅ Audit logging: "DropCarried" event with EntityId/KillerPartyId/ItemCount

#### Scenario: Loot pickup with protection
- **WHEN** a player attempts to pick up loot
- **THEN** the server MUST check if the loot container exists
- **AND THEN** the server MUST enforce 10-second killer-party protection
- **AND THEN** the server MUST add items to the player's inventory and remove the loot container

#### Verification
- ✅ Implemented: `ZoneLootService.TryPickupLoot` (lines 54-91)
- ✅ Protection check: `LootContainer.CanPickup(pickerPartyId, now)`
- ✅ Atomic operation: Check → Add to inventory → Remove container
- ✅ Audit logging: "PickupLoot" event with LootId/PickerPartyId/ItemCount
- ✅ Smoke test: `PlayableSliceTest` verifies loot spawn and pickup

## Implementation Status
- **Status**: ✅ Complete (2026-01-03)
- **Test Coverage**: 34/34 tests passing
- **Key Files**:
  - `src/Arcadia.Server/Zone/ZoneServerHost.cs` (Core zone server logic)
  - `src/Arcadia.Server/Zone/ZoneLootService.cs` (Loot drop and pickup)
  - `src/Arcadia.Server/Systems/CombatSystem.cs` (Combat and death)
  - `src/Arcadia.Core/Aoi/GridAoi.cs` (Area-of-interest visibility)
