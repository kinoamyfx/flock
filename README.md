# Flock Engine & Arcadia

**Flock** is a radically minimalistic, WASM-driven microkernel game engine. It acts as a strict host runtime (wgpu + wasmtime + winit) where **all** game logic—including the official ECS and all modding capabilities—runs securely within WebAssembly modules.

**Arcadia** is the flagship 2D multiplayer ARPG built entirely as a suite of WASM modules running on top of the Flock engine.

## Architecture
- `src/flock`: The host microkernel (Native Rust, Wasmtime, WGPU).
- `src/flock_abi`: The WIT/Bincode boundary definitions connecting the Host and Guest WASM plugins.
- `src/core_wasm`: Arcadia's authoritative game logic compiled to `wasm32`.
- `src/mdk`: The Mod Development Kit (built on top of `flock_abi`) for community developers to write WASM mods.
