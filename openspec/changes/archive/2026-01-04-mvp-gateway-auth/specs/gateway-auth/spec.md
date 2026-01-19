## ADDED Requirements
### Requirement: Gateway Issues Zone Handshake Tokens
The system MUST provide a Gateway service that issues signed authentication tokens for dungeon zone handshake.

#### Scenario: Issue token success
- **WHEN** a client requests a zone token with valid credentials (or dev-mode credentials in MVP)
- **THEN** the Gateway MUST return a signed token
- **AND THEN** the token MUST include `playerId`, `iat`, `exp`, and `kid`

### Requirement: Key Rotation
The system MUST support key rotation by maintaining a key set identified by `kid`.

#### Scenario: Active key used for signing
- **WHEN** the Gateway signs a new token
- **THEN** the Gateway MUST use the active signing key
- **AND THEN** the token header MUST contain the active `kid`

#### Scenario: Retired key still verifies
- **WHEN** a signing key is rotated
- **THEN** the Gateway MUST stop issuing new tokens with the retired key
- **AND THEN** the zone server MUST still be able to verify tokens signed by the retired key until they expire

