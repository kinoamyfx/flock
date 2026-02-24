# Proposal: Refactor Arcadia to Custom WASM Microkernel Engine

## Objective
Migrate the entire Arcadia project (currently C# microservices + Godot/C# client) to a **Custom WebAssembly (WASM) Microkernel Engine built in Rust**. This is a radical pivot from adopting Bevy. The host engine will act solely as a thin runtime wrapper (providing Winit windowing, WGPU rendering, Networking, and Wasmtime scheduling). **All gameplay logic, ECS systems, and modifications will be executed as secure, sandboxed WASM modules.**

## Motivation
- **Ultimate Modding Freedom ("Everything is a Mod")**: Blurring the lines between official core gameplay and community modifications. The entire game logic is hot-swappable WASM bytecode.
- **WASM Ecosystem & Wasmtime**: Leveraging `wasmtime`'s JIT (Cranelift), strict linear memory isolation, fuel consumption (for infinite loop protection), and the **Component Model** for seamless complex-type passing across the ABI boundary.
- **Performance & Security**: The host engine is uncrashable by bad logic. Modules run at 80-90% native speed.
- **Unified Client/Server Architecture**: The client and server are just different WASM compositions loaded by the same thin host engine executable.

## Scope of Changes

### 1. Project Structure (Radical Restructuring)
The Cargo workspace `src/` will be organized into:
- `src/engine` (The thin Rust Host: `wasmtime` runtime, WGPU renderer, ENet/UDP networking).
- `src/abi` (The shared WASM ABI definitions, likely using `wit-bindgen` to define the Component Model interface).
- `src/core_wasm` (The official Arcadia gameplay logic compiled to `wasm32-wasi` or `wasm32-unknown-unknown`).
- `src/mdk` (Mod Development Kit: Toolchain and macros for users to write their own WASM modules).

### 2. The Engine Microkernel (`src/engine`)
- **ECS Memory Management**: The host will maintain a packed linear memory block for the Entity-Component data, passing pointers to the WASM modules, or utilizing the WASM Component Model to pass ECS deltas.
- **WGPU Rendering**: A minimal 2D sprite/tilemap renderer built on raw `wgpu`. The WASM logic will emit "Draw Commands" across the ABI boundary.
- **Networking**: Raw UDP socket management in the host, emitting `OnNetworkPacket` events into the WASM logic.

### 3. The WASM Logic (`src/core_wasm`)
- **Systems & Rules**: Movement, combat math, inventory management, and AOI (Area of Interest) calculations are pure Rust compiled to WASM.
- **Client Prediction**: The same WASM movement logic module can be loaded by the client engine for prediction, and by the server engine for authoritative validation.

## Constraints & Risks
- **Extremely High Engineering Complexity**: We are building a custom engine and a WASM runtime boundary simultaneously. We lose Bevy's robust ecosystem (UI, Asset Loading, Bevy ECS).
- **Time/Cost**: The timeline for achieving a playable MVP stretches significantly compared to utilizing off-the-shelf frameworks.
- **ABI Overhead**: Care must be taken to minimize serialization/deserialization costs across the Host/WASM boundary during high-frequency tasks (like rendering 10,000 sprites).

## Sign-off
Owner approved the radical "WASM Microkernel" architecture. The development will pivot away from Bevy toward building a custom `wgpu` + `wasmtime` engine.
