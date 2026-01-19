## ADDED Requirements
### Requirement: Chunk-Activated World Tick
The system MUST advance simulation only for loaded chunks by default.

#### Scenario: Player enters a chunk
- **WHEN** a player character enters chunk `C`
- **THEN** the system MUST mark `C` as loaded
- **AND THEN** the system MUST advance simulation within `C` on each world tick

#### Scenario: No players nearby
- **WHEN** chunk `C` has no active loaders (no players, no anchors)
- **THEN** the system MUST stop advancing simulation within `C`

### Requirement: Headless Simulation Kernel (World)
The system MUST run world simulation in a headless kernel with a fixed tick rate and MUST treat client frame-rate as presentation-only.

#### Scenario: Fixed tick progression
- **WHEN** the server is configured with `TickHz = H`
- **THEN** the server MUST progress the authoritative simulation in discrete ticks
- **AND THEN** authoritative logic MUST NOT depend on client frame rate

#### Scenario: Deterministic-by-contract (same inputs)
- **GIVEN** same initial simulation state and same ordered input stream
- **WHEN** the simulation runs for `N` ticks
- **THEN** the resulting state MUST be identical (byte-for-byte for persisted fields) within the same server build

### Requirement: World Anchor
The system MUST provide a “world anchor” mechanism that keeps a configured chunk region loaded for a limited duration by consuming resources.

#### Scenario: Activate anchor
- **WHEN** a player activates a world anchor with valid payment
- **THEN** the system MUST keep the configured chunk region loaded for the anchor duration

#### Scenario: Anchor expires
- **WHEN** the anchor duration ends (or resource consumption fails)
- **THEN** the system MUST stop keeping the region loaded
- **AND THEN** the region MUST follow default chunk-activated tick rules
