## 1. Specification
- [x] 1.1 Review charter UI acceptance (v1.0.0) and lock key screens list
- [x] 1.2 Add UI style guide requirements and scenarios
- [x] 1.3 Refine client-render requirements for UI (non-authoritative + FOW/lighting integration + mod override)
- [x] 1.4 Validate OpenSpec strictly

## 2. Design (Godot UI System)
- [x] 2.1 Define UI token system (colors/typography/spacing/motion)
- [x] 2.2 Define component library scope (buttons, panels, slots, tooltips, HUD bars, cast bar)
- [x] 2.3 Define performance budget (FPS, draw calls, UI batching) and profiling checklist

## 3. Build (Implementation)
- [x] 3.1 Implement global Theme + StyleBoxes + icon atlas pipeline
- [x] 3.2 Implement inventory UI (carried/safe box) + sort/organize interactions
- [x] 3.3 Implement dungeon HUD (hp/spirit/skills + evacuation cast bar)
- [x] 3.4 Implement loot prompt + pickup feedback (incl. 10s protection hint)
- [x] 3.5 Implement settings menu (graphics/audio/controls) and main menu
- [x] 3.6 Add screenshot-based acceptance artifacts (stored under `.tmp/ui/` during CI/local runs)

## 4. Verification
- [x] 4.1 Manual runbook: capture screenshots/short clips for the five key screens
- [x] 4.2 Confirm style guide completeness and consistency across screens

