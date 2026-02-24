## 1. Specification
- [x] 1.1 Lock "first playable" acceptance criteria (loop + rules + observability)
- [x] 1.2 Validate OpenSpec strictly

## 2. Client (Godot Render Layer)
- [x] 2.1 Godot client: connect to Zone, show connection state
- [x] 2.2 Player movement input → intent → server-authoritative position update (client prediction placeholder)
- [x] 2.3 Minimal HUD: HP/Spirit placeholders + evacuation cast bar placeholder
- [x] 2.4 Loot prompt + pickup feedback placeholder

## 3. Zone Server (Authoritative Slice)
- [x] 3.1 Minimal player entity state + authoritative movement
- [x] 3.2 Death trigger placeholder + full-drop (safe box excluded)
- [x] 3.3 Loot pickup protection (10s killer-party) + audit
- [x] 3.4 Evacuation point (long cast, interruptible, high cost placeholder)

## 4. Verification
- [x] 4.1 Smoke: connect + move + pickup + evac + death/drop (local)
- [x] 4.2 Export visuals: `scripts/capture_art_baseline.sh` + HUD screenshots pack

