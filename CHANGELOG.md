# Changelog

All notable changes to the Arcadia project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Removed - 2026-01-17

#### Godot 客户端完全移除
- **架构决策**: 从双客户端策略（Godot + Silk.NET）转为纯 Silk.NET 客户端
- **删除范围**:
  - `godot/` 目录（91 个文件）：GDScript 客户端 + C# 客户端 + 所有场景/脚本/shader/theme
  - `scripts/export_godot_client.sh` - Godot PCK 导出脚本
  - `scripts/run_godot_client.sh` - Godot 启动脚本
  - `.tmp/export/Arcadia.pck` - PCK 数据包
  - `.tmp/ui/*.png` - UI 截图产物
  - `.tmp/art/baseline_main.png` - 美术基线截图
- **保留设计资产**（引擎无关）:
  - UI Token 系统设计规范（颜色/字体/间距/动效 tokens）
  - 像素风技术规范（Pixel Grid/Scale/Color Palette）
  - 命名与导入规则（ResourceKey 映射）
- **技术影响**:
  - v1.0.0 Must 项更新：~~"Godot 2D 灯光迷雾"~~ → **"客户端 2D 渲染（支持光照与迷雾）"**
  - 验收方式变更：~~`ui_regression_gate.sh`~~ → Silk.NET 客户端视觉验收
- **当前客户端**: Silk.NET MVP 客户端（已验收，60-120 FPS，支持精灵渲染/动画/HUD）

---

## [mvp-ui-art-direction] - 2026-01-03

### Added

#### UI Style Guide and Token System
- **UI Token System** (`godot/arcadia_godot_client/docs/ui_tokens.md`)
  - Color Tokens: 深色基调（bg_darkest/dark/panel）+ 语义色（health_red/spirit_blue/loot_gold/warning_yellow/danger_red/success_green）
  - Typography Tokens: 5 级字号（24px/18px/14px/12px/10px）
  - Spacing Tokens: 8px 基准（4/8/12/16/24px）
  - Motion Tokens: 4 档过渡（0s/0.1s/0.2s/0.3s）
  - Component Spec: Button/Panel/ProgressBar/Slot/Tooltip/HUD Bar/Cast Bar
  - Performance Budget: 60 FPS稳定 + <20 draw calls + <100 UI nodes + <4MB atlas

- **Profiling Checklist** (8 个方面)
  - FPS Profiling（帧率分析）
  - Draw Call Profiling（绘制调用分析）
  - Memory Profiling（内存分析）
  - Node Hierarchy Profiling（节点层级分析）
  - Batching Profiling（批处理分析）
  - Animation Profiling（动画分析）
  - Input Latency Profiling（输入延迟分析）
  - Mod Override Profiling（Mod 覆盖分析）

#### Five Core UI Screens
- **Main Menu** (`godot/arcadia_godot_client/scenes/main_menu.tscn`)
  - Start Game/Settings/Exit buttons
  - Theme-consistent styling (dark background + gold accents)
  - Screenshot: `.tmp/ui/main_menu.png` (10KB)

- **Settings** (`godot/arcadia_godot_client/scenes/settings.tscn`)
  - Graphics/Audio/Controls settings
  - Volume sliders + VSync/Fullscreen checkboxes
  - Screenshot: `.tmp/ui/settings.png` (11KB)

- **Inventory** (`godot/arcadia_godot_client/scenes/inventory.tscn`)
  - Carried items grid (20 slots) + Safe box grid (介子袋, 9 slots)
  - Sort/Organize button
  - Screenshot: `.tmp/ui/inventory.png` (12KB)

- **Dungeon HUD** (`godot/arcadia_godot_client/scenes/hud.tscn`)
  - HP bar (health_red) + Spirit bar (spirit_blue)
  - Evacuation cast bar placeholder
  - Loot prompt area
  - Screenshot: `.tmp/ui/hud.png` (9.2KB)

- **Loot Prompt** (`godot/arcadia_godot_client/scenes/loot_prompt.tscn`)
  - Item name and count display (loot_gold)
  - 10-second protection period timer (danger_red)
  - Pickup action prompt ([E] key)
  - Screenshot: `.tmp/ui/loot_prompt.png` (11KB)

#### Client Render Enhancements
- **UI Non-Authoritative Scenarios**
  - UI inventory operations: optimistic preview + server-authoritative revert
  - UI evacuation cast bar: server `EvacStatus` messages drive display

- **Fog Of War And Lighting Integration**
  - Unexplored area fog + tactical fog
  - UI minimap integration (future)
  - Lighting effects on scene (not HUD)
  - Loot prompts respect scene lighting

- **Mod UI Asset Override System**
  - Mod overrides button texture/theme color
  - Asset validation with fallback to base game
  - Priority conflict resolution with queryable candidate list
  - Override logging (ResourceKey + Mod ID + Priority)

