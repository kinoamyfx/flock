## MODIFIED Requirements
### Requirement: Render Layer Separation (Godot)
The client MUST treat Godot as a render/input layer and MUST NOT be a source of authority for combat, loot, inventory, or settlement.

#### Scenario: First playable HUD
- **WHEN** a player is in a dungeon
- **THEN** the client MUST render a minimal HUD (HP/Spirit placeholders, evacuation cast bar placeholder, loot prompt)
- **AND THEN** UI interactions MUST translate into intents (not direct state mutations)

