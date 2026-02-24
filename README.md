# Flock WASM Engine

**Flock** is a radically minimalistic, WASM-driven microkernel game engine built in Rust. It acts as a strict host runtime where **all** game logic runs securely within WebAssembly modules, while the host provides raw system services (rendering, networking, windowing).

## Architecture
- `src/flock`: The host microkernel (Native Rust, Wasmtime, WGPU).
- `src/flock_abi`: The boundary definitions (e.g., Component Model WIT or serialized structs) connecting the Host and Guest WASM modules.

## Philosophy
- **Bring Your Own Logic**: The engine has no concept of ECS, Entities, or specific game rules.
- **WASM First**: Games built for Flock are compiled to `.wasm` files.
- **Microkernel Security**: Linear memory sandboxing prevents game logic or community mods from crashing the host or accessing arbitrary system resources.
