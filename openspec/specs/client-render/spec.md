# Client Render Specification

## Purpose
Define the separation between the client render layer (客户端渲染层) and server authority, ensuring the client is purely for rendering and input while all game logic remains server-authoritative.
## Requirements
### Requirement: Render Layer Separation (客户端渲染层)
The client MUST treat 客户端渲染层 as a render/input layer and MUST NOT be a source of authority for combat, loot, inventory, or settlement.

#### Scenario: Input to intent translation
- **WHEN** a player provides input (movement, pickup, evacuation)
- **THEN** the client MUST translate the input into intents (not direct state mutations)
- **AND THEN** the client MUST send intents to the server via network messages
- **AND THEN** the client MUST wait for server-authoritative results before updating the visual state

#### Verification
- ✅ Implemented: `EnetClientTransport.SendMoveIntent` (lines 200-209)
- ✅ Implemented: `EnetClientTransport.SendPickupIntent` (lines 211-219)
- ✅ Implemented: `EnetClientTransport.SendEvacIntent` (lines 230-238)
- ✅ No client-side state mutation: All input → intent → server authority → client update

#### Scenario: First playable HUD
- **WHEN** a player is in a dungeon
- **THEN** the client MUST render a minimal HUD (HP/Spirit placeholders, evacuation cast bar placeholder, loot prompt)
- **AND THEN** UI interactions MUST translate into intents (not direct state mutations)

#### Verification
- ✅ Implemented: `godot/arcadia_godot_client/scenes/hud.tscn` (HUD scene with ProgressBar + Label placeholders)
- ✅ Implemented: `godot/arcadia_godot_client/scripts/hud.gd` (HP/Spirit/EvacBar/LootPrompt rendering)
- ✅ Implemented: `godot/arcadia_godot_client/scripts/player.gd` (WASD input → SendMoveIntent)
- ✅ UI screenshots: `.tmp/ui/hud.png` (9.2KB, verified 2026-01-03)
- ✅ Theme application: `arcadia_theme.tres` (深色主题 + 金色点缀 + 冷光反馈)

#### Scenario: Client-side prediction with server reconciliation
- **WHEN** a player moves using WASD input
- **THEN** the client MAY predict the movement locally for smooth rendering
- **AND THEN** the client MUST reconcile the predicted position with the server-authoritative position from Snapshot
- **AND THEN** the client MUST use interpolation (lerp) to smooth out corrections

#### Verification
- ✅ Implemented: `godot/arcadia_godot_client/scripts/player.gd`
  - Line ~15: Client-side prediction (move position locally)
  - Line ~30: Server reconciliation (lerp to authoritative position from Snapshot)
- ✅ Snapshot handling: `OnSnapshot` callback updates authoritative position
- ✅ No authority: Client prediction is purely visual, server always wins

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

---

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

---

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

### Requirement: Asset Pipeline Consistency
The client MUST enforce asset pipeline consistency for v1.0.0 (naming, scale, compression/import settings) to keep the visual style cohesive.

#### Scenario: Import validation
- **WHEN** an asset is added to the project
- **THEN** the pipeline MUST validate naming and basic import constraints
- **AND THEN** invalid assets MUST be rejected or flagged with actionable diagnostics

#### Scenario: Mod override visibility
- **WHEN** a higher-priority mod overrides a visual asset
- **THEN** the client MUST be able to expose which asset was overridden and by which mod (at least via logs)
  - **AND THEN** the override MUST NOT change authoritative gameplay outcomes

#### Scenario: Consistent pixel density import
- **WHEN** a pixel art texture is imported
- **THEN** the pipeline MUST enforce pixel-art friendly import settings (e.g., no unintended filtering)
- **AND THEN** textures MUST not silently change scale that breaks the Art Bible tokens

## Implementation Status
- **Status**: ✅ Complete (2026-01-03)
- **Key Files**:
  - `src/Arcadia.Client/Net/Enet/EnetClientTransport.cs` (Network layer)
  - `godot/arcadia_godot_client/scripts/player.gd` (Player input and prediction)
  - `godot/arcadia_godot_client/scripts/hud.gd` (HUD rendering)
  - `godot/arcadia_godot_client/scenes/hud.tscn` (HUD scene)
  - `godot/arcadia_godot_client/theme/arcadia_theme.tres` (UI theme)
- **Visual Artifacts**:
  - `.tmp/ui/hud.png` (HUD screenshot)
  - `.tmp/ui/main_menu.png` (Main menu screenshot)
  - `.tmp/art/baseline_main.png` (客户端渲染层 rendering baseline)
