## ADDED Requirements
### Requirement: NPC Dynamic Exchange As Stabilizer
The system MUST provide an NPC exchange that dynamically adjusts exchange rates based on resource supply to stabilize the economy.

#### Scenario: Resource oversupply
- **WHEN** a resource is oversupplied relative to the NPC target inventory
- **THEN** the NPC MUST reduce the exchange rate for that resource

#### Scenario: Resource scarcity
- **WHEN** a resource is scarce relative to the NPC target inventory
- **THEN** the NPC MUST increase the exchange rate for that resource

### Requirement: Mandatory Resource Sinks
The system MUST include mandatory sinks that continuously remove resources from the economy.

#### Scenario: Equipment durability
- **WHEN** players use tools or equipment
- **THEN** the system MUST reduce durability
- **AND THEN** repair MUST consume materials or currency

#### Scenario: Sect maintenance
- **WHEN** players maintain a sect with recruited members
- **THEN** the system MUST require ongoing consumption (food, salary, spirit resources, or equivalent)

### Requirement: Trade Taxes And Fees As Sink
The system MUST apply transaction taxes and trading fees, and MUST destroy collected taxes/fees as a resource sink.

#### Scenario: Player-to-player trade fee
- **WHEN** a player-to-player trade is settled
- **THEN** the system MUST charge a trading fee
- **AND THEN** the fee MUST be removed from the economy (system sink)

#### Scenario: NPC exchange tax
- **WHEN** a player exchanges items or currency with an NPC exchange
- **THEN** the system MUST charge a tax
- **AND THEN** the tax MUST be removed from the economy (system sink)
