## ADDED Requirements
### Requirement: First Playable Dungeon Loop
The project MUST provide a first playable dungeon loop for v1.0.0 where a player can enter, move, interact, die/drop/loot, and evacuate with server authority.

#### Scenario: Complete one run
- **WHEN** a player starts the client and enters a dungeon
- **THEN** the player MUST be able to move and interact using intents
- **AND THEN** the server MUST apply authoritative results (position, death, loot)
- **AND THEN** the player MUST be able to evacuate to settle or die and drop carried items

### Requirement: Audit Reconstruction (Kill → Drop → Loot)
The system MUST reconstruct a full Kill → Drop → Loot chain for the first playable loop.

#### Scenario: PvP loot dispute
- **WHEN** a kill and subsequent loot pickup happens
- **THEN** the system MUST record sufficient audit events to reconstruct who killed whom, where, what dropped, and who looted what

