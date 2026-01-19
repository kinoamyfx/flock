## ADDED Requirements
### Requirement: Dungeon Zone Server Authority
The system MUST process dungeon combat, death, loot generation, inventory mutations, and exit settlement on a trusted server.

#### Scenario: Client submits input
- **WHEN** a client submits player input for movement or skill usage
- **THEN** the server MUST validate the input (rate, bounds, prerequisites)
- **AND THEN** the server MUST compute the authoritative state transition

#### Scenario: Client attempts illegal state change
- **WHEN** a client attempts to apply a state change not derived from validated input
- **THEN** the server MUST reject the change
- **AND THEN** the server MUST record an audit event for investigation

### Requirement: Dungeon Lines With Soft Cap
The system MUST shard a dungeon map into multiple “lines” and MUST enforce a per-line soft cap of 64 concurrent players.

#### Scenario: Line reaches cap
- **WHEN** a line reaches 64 concurrent players
- **THEN** the system MUST route new entrants to another line

#### Scenario: Party/friend wants same line
- **WHEN** a player requests to join a specific line
- **THEN** the system MUST allow join only if the line is under cap
- **AND THEN** the system MUST provide a fallback line selection

### Requirement: Area Of Interest (AOI)
The system MUST implement AOI filtering to limit state updates to relevant nearby entities.

#### Scenario: Entity outside AOI
- **WHEN** an entity is outside a player’s AOI range
- **THEN** the server MUST NOT send frequent state updates for that entity to the player

### Requirement: Dungeon Reset
The system MUST support resetting a dungeon line, clearing transient entities and returning players to the dungeon entrance.

#### Scenario: Dungeon line resets
- **WHEN** a dungeon line is reset
- **THEN** the system MUST clear transient dungeon entities (monsters, loot containers, projectiles, temporary effects)
- **AND THEN** the system MUST move surviving players in that line to the dungeon entrance spawn point

### Requirement: Dungeon Reset Timing
The system MUST reset a dungeon line 10 minutes after it becomes empty OR 10 minutes after it is cleared (completed), whichever applies.

#### Scenario: Empty line
- **WHEN** a dungeon line has zero players for 10 minutes
- **THEN** the system MUST reset the dungeon line

#### Scenario: Dungeon cleared
- **WHEN** the dungeon line is marked as cleared (boss defeated or completion condition met)
- **THEN** the system MUST schedule a reset after 10 minutes

### Requirement: Evacuation Point (Teleport Array)
The system MUST support evacuation points (teleport arrays) as a controlled dungeon exit mechanism.

#### Scenario: Start evacuation
- **WHEN** a player activates an evacuation point
- **THEN** the system MUST start a long cast/charge timer
- **AND THEN** the system MUST charge a high cost (resources or currency)

#### Scenario: Evacuation interrupted
- **WHEN** the player takes damage OR cancels action during evacuation charging
- **THEN** the system MUST interrupt the evacuation
- **AND THEN** the system MUST apply the configured cost rules (at minimum, consume part of the cost)
