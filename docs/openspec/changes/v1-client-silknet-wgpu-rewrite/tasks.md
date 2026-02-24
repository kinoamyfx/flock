## 1. Specification
- [ ] 1.1 Add spec deltas for the Silk.NET client rewrite
- [ ] 1.2 Validate OpenSpec strictly

## 2. Architecture & Design
- [ ] 2.1 Define client module boundaries (render/input/net/state cache)
- [ ] 2.2 Define rendering approach (2D sprite batching + lighting/fog hooks)
- [ ] 2.3 Define UI rendering approach (reuse tokens + theme rules)
- [ ] 2.4 Define performance budget and profiling checklist

## 3. Build (Implementation)
- [ ] 3.1 Minimal window + render loop (hello scene)
- [ ] 3.2 Load a sprite + draw at fixed resolution with pixel-friendly scaling
- [ ] 3.3 Integrate network handshake (Hello/Welcome) and snapshots rendering
- [ ] 3.4 Implement the five key UI screens parity (main/settings/inventory/hud/loot prompt)

## 4. Verification
- [ ] 4.1 `dotnet build Arcadia.sln`
- [ ] 4.2 `dotnet test Arcadia.sln`
- [ ] 4.3 Screenshot artifacts generated for UI regression

