# UI Style Specification

## Purpose
Define the v1.0.0 UI style guide, key screens, and verification evidence so UI changes remain consistent and regression-testable.

## Requirements

### Requirement: v1.0.0 Key Screens List (Locked)
The client MUST deliver the following five key screens for v1.0.0, each meeting the UI acceptance criteria defined in the charter.

#### Key Screens Locked for v1.0.0
1. **Main Menu** (`main_menu.tscn`)
   - Start Game / Settings / Exit buttons
   - Game title display
   - Theme-consistent styling (dark background + gold accents)

2. **Settings** (`settings.tscn`)
   - Graphics settings (resolution, fullscreen, VSync)
   - Audio settings (master volume, music, SFX)
   - Controls settings (keybindings placeholder)
   - Apply/Cancel buttons with theme styling

3. **Inventory** (`inventory.tscn`)
   - Carried items grid (20 slots)
   - Safe box grid (介子袋, 9 slots)
   - Sort/Organize button
   - Item tooltips on hover (future)

4. **Dungeon HUD** (`hud.tscn`)
   - HP bar (health_red semantic color)
   - Spirit/Energy bar (spirit_blue semantic color)
   - Evacuation cast bar (warning_yellow during cast)
   - Loot prompt area (loot_gold for item names)

5. **Loot Prompt** (`loot_prompt.tscn`)
   - Item name and count display
   - 10-second protection period timer
   - Killer-party exclusive pickup hint
   - Pickup action prompt (E key)

#### Scenario: All key screens delivered
- **WHEN** v1.0.0 is prepared for release
- **THEN** all five key screens MUST be implemented and verified with screenshots
- **AND THEN** screenshots MUST be stored under `.tmp/ui/` for regression testing

#### Verification (Completed 2026-01-03)
- ✅ Screenshots generated: `.tmp/ui/*.png` (5 files, total ~53KB)
- ✅ Script: `scripts/capture_ui_screenshots.sh`
- ✅ Theme applied: `arcadia_theme.tres`

---

### Requirement: UI Style Guide (v1.0.0)
The client MUST ship a UI style guide that defines colors, typography, iconography, panel materials, motion rules, and accessibility constraints.

#### Scenario: Style guide completeness
- **WHEN** v1.0.0 is prepared for release
- **THEN** the style guide MUST include the following components:
  - **Color Tokens**: bg_darkest/dark/panel, text_primary/secondary/disabled, health_red, spirit_blue, loot_gold, warning_yellow
  - **Typography Tokens**: font_size_h1 (24px), h2 (18px), body (14px), small (12px), tiny (10px)
  - **Spacing Tokens**: spacing_xs (4px), s (8px), m (12px), l (16px), xl (24px)
  - **Motion Tokens**: transition_fast (0.1s), normal (0.2s), slow (0.3s)
  - **Component Templates**: Button (normal/hover/pressed), Panel (border/corner radius), ProgressBar (fill/background), Slot (empty/filled/hover), Tooltip (position/delay)
  - **Performance Budget**: 60 FPS stable, <20 draw calls, <100 UI nodes

#### Verification (Completed 2026-01-03)
- ✅ Style Guide documented: `[已删除] docs/ui_tokens.md`
- ✅ Theme resource created: `[已删除] theme/arcadia_theme.tres`

---

### Requirement: Key Screens Follow One Style
The client MUST apply the style guide consistently across all five key screens.

#### Scenario: Screen consistency
- **WHEN** a player navigates between key screens
- **THEN** colors, fonts, icons, panel materials, and motion MUST remain consistent with the style guide
- **AND THEN** semantic colors MUST be used correctly (e.g., health_red for HP, loot_gold for item names)

#### Scenario: Theme resource reuse
- **WHEN** a new UI element is added
- **THEN** it MUST reference `arcadia_theme.tres` instead of defining inline styles
- **AND THEN** any style customization MUST be recorded in `ui_tokens.md`

### Requirement: UI Performance Budget (PC)
The client MUST define and meet a UI performance budget for v1.0.0 on PC.

#### Scenario: Minimum performance
- **WHEN** the game runs on the v1.0.0 target PC baseline
- **THEN** UI rendering MUST remain responsive and MUST NOT introduce visible stutters during normal interaction
