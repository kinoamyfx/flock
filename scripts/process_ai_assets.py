#!/usr/bin/env python3
"""
AI 生成素材后处理脚本
调整尺寸到规范并验证命名
"""

import os
import sys
from pathlib import Path
from PIL import Image

# 尺寸规范
SPECS = {
    # 主角: 48×48
    'char_adventurer': (48, 48),

    # NPC: 32×32
    'npc_merchant': (32, 32),
    'npc_guard': (32, 32),
    'npc_mystic': (32, 32),

    # 怪物: 24×24 ~ 64×64
    'enemy_slime': (24, 24),
    'enemy_skeleton': (32, 32),
    'enemy_ghost': (40, 40),
    'enemy_orc': (48, 48),
    'enemy_dragon': (64, 64),
    'enemy_boss': (64, 64),

    # Tile: 16×16
    'tile_floor': (16, 16),
    'tile_wall': (16, 16),
    'decoration': (16, 16),

    # 道具: 32×32
    'item_weapon': (32, 32),
    'item_armor': (32, 32),
    'item_consumible': (32, 32),
    'item_special': (32, 32),

    # 特效: 64×64 ~ 96×96
    'fx_sword': (64, 64),
    'fx_arrow': (64, 64),
    'fx_fire': (64, 64),
    'fx_ice': (96, 96),
    'fx_thunder': (96, 96),
    'fx_heal': (96, 96),

    # 背景: 1280×720
    'bg_town': (1280, 720),
    'bg_dungeon': (1280, 720),
}

def get_target_size(filename):
    """根据文件名确定目标尺寸"""
    for key, size in SPECS.items():
        if key in filename:
            return size

    # 默认尺寸
    return (32, 32)

def process_image(input_path, output_path):
    """处理单张图片：调整尺寸并确保 RGBA 格式"""
    try:
        img = Image.open(input_path)
        target_size = get_target_size(input_path.name)

        # 调整尺寸（nearest-neighbor 保持像素清晰）
        img_resized = img.resize(target_size, Image.NEAREST)

        # 确保 RGBA 格式
        if img_resized.mode != 'RGBA':
            img_resized = img_resized.convert('RGBA')

        # 保存
        img_resized.save(output_path, 'PNG')
        print(f"✅ Processed: {input_path.name} → {target_size[0]}×{target_size[1]}")

    except Exception as e:
        print(f"❌ Error processing {input_path.name}: {e}")

def main():
    # 输入/输出目录
    input_dir = Path('.tmp/arcadia-assets-generated')
    output_dir = Path('.tmp/arcadia-assets-processed')

    if not input_dir.exists():
        print(f"❌ 输入目录不存在: {input_dir}")
        print("\n请先运行生成脚本或将 AI 生成的图片放入该目录")
        sys.exit(1)

    # 创建输出目录
    output_dir.mkdir(parents=True, exist_ok=True)

    # 查找所有 PNG 文件
    png_files = list(input_dir.rglob('*.png'))

    if not png_files:
        print(f"❌ 未找到 PNG 文件: {input_dir}")
        sys.exit(1)

    print(f"🎨 找到 {len(png_files)} 张图片")
    print("="*60)

    # 处理每张图片
    for input_path in png_files:
        # 保持相对路径结构
        rel_path = input_path.relative_to(input_dir)
        output_path = output_dir / rel_path

        # 创建输出子目录
        output_path.parent.mkdir(parents=True, exist_ok=True)

        # 处理图片
        process_image(input_path, output_path)

    print("="*60)
    print(f"✅ 处理完成！")
    print(f"📂 输出目录: {output_dir.absolute()}")
    print(f"📊 处理数量: {len(png_files)} 张")
    print("\n下一步：")
    print("1. 检查输出图片质量")
    print("2. 移动到资源目录: cp -r {output_dir}/* assets/")
    print("3. 运行校验: Arcadia.AssetTool validate --dir assets/")

if __name__ == '__main__':
    main()
