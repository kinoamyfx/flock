## ADDED Requirements

### Requirement: Silk.NET Client Exists (v1.0.0 Proposal)
The project SHALL provide a Silk.NET + WGPU based client that preserves the client-non-authoritative boundary and can reach feature parity with the existing v1.0.0 client baseline.

#### Scenario: Minimal parity smoke
- **WHEN** the Silk.NET client is built and run locally
- **THEN** it SHALL render a 2D pixel-friendly scene and a minimal HUD
- **AND THEN** it SHALL perform Hello/Welcome handshake and render server-authoritative snapshots

### Requirement: Godot Baseline Preserved Until Cutover
The project MUST keep the existing Godot client baseline available until the Silk.NET client meets the v1.0.0 acceptance gates.

#### Scenario: Cutover gating
- **WHEN** the team decides to switch the default client to Silk.NET
- **THEN** it MUST show evidence that build/test/smoke and UI screenshot artifacts are passing
- **AND THEN** it MUST keep a rollback path back to the Godot baseline

