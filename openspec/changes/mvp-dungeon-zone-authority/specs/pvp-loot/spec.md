## ADDED Requirements
### Requirement: Full-Drop On Death In Dungeon
The system MUST drop all items currently carried by the player character upon death inside a dungeon, excluding items stored in the safe box.

#### Scenario: Player dies in dungeon
- **WHEN** a player character’s HP reaches zero inside a dungeon
- **THEN** the system MUST create a loot container at the death location
- **AND THEN** the system MUST move all carried items (excluding safe box items) into the loot container

### Requirement: Safe Box Items Are Non-Droppable
The system MUST NOT drop items stored in the safe box when the player dies or disconnects in a dungeon.

#### Scenario: Player dies with safe box items
- **WHEN** a player dies inside a dungeon while having items in the safe box
- **THEN** the system MUST drop carried items only
- **AND THEN** the system MUST keep safe box items unchanged

### Requirement: Safe Box Is “9-slot Storage Bag”
The system MUST provide a safe box represented as a 9-slot “storage bag” that is craftable by consuming materials.

#### Scenario: Safe box capacity reached
- **GIVEN** a player has a safe box with 9 occupied slots
- **WHEN** the player attempts to store an additional item stack into the safe box
- **THEN** the system MUST reject the action

#### Scenario: Safe box usable in dungeon
- **WHEN** a player is inside a dungeon
- **THEN** the player MUST be able to store, withdraw, and organize items in the safe box

### Requirement: Disconnect Leaves Avatar In-World
The system MUST keep the player character avatar in the dungeon world for 60 seconds after disconnect, allowing it to be attacked and killed.

#### Scenario: Client disconnects
- **WHEN** a client disconnects inside a dungeon
- **THEN** the system MUST keep the avatar at the last authoritative position
- **AND THEN** the avatar MUST remain attackable by other players and NPCs
- **AND THEN** the avatar MUST remain in-world for 60 seconds since disconnect

#### Scenario: Disconnected avatar is killed
- **WHEN** a disconnected avatar is killed in a dungeon
- **THEN** the system MUST apply the same full-drop rule as a normal death

### Requirement: Reconnect Restores Control If Dungeon Not Reset
The system MUST restore player control at the last authoritative position when the player reconnects within the disconnect grace period and the dungeon has not been reset.

#### Scenario: Reconnect within 60 seconds and dungeon not reset
- **WHEN** a disconnected player reconnects within 60 seconds
- **AND WHEN** the dungeon instance has not been reset
- **THEN** the system MUST restore control of the avatar at the last authoritative position

### Requirement: Reconnect Spawns At Entrance If Dungeon Reset
The system MUST spawn the player at the dungeon entrance when the player reconnects and the dungeon instance has been reset.

#### Scenario: Reconnect after dungeon reset
- **WHEN** a disconnected player reconnects
- **AND WHEN** the dungeon instance has been reset
- **THEN** the system MUST spawn the player at the dungeon entrance

### Requirement: Kill/Loot Audit Trail
The system MUST record an audit trail sufficient to reconstruct “who killed whom, where, and who looted what”.

#### Scenario: Loot picked up
- **WHEN** any player picks up an item from a loot container
- **THEN** the system MUST record an audit event including killer (if any), victim (if any), looter, item id, and location

### Requirement: Loot Pickup Protection Window
The system MUST enforce a 10-second loot pickup protection window after a player death, during which only the killer’s party can pick up items from the death loot container.

#### Scenario: Within protection window
- **WHEN** time since loot container creation is ≤ 10 seconds
- **THEN** the system MUST allow pickup only if the looter is in the killer’s party
- **AND THEN** the system MUST reject pickup attempts from others

#### Scenario: After protection window
- **WHEN** time since loot container creation is > 10 seconds
- **THEN** the system MAY allow pickup by any player according to dungeon rules (free-for-all)
