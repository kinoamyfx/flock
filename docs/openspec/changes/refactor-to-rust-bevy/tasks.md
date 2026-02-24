# Tasks: Custom WASM Microkernel Engine

## Phase 1: Workspace & ABI Boundary
- [ ] Initialize Cargo workspace (`src/`).
- [ ] Setup `src/abi` using WIT (WebAssembly Interface Type) or basic `extern "C"` functions for memory sharing.
- [ ] Define the ECS Data Layout that will be shared between Host (`engine`) and Guest (`core_wasm`).

## Phase 2: The Host Engine (`src/engine`)
- [ ] Setup `winit` for window management.
- [ ] Setup raw `wgpu` instance, surface, device, and a basic 2D Quad/Sprite rendering pipeline.
- [ ] Integrate `wasmtime` engine, module compilation, and linker.
- [ ] Implement the Host functions exposed to WASM (e.g., `host_draw_sprite`, `host_send_network_packet`).

## Phase 3: The WASM Gameplay Logic (`src/core_wasm`)
- [ ] Configure `core_wasm` crate for `wasm32-unknown-unknown` target.
- [ ] Export the `update(delta_time: f32)` function via ABI.
- [ ] Re-implement GridAoi, Combat Math, and Movement logic entirely in WASM.
- [ ] Push draw calls across the ABI boundary to the host engine.

## Phase 4: Networking & Multiplayer
- [ ] Implement UDP Server (`std::net::UdpSocket` or `tokio`) within `src/engine`.
- [ ] Route incoming UDP packets into the WASM module via an `on_packet_received(ptr, len)` ABI call.
- [ ] Implement Client-Side Prediction loop in WASM.

## Phase 5: The Mod Development Kit (`src/mdk`)
- [ ] Create `arcadia-mdk` crate wrapping the ABI calls into a safe, idiomatic Rust API.
- [ ] Build a sample Mod (`HelloWorldItem`) compiled to WASM.
- [ ] Write the Host loader logic to dynamically find `.wasm` files in a `mods/` directory and link them alongside `core_wasm`.
