## MODIFIED Requirements
### Requirement: Render Layer Separation (Godot)
The client MUST treat Godot as a render/input layer and MUST NOT be a source of authority for combat, loot, inventory, or settlement.

#### Scenario: State update rendering
- **WHEN** the client receives authoritative state updates from the server
- **THEN** the client MUST render the scene based on the server state
- **AND THEN** visual effects MUST NOT change authoritative game state

#### Scenario: UI is non-authoritative
- **WHEN** the player interacts with UI (inventory, evacuation, loot prompt)
- **THEN** the client MUST translate interactions into intents/requests
- **AND THEN** the server MUST remain the source of truth for authoritative results
- **AND THEN** UI state changes (e.g., inventory sort, HUD visibility toggle) MUST be purely cosmetic until confirmed by server

#### Scenario: UI inventory operations
- **WHEN** the player drags an item in the inventory UI
- **THEN** the client MUST send an intent to the server (e.g., `MoveItem(fromSlot, toSlot)`)
- **AND THEN** the client MAY show an optimistic preview (ghost item)
- **AND THEN** the client MUST revert to server-authoritative state if the server rejects the operation

#### Scenario: UI evacuation cast bar
- **WHEN** the player initiates evacuation
- **THEN** the client MUST send `EvacIntent` to the server
- **AND THEN** the client MUST display the cast bar based on server `EvacStatus` messages (not local timer)
- **AND THEN** the client MUST handle interruption (movement/damage) via server-sent `EvacStatus.interrupted`

### Requirement: Fog Of War And Lighting
The client MUST support fog-of-war and lighting as presentation features for a 2D top-down view, and MUST integrate these effects with UI rendering.

#### Scenario: Unexplored area
- **WHEN** an area has not been explored by the player
- **THEN** the client MUST render persistent exploration fog for that area
- **AND THEN** UI minimap (future) MUST show unexplored areas as black/dark

#### Scenario: Out of vision range
- **WHEN** an explored area is currently outside the player's vision
- **THEN** the client MUST render tactical fog based on current visibility rules
- **AND THEN** loot prompts and enemy HP bars MUST be hidden for entities outside vision range

#### Scenario: Lighting integration with HUD
- **WHEN** the player enters a dark area (low ambient light)
- **THEN** the client MUST apply lighting effects to the game scene (not HUD overlay)
- **AND THEN** HUD elements (HP/Spirit/EvacBar) MUST remain fully visible regardless of scene lighting
- **AND THEN** loot prompts MUST respect scene lighting (dimmed in dark areas)

### Requirement: Client Mod Networking Disabled (MVP)
The client MUST NOT allow mods to perform network access in the MVP stage.

#### Scenario: Mod attempts networking
- **WHEN** a mod attempts to open a network connection
- **THEN** the client MUST block the operation
- **AND THEN** the client MUST record a diagnostic log/audit entry

---

### Requirement: Mod UI Asset Override (表现层 Mod)
The client MUST support表现层 Mod to override UI assets (textures, fonts, theme resources) with higher-priority mod resources, while preserving the underlying UI logic and layout.

#### Scenario: Mod overrides button texture
- **WHEN** a mod provides a higher-priority texture for a UI button (e.g., `ui/button/normal`)
- **THEN** the client MUST load the mod's texture instead of the base game texture
- **AND THEN** the client MUST log the override (ResourceKey, mod ID, priority) for profiling/debugging

#### Scenario: Mod overrides theme color
- **WHEN** a mod provides a higher-priority color token (e.g., `loot_gold` → custom RGB)
- **THEN** the client MUST apply the mod's color to all UI elements using that token
- **AND THEN** the original theme resource MUST remain unmodified (mod override is runtime-only)

#### Scenario: Mod asset validation
- **WHEN** a mod provides a UI asset that fails validation (e.g., wrong dimensions, missing ResourceKey)
- **THEN** the client MUST fall back to the base game asset
- **AND THEN** the client MUST log a warning with the validation failure reason

#### Scenario: Mod priority conflict
- **WHEN** multiple mods override the same UI asset
- **THEN** the client MUST load the asset from the highest-priority mod
- **AND THEN** the client MUST provide a queryable list of "候选覆盖" (candidate overrides) for debugging