### Changed
- **client-render spec**: Merged UI Art Direction requirements into existing spec
  - Added 4 new scenarios for Render Layer Separation
  - Added Fog Of War And Lighting requirement (3 scenarios)
  - Added Client Mod Networking Disabled requirement
  - Added Mod UI Asset Override requirement (4 scenarios)

### Verified
- **UI Style Consistency Check Report** (`.tmp/ui_style_consistency_report.md`)
  - ✅ All five screens pass consistency check
  - ✅ Color system consistency (dark theme + semantic colors)
  - ✅ Font hierarchy consistency (24px/18px/14px/12px)
  - ✅ Panel/Button style consistency (unified Theme resource)
  - ✅ Interaction feedback consistency (gold focus/red warning/blue feedback)

### Archived
- **mvp-ui-art-direction** (`openspec/changes/archive/2026-01-03-mvp-ui-art-direction/`)
  - Status: 15/15 tasks complete (100%)
  - Verification: UI consistency check passed
  - Specs promoted to `openspec/specs/`:
    - `ui-style/spec.md` (UI Token System + Five Core Screens + Style Guide)
    - `client-render/spec.md` (merged: UI non-authoritative + FOW/lighting + Mod override)

---

## [v1.0.0-milestone] - 2026-01-03

### 🎉 v1.0.0 Must 项完成 (12/12, 100%)

This milestone marks the completion of all v1.0.0 Must items, delivering a complete playable dungeon loop with full server authority, anti-cheat infrastructure, and audit reconstruction capabilities.

### Added

#### Core Infrastructure
- **Zone Server Authority** (`src/Arcadia.Server/Zone/ZoneServerHost.cs`)
  - Server-authoritative tick loop (20 Hz fixed timestep)
  - Player entity state management with position tracking
  - Sequence number-based replay protection for movement intents
  - Speed enforcement (100 units/s) with direction vector normalization

#### Gateway Authentication
- **Token-based Authentication** (`openspec/changes/archive/mvp-gateway-auth/`)
  - Gateway issues signed JWT tokens with `kid` (key ID)
  - Zone Server validates tokens without shared secret
  - Support for key rotation without service restart
  - Negative test coverage: Invalid token rejection

#### Dungeon Zone Features
- **Area-of-Interest (AOI) System** (`src/Arcadia.Core/Aoi/GridAoi.cs`)
  - Grid-based 9-cell visibility filtering (64 units/grid)
  - Position indexing updated every tick
  - Supports 64 players/line with efficient message broadcasting

- **Server-Authoritative Combat** (`src/Arcadia.Server/Systems/CombatSystem.cs`)
  - Tick-based combat loop (cooldown/range/damage/death)
  - Health and CombatStats components
  - Death queue flushing with killer information

- **Death and Loot System** (`src/Arcadia.Server/Zone/ZoneLootService.cs`)
  - Full-drop on death (carried items only, safe box excluded)
  - 10-second killer-party loot protection
  - Atomic drop operation: `DropAllCarriedToNewLootAsync`
  - Loot container with pickup permission checks (`LootContainer.CanPickup`)

- **Inventory and Safe Box** (`src/Arcadia.Core/Items/Inventory.cs`)
  - 9-slot safe box (介子袋) with hard capacity constraint
  - Carried items (unlimited, but dropped on death)
  - Item store abstraction: `IItemStore` with PostgreSQL and in-memory implementations

- **Evacuation System** (`src/Arcadia.Server/Zone/ZoneServerHost.cs` lines 44-49, 192-231, 265-295)
  - 10-second cast timer with tick-based progress checking
  - Movement interruption (any movement intent cancels evacuation)
  - High cost placeholder (100 gold, to be replaced with evacuation item)
  - Real-time status broadcasting (`EvacStatus`: casting/completed/interrupted)

- **Disconnect and Reconnect Semantics** (`src/Arcadia.Server/Zone/ZoneSessionManager.cs`)
  - 60-second disconnect timeout (avatar remains in-world, vulnerable)
  - Reconnect decision: `entrance` (dungeon reset) or `resume` (original position)
  - Session state management with `OnDisconnect`/`OnReconnect` handlers

#### Audit and Observability
- **Audit Chain Reconstruction** (`src/Arcadia.Server/Audit/`)
  - `IAuditSink` interface with console and PostgreSQL implementations
  - "DropCarried" audit event: EntityId/KillerPartyId/ItemCount
  - "PickupLoot" audit event: LootId/PickerPartyId/ItemCount
  - Complete Kill → Drop → Loot audit trail

#### Client and UI
- **Godot Client Integration** (`godot/arcadia_godot_client/`)
  - ENet client transport with callback-based message handling
  - Input-to-intent translation (WASD → MoveIntent, E → PickupIntent, Evac → EvacIntent)
  - Client-side prediction with server reconciliation (lerp interpolation)
  - No client-side authority (all game logic server-side)

