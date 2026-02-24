## 1. Specification
- [x] 1.1 Review proposal/design/spec deltas with owner
- [x] 1.2 Lock MVP acceptance criteria (combat authority, loot, disconnect, 64 cap)

## 2. Core Architecture (Headless + Godot Render)
- [x] 2.1 Define headless simulation module boundaries (world vs dungeon)
- [x] 2.2 Define client-render contract (state snapshots, events, VFX hooks)

## 3. Dungeon Zone Server (Authoritative)
- [x] 3.1 Implement zone lifecycle: create line, join/leave, migration, shutdown
- [x] 3.2 Implement AOI (area-of-interest) visibility & message filtering
- [x] 3.3 Implement authoritative combat loop (tick, hit validation, damage, death)
- [x] 3.4 Implement transport abstraction (MVP: ENet via EnetServerTransport)

## 4. Inventory / Loot / Full-Drop
- [x] 4.1 Persist authoritative inventory & dungeon backpack
- [x] 4.2 Implement death: drop "carried all items" into loot container entity
- [x] 4.3 Implement disconnect: keep avatar in-world; allow kill & drop
- [x] 4.4 Implement pickup ownership & anti-dupe (10s killer-party protection)
- [x] 4.5 Implement safe box: non-droppable storage excluded from full-drop

## 5. Anti-Cheat & Abuse Controls
- [x] 5.1 Input rate limiting & sanity checks (speed, skill cadence, teleport)
  - Code: `src/Arcadia.Server/Net/Enet/ZoneIntentRateLimiter.cs`, `src/Arcadia.Server/Zone/ZoneMovement.cs`
  - Tests: `tests/Arcadia.Tests/ZoneIntentRateLimiterTests.cs`
- [x] 5.2 Replay protection & sequence validation
  - Code: `src/Arcadia.Server/Zone/ZoneServerHost.cs` (seq reject + per-tick gating)
- [x] 5.3 Server audit logs for "kill/loot" chain reconstruction

## 6. Observability
- [x] 6.1 Correlation id across client ↔ zone server ↔ persistence
  - Code: `src/Arcadia.Core/Logging/ArcadiaLogContext.cs`, `src/Arcadia.Core/Logging/ArcadiaLog.cs`, `src/Arcadia.Server/Net/Enet/EnetServerTransport.cs`, `src/Arcadia.Client/Net/Enet/EnetClientTransport.cs`
- [x] 6.2 Metrics: line population, tick cost, bandwidth, kill/loot rates
  - Code: `src/Arcadia.Server/Zone/ZoneServerHost.cs`, `src/Arcadia.Server/Net/Enet/EnetServerTransport.cs`

## 7. Testing & Verification
- [x] 7.1 Unit tests for loot drop & persistence invariants (no dupe, no loss)
- [x] 7.2 Simulation determinism tests (same inputs => same results)
  - Tests: `tests/Arcadia.Tests/ZoneMovementDeterminismTests.cs`
- [x] 7.3 Load test: 64 clients baseline with AOI enabled
  - Script: `scripts/loadtest_64.sh`
  - Artifacts: `.tmp/loadtest64_gateway.log`, `.tmp/loadtest64_server.log`, `.tmp/loadtest64_loadtest.log`
- [x] 7.4 Integration test: disconnect within 60s reconnect (not reset => restore pos; reset => entrance)
