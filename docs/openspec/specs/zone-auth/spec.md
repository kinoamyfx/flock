# zone-auth Specification

## Purpose
Define how the zone server validates Gateway-issued tokens during handshake and binds connections to authenticated `playerId`, providing short disconnect error hints for diagnostics without leaking sensitive details.

## Requirements
### Requirement: Zone Handshake Authentication Token
The system MUST require an authentication token during dungeon zone handshake and MUST NOT trust client-supplied player identity fields.

#### Scenario: Valid token
- **WHEN** a client sends `Hello` with a valid auth token issued by the Gateway
- **THEN** the zone server MUST bind the connection to the player identity derived from the token
- **AND THEN** the zone server MUST return `Welcome`

#### Scenario: Invalid token
- **WHEN** a client sends `Hello` with an invalid or expired token
- **THEN** the zone server MUST reject the handshake
- **AND THEN** the zone server MUST disconnect the client

### Requirement: Disconnect Error Hint
The system MUST include a short error hint in the disconnect message payload for diagnostic purposes.

#### Scenario: Auth failed hint
- **WHEN** a client is disconnected due to auth failure
- **THEN** the client MUST receive a short error code and message (e.g., `auth_failed`, `invalid_signature`)