- **UI Theme and Style Guide** (`godot/arcadia_godot_client/theme/arcadia_theme.tres`)
  - Dark theme with warm accents (loot_gold 1.0,0.8,0.3) and cool highlights (spirit_blue 0.5,0.8,1.0)
  - Font hierarchy: 24px/18px/14px/12px/10px
  - Spacing system: 8px base (4/8/12/16/24px)
  - Animation tokens: 0.1s/0.2s/0.3s transitions
  - Performance budget: 60 FPS stable, <20 draw calls, <100 UI nodes

- **Five Core UI Screens** (`.tmp/ui/`)
  - Main Menu (`main_menu.png`, 10KB)
  - Settings (`settings.png`, 11KB)
  - Inventory (`inventory.png`, 12KB) - 20 carried slots + 9 safe box slots + sort button
  - Dungeon HUD (`hud.png`, 9.2KB) - HP/Spirit/EvacBar/LootPrompt placeholders
  - Loot Prompt (`loot_prompt.png`, 11KB) - 10s protection period display

#### Asset Pipeline
- **Asset Validation Tool** (`src/Arcadia.AssetTool/`)
  - Naming/directory/ResourceKey mapping validation
  - Pixel grid compliance checks
  - CLI: `Arcadia.AssetTool validate`

- **Godot Rendering Baseline** (`godot/arcadia_godot_client/`)
  - 2D layering with lighting and fog
  - Pixel-friendly post-processing shaders
  - Placeholder asset pack (minimal sprites, tilesets, effects)
  - Verification script: `scripts/capture_art_baseline.sh` → `.tmp/art/baseline_main.png`

#### Testing and Verification
- **Automated Playable Slice Test** (`src/Arcadia.LoadTest/PlayableSliceTest.cs`)
  - 6-step automated test: Connect → Move → Kill → Loot Spawn → Pickup → Summary
  - Verdict: PASS (verified 连接→移动→死亡掉落→拾取 loop)
  - Smoke test script: `scripts/smoke_playable_slice.sh`

- **Test Coverage**
  - 34/34 unit tests passing
  - GridAoi: 6 tests
  - CombatSystem: 5 tests
  - LootProtection: 4 tests
  - DisconnectReconnect: 2 tests
  - Inventory/ItemStore: 10+ tests

### Changed
- **Network Transport**: Replaced LiteNetLib placeholder with ENet-based transport (macOS arm64 compatibility)
- **Loot Container**: Added killer-party protection fields (`KillerPartyId`, `ProtectedUntil`, `CanPickup` method)
- **Zone Server**: Integrated AOI, combat system, loot service, and evacuation logic into tick loop

### Fixed
- **Nullable Value Access**: Removed `.Value` from non-nullable `ZoneSnapshot` in `PlayableSliceTest.cs`
- **Grep Pattern Mismatch**: Fixed smoke test script pattern from `Verdict|PASS` to `Verdict.*PASS`
- **Variable Scope Conflict**: Removed duplicate `var now` declaration in `ZoneServerHost.cs` line 311

### Archived
- **v1-playable-dungeon-slice** (`openspec/changes/archive/2026-01-03-v1-playable-dungeon-slice/`)
  - Status: 12/12 tasks complete (100%)
  - Verification: Automated smoke test passing
  - Specs promoted to `openspec/specs/`:
    - `playable-slice/spec.md` (First playable loop + audit reconstruction)
    - `dungeon-zone/spec.md` (Server authority for movement, evacuation, death, loot)
    - `client-render/spec.md` (Render layer separation, input-to-intent, client prediction)

### Documentation
- **CHECKPOINT.md Updated**:
  - Section 14: Evacuation mechanism implementation (2026-01-03)
  - Section 15: RunningHub pixel art tool resource (2026-01-03)
- **Pixel Art Tool**: Documented https://www.runninghub.cn/ai-detail/1957729299266727938 with prompt templates for 5 asset categories

---

## [mvp-gateway-auth] - 2026-01-02

### Added
- Gateway authentication with signed JWT tokens
- Token rotation support with `kid` (key ID)
- Zone Server token validation without shared secret
- Smoke test: `scripts/smoke_enet.sh` (positive + negative cases)

### Archived
- `openspec/changes/archive/mvp-gateway-auth/` (completion date: 2026-01-02)

---

## Project Setup - 2026-01-01

### Added
- Initial project structure with OpenSpec integration
- C# solution: Gateway, Zone Server, Core, Tests
- PostgreSQL persistence layer with auto-migration
- In-memory persistence fallback
- ECS (Entity-Component-System) foundation
- Fixed-tick game loop (20 Hz)
- Logging infrastructure with pipe-delimited format

---

[Unreleased]: https://github.com/your-org/arcadia/compare/v1.0.0-milestone...HEAD
[v1.0.0-milestone]: https://github.com/your-org/arcadia/releases/tag/v1.0.0-milestone
[mvp-gateway-auth]: https://github.com/your-org/arcadia/releases/tag/mvp-gateway-auth
