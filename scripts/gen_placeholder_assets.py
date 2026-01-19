#!/usr/bin/env python3
"""
Generate a minimal v1.0.0 placeholder asset pack (pixel art) under ./assets.

Why: v1.0.0 requires a minimal asset pack to validate pipeline + ResourceKey mapping + mod overrides.
Context: Option A (high-density pixel art) + strong glow/fog/particles; placeholders must still obey naming rules.
Attention: This script is deterministic and only writes files that are missing (non-destructive by default).
"""

from __future__ import annotations

import os
import struct
import zlib


def _crc32(data: bytes) -> int:
    return zlib.crc32(data) & 0xFFFFFFFF


def _chunk(chunk_type: bytes, data: bytes) -> bytes:
    return struct.pack(">I", len(data)) + chunk_type + data + struct.pack(">I", _crc32(chunk_type + data))


def write_png(path: str, width: int, height: int, pixels_rgba: bytes) -> None:
    if len(pixels_rgba) != width * height * 4:
        raise ValueError("pixels_rgba length mismatch")

    # PNG signature
    out = bytearray(b"\x89PNG\r\n\x1a\n")

    # IHDR: 8-bit RGBA
    ihdr = struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0)
    out += _chunk(b"IHDR", ihdr)

    # IDAT: zlib-compressed scanlines (filter type 0)
    scanlines = bytearray()
    stride = width * 4
    for y in range(height):
        scanlines.append(0)
        scanlines += pixels_rgba[y * stride : (y + 1) * stride]
    compressed = zlib.compress(bytes(scanlines), level=6)
    out += _chunk(b"IDAT", compressed)

    out += _chunk(b"IEND", b"")

    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(out)


def solid_rgba(width: int, height: int, rgba: tuple[int, int, int, int]) -> bytes:
    r, g, b, a = rgba
    return bytes([r, g, b, a]) * (width * height)


def checker_rgba(width: int, height: int, a: tuple[int, int, int, int], b: tuple[int, int, int, int], cell: int = 4) -> bytes:
    out = bytearray()
    for y in range(height):
        for x in range(width):
            use_a = ((x // cell) + (y // cell)) % 2 == 0
            out += bytes(a if use_a else b)
    return bytes(out)


def ensure_png(path: str, width: int, height: int, pixels: bytes) -> None:
    if os.path.exists(path):
        return
    write_png(path, width, height, pixels)


def main() -> int:
    root = os.path.join(os.getcwd(), "assets")

    # Tilesets
    ensure_png(
        os.path.join(root, "tiles", "biome", "temperate", "tile_temperate_grass_0.png"),
        16,
        16,
        checker_rgba(16, 16, (34, 110, 50, 255), (28, 95, 44, 255), cell=4),
    )
    ensure_png(
        os.path.join(root, "tiles", "biome", "temperate", "tile_temperate_dirt_0.png"),
        16,
        16,
        checker_rgba(16, 16, (96, 62, 38, 255), (84, 54, 33, 255), cell=4),
    )

    ensure_png(
        os.path.join(root, "tiles", "dungeon", "cave", "tile_cave_floor_0.png"),
        16,
        16,
        checker_rgba(16, 16, (55, 55, 60, 255), (45, 45, 50, 255), cell=4),
    )
    ensure_png(
        os.path.join(root, "tiles", "dungeon", "cave", "tile_cave_wall_0.png"),
        16,
        16,
        checker_rgba(16, 16, (32, 32, 36, 255), (26, 26, 30, 255), cell=4),
    )

    # Props/buildings (10)
    for i in range(10):
        ensure_png(
            os.path.join(root, "props", f"prop_workshop_{i}.png"),
            24,
            24,
            checker_rgba(24, 24, (120, 84, 52, 255), (100, 70, 44, 255), cell=3),
        )

    # Player character set (minimal frames)
    for anim in ["idle", "run", "attack", "cast", "hit", "death"]:
        for frame in range(4):
            ensure_png(
                os.path.join(root, "chars", "player", "main", f"char_player_main_{anim}_s_{frame}.png"),
                32,
                32,
                checker_rgba(32, 32, (200, 200, 210, 255), (160, 160, 175, 255), cell=4),
            )

    # NPC/monster sets (3)
    for monster in ["slime", "rat", "bandit"]:
        for anim in ["idle", "run", "attack", "hit", "death"]:
            for frame in range(3):
                ensure_png(
                    os.path.join(root, "chars", "npc", monster, f"char_npc_{monster}_{anim}_s_{frame}.png"),
                    32,
                    32,
                    checker_rgba(32, 32, (120, 220, 140, 255), (70, 160, 90, 255), cell=4),
                )

    # Item icons (40)
    for i in range(40):
        ensure_png(
            os.path.join(root, "items", "icons", f"icon_item_{i:02d}.png"),
            16,
            16,
            checker_rgba(16, 16, (220, 190, 90, 255), (170, 140, 60, 255), cell=2),
        )

    # VFX (12)
    for i in range(12):
        ensure_png(
            os.path.join(root, "vfx", f"vfx_hit_spark_{i:02d}.png"),
            16,
            16,
            checker_rgba(16, 16, (255, 220, 140, 255), (255, 150, 80, 255), cell=1),
        )

    # UI placeholders (minimal)
    ensure_png(os.path.join(root, "ui", "ui_panel_default.png"), 32, 32, solid_rgba(32, 32, (20, 20, 24, 255)))
    ensure_png(os.path.join(root, "ui", "ui_button_default.png"), 32, 16, solid_rgba(32, 16, (40, 40, 46, 255)))
    ensure_png(os.path.join(root, "ui", "ui_slot_default.png"), 16, 16, solid_rgba(16, 16, (55, 55, 62, 255)))

    print("Placeholder assets ensured under ./assets")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

