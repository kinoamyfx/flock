# Art Style Specification

## Purpose
Define the v1.0.0 visual direction (pixel density, palette, fog/glow language) and the minimum asset coverage required to support the playable loop, so the project stays cohesive and regression-verifiable.

## Requirements
### Requirement: Art Bible (v1.0.0)
The project MUST ship an Art Bible for v1.0.0 that defines visual style rules for world assets, characters, VFX, and UI consistency.

#### Scenario: Art Bible completeness
- **WHEN** v1.0.0 is prepared for release
- **THEN** the Art Bible MUST define pixel density/scale, color system, material language, lighting rules, fog rules, and VFX rules

### Requirement: Pixel Grid Discipline (Option A)
The client MUST render and ship assets that respect a consistent pixel grid (no unintended sub-pixel jitter) for the Option A high-density pixel art direction.

#### Scenario: Camera movement
- **WHEN** the camera pans or tracks the player
- **THEN** pixel art sprites MUST remain visually stable (no shimmering/jitter caused by fractional pixel sampling)

#### Scenario: Sprite placement
- **WHEN** any world sprite is placed in the scene (tiles/props/characters)
- **THEN** its transform MUST align to the configured pixel grid policy
- **AND THEN** the asset pipeline MUST flag assets or scenes that violate the policy

### Requirement: Scale & Pixel Density Tokens
The project MUST define and use a single source of truth for pixel density and scale tokens (tile pixel size, pixels-per-unit, camera zoom steps).

#### Scenario: No mixed scales
- **WHEN** assets from different categories (tiles, characters, items, VFX, UI icons) are combined on screen
- **THEN** they MUST share compatible scale tokens
- **AND THEN** the project MUST avoid mixed arbitrary scales that break the cohesive look

### Requirement: Color Tokens For Day/Night/Dungeon
The Art Bible MUST define a color token system that supports day/night and dungeon (cave) moods, including fog and light color language.

#### Scenario: Dungeon atmosphere
- **WHEN** the player enters a dungeon with fog and lighting enabled
- **THEN** fog, light sources, and VFX MUST follow the defined color tokens

### Requirement: VFX Style Rules (Glow/Fog/Particles)
The Art Bible MUST define VFX style rules that make the game feel “cool” while remaining consistent with pixel art (glow, fog, particles, trails).

#### Scenario: Skill cast and impact
- **WHEN** a skill is cast and hits a target
- **THEN** cast/impact VFX MUST be readable and follow the defined VFX rules (colors, glow usage, timing)
- **AND THEN** VFX MUST NOT obscure critical gameplay readability (HP, loot prompts, evacuation cast bar)

### Requirement: v1.0.0 Minimal Asset Pack
The project MUST ship a minimal asset pack for v1.0.0 that supports the playable loop (prepare → enter dungeon → loot/evacuate → settle).

#### Scenario: Asset coverage
- **WHEN** a player completes one dungeon run in v1.0.0
- **THEN** the world, characters, items, VFX, and UI MUST be represented by assets that follow the Art Bible

#### Scenario: Minimal pack categories
- **WHEN** the v1.0.0 asset pack is delivered
- **THEN** it MUST include at least:
  - one biome tileset (ground + edge transitions)
  - one dungeon tileset (floor/walls/door/placeholder mechanics)
  - at least 10 props/buildings for the “prepare” loop
  - at least 1 player character set (idle/run/attack/cast/hit/death)
  - at least 3 NPC/monster sets for dungeon
  - at least 40 item/icon sprites (resources/materials/equipment/artifacts placeholders allowed)
  - at least 12 VFX assets (hit/crit/pickup/drop sparkle/evac cast/projectile/explosion)

#### Scenario: Animations present
- **WHEN** the player character and NPC/monsters are used in v1.0.0 gameplay
- **THEN** they MUST provide a minimal animation set that supports readability (idle/move/attack/cast/hit/death)

### Requirement: Visual Acceptance Artifacts
The project MUST ship visual acceptance artifacts for v1.0.0 that demonstrate cohesion and “finish” level.

#### Scenario: Screenshot/clip pack exists
- **WHEN** v1.0.0 is prepared for release
- **THEN** the team MUST provide a screenshot pack (or short clips) covering: a biome scene, a dungeon scene (fog+lighting), inventory UI, dungeon HUD, and a skill VFX moment

### Requirement: Mod-Friendly Asset Overrides
The client MUST allow higher-priority mods to override visual assets while keeping authoritative gameplay unchanged.

#### Scenario: Mod overrides an asset
- **WHEN** a mod overrides a visual asset (texture/icon/ui resource)
- **THEN** the client MUST load the overridden asset according to priority rules
- **AND THEN** the client MUST record which asset was overridden and by which mod
