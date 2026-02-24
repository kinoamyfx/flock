## 1. Specification
- [x] 1.1 Lock art direction option (pixel vs painted vs vector) with owner
- [x] 1.2 Add Art Bible requirements (style, material, lighting, VFX, UI consistency)
- [x] 1.3 Add v1.0.0 minimal Asset Pack requirements (tiles/characters/items/VFX)
- [x] 1.4 Refine client-render requirements for cool visuals (lighting/fog/postfx) and pipeline
- [x] 1.5 Validate OpenSpec strictly

## 2. Design (Pipeline)
- [x] 2.1 Define asset naming + folder conventions (Godot import friendly)
- [x] 2.2 Define atlas/packing strategy and LOD policy (if any)
- [x] 2.3 Define mod override priority rules + audit visibility (what got overridden)
- [x] 2.4 Define performance budgets and profiling checklist

## 3. Build (Implementation)
- [x] 3.1 Implement Godot import pipeline + validators (naming, resolution, compression)
- [x] 3.2 Implement lighting + fog + postfx baseline
- [x] 3.3 Add starter asset pack (placeholder allowed only if style constraints are met)
- [x] 3.4 Add screenshot/clip based acceptance artifacts (via `scripts/capture_art_baseline.sh`)

## 4. Godot C# (NuGet)
- [x] 4.1 Create a Godot C# project using `Godot.NET.Sdk` (version pinned)
- [x] 4.2 Ensure it builds via `dotnet build` without requiring Godot binary
