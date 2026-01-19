## ADDED Requirements
### Requirement: Render Layer Separation (Godot)
The client MUST treat Godot as a render/input layer and MUST NOT be a source of authority for combat, loot, inventory, or settlement.

#### Scenario: State update rendering
- **WHEN** the client receives authoritative state updates from the server
- **THEN** the client MUST render the scene based on the server state
- **AND THEN** visual effects MUST NOT change authoritative game state

### Requirement: Fog Of War And Lighting
The client MUST support fog-of-war and lighting as presentation features for a 2D top-down view.

#### Scenario: Unexplored area
- **WHEN** an area has not been explored by the player
- **THEN** the client MUST render persistent exploration fog for that area

#### Scenario: Out of vision range
- **WHEN** an explored area is currently outside the player’s vision
- **THEN** the client MUST render tactical fog based on current visibility rules

### Requirement: Client Mod Networking Disabled (MVP)
The client MUST NOT allow mods to perform network access in the MVP stage.

#### Scenario: Mod attempts networking
- **WHEN** a mod attempts to open a network connection
- **THEN** the client MUST block the operation
- **AND THEN** the client MUST record a diagnostic log/audit entry
