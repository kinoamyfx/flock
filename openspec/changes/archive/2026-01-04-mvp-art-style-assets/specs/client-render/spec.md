## ADDED Requirements
### Requirement: Asset Pipeline Consistency
The client MUST enforce asset pipeline consistency for v1.0.0 (naming, scale, compression/import settings) to keep the visual style cohesive.

#### Scenario: Import validation
- **WHEN** an asset is added to the project
- **THEN** the pipeline MUST validate naming and basic import constraints
- **AND THEN** invalid assets MUST be rejected or flagged with actionable diagnostics

#### Scenario: Mod override visibility
- **WHEN** a higher-priority mod overrides a visual asset
- **THEN** the client MUST be able to expose which asset was overridden and by which mod (at least via logs)
  - **AND THEN** the override MUST NOT change authoritative gameplay outcomes

#### Scenario: Consistent pixel density import
- **WHEN** a pixel art texture is imported
- **THEN** the pipeline MUST enforce pixel-art friendly import settings (e.g., no unintended filtering)
- **AND THEN** textures MUST not silently change scale that breaks the Art Bible tokens
