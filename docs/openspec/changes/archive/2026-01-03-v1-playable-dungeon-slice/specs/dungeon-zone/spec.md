## MODIFIED Requirements
### Requirement: Dungeon Zone Server Authority
The system MUST process dungeon combat, death, loot generation, inventory mutations, and exit settlement on a trusted server.

#### Scenario: First playable movement
- **WHEN** a client submits movement intent
- **THEN** the server MUST validate and apply authoritative position updates

#### Scenario: First playable evacuation
- **WHEN** a client submits an evacuation intent
- **THEN** the server MUST validate and apply the evacuation rules (cast, interrupt, cost placeholder) and settle the run

