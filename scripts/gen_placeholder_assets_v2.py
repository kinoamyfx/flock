#!/usr/bin/env python3
"""
Arcadia 像素风素材占位符生成器
生成符合命名规范的彩色方块占位符，用于快速验收整体效果
"""

import os
from pathlib import Path
from PIL import Image, ImageDraw

# 色板定义
COLORS = {
    # 背景基调
    'bg_darkest': (13, 13, 19),
    'bg_dark': (20, 20, 25),
    'bg_panel': (30, 30, 38),

    # 语义色彩
    'health_red': (230, 57, 70),
    'spirit_blue': (72, 202, 228),
    'loot_gold': (255, 183, 3),
    'warning_yellow': (251, 133, 0),
    'danger_red': (208, 0, 0),
    'success_green': (6, 214, 160),

    # 光照色彩
    'flame_orange': (255, 107, 53),
    'frost_cyan': (144, 224, 239),
    'toxic_green': (112, 224, 0),
    'shadow_purple': (114, 9, 183),
}

def create_solid_color_image(size, color, output_path):
    """创建纯色图像"""
    if isinstance(color, tuple) and len(color) == 4:
        # 已经包含 alpha 通道
        img = Image.new('RGBA', size, color)
    else:
        # 添加 alpha 通道
        img = Image.new('RGBA', size, color + (255,) if len(color) == 3 else color)
    img.save(output_path)
    print(f"✅ Created: {output_path}")

def create_gradient_image(size, color1, color2, output_path):
    """创建渐变图像"""
    img = Image.new('RGBA', size, color1 + (255,))
    draw = ImageDraw.Draw(img)

    # 简单的垂直渐变
    for y in range(size[1]):
        ratio = y / size[1]
        r = int(color1[0] * (1 - ratio) + color2[0] * ratio)
        g = int(color1[1] * (1 - ratio) + color2[1] * ratio)
        b = int(color1[2] * (1 - ratio) + color2[2] * ratio)
        draw.rectangle([(0, y), (size[0], y+1)], fill=(r, g, b, 255))

    img.save(output_path)
    print(f"✅ Created: {output_path}")

def create_character_sprite(size, main_color, accent_color, output_path):
    """创建角色精灵（简单人形）"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))  # 透明背景
    draw = ImageDraw.Draw(img)

    w, h = size
    # 身体（主色）
    body_width = w // 2
    body_height = h // 2
    body_x = (w - body_width) // 2
    body_y = h // 4
    draw.rectangle([body_x, body_y, body_x + body_width, body_y + body_height],
                   fill=main_color + (255,))

    # 头部（主色）
    head_size = w // 3
    head_x = (w - head_size) // 2
    head_y = body_y - head_size // 2
    draw.rectangle([head_x, head_y, head_x + head_size, head_y + head_size],
                   fill=main_color + (255,))

    # 武器/装饰（强调色）
    weapon_width = w // 6
    weapon_height = h // 2
    weapon_x = body_x + body_width
    weapon_y = body_y
    draw.rectangle([weapon_x, weapon_y, weapon_x + weapon_width, weapon_y + weapon_height],
                   fill=accent_color + (255,))

    img.save(output_path)
    print(f"✅ Created: {output_path}")

def create_tile_with_border(size, bg_color, border_color, output_path):
    """创建带边框的 tile"""
    img = Image.new('RGBA', size, bg_color + (255,))
    draw = ImageDraw.Draw(img)

    # 内边框
    border_width = 2
    draw.rectangle([border_width, border_width,
                   size[0]-border_width, size[1]-border_width],
                  outline=border_color + (255,), width=1)

    img.save(output_path)
    print(f"✅ Created: {output_path}")

def main():
    # 输出目录
    output_base = Path('.tmp/arcadia-assets')
    dirs = {
        'char': output_base / 'sprites' / 'char' / 'adventurer',
        'npc': output_base / 'sprites' / 'npc',
        'enemy': output_base / 'sprites' / 'enemy',
        'tiles': output_base / 'tiles',
        'items': output_base / 'items',
        'fx': output_base / 'sprites' / 'fx',
        'backgrounds': output_base / 'backgrounds',
    }

    for dir_path in dirs.values():
        dir_path.mkdir(parents=True, exist_ok=True)

    print("🎨 开始生成 Arcadia 占位符素材...\n")

    # ========== 角色与 NPC ==========
    print("### 1. 角色与 NPC")

    # 主角 - 5 个动作（idle, walk, attack, hurt, death）
    main_char_color = COLORS['spirit_blue']
    main_char_accent = COLORS['loot_gold']

    for action, frames in [('idle', 4), ('walk', 8), ('attack', 6), ('hurt', 3), ('death', 10)]:
        for frame in range(frames):
            output_path = dirs['char'] / f'char_adventurer_{action}_{frame+1:03d}.png'
            create_character_sprite((48, 48), main_char_color, main_char_accent, output_path)

    # NPC Merchant（绿色长袍 + 棕色背包）
    (dirs['npc'] / 'npc_merchant').mkdir(parents=True, exist_ok=True)
    for action, frames in [('idle', 4), ('walk', 8)]:
        for frame in range(frames):
            output_path = dirs['npc'] / 'npc_merchant' / f'npc_merchant_{action}_{frame+1:03d}.png'
            create_character_sprite((32, 32), (34, 139, 34), (139, 69, 19), output_path)

    # NPC Guard（银色盔甲 + 红色披风）
    (dirs['npc'] / 'npc_guard').mkdir(parents=True, exist_ok=True)
    for action, frames in [('idle', 4), ('walk', 8)]:
        for frame in range(frames):
            output_path = dirs['npc'] / 'npc_guard' / f'npc_guard_{action}_{frame+1:03d}.png'
            create_character_sprite((32, 32), (192, 192, 192), COLORS['danger_red'], output_path)

    # NPC Mystic（紫色长袍 + 发光法杖）
    (dirs['npc'] / 'npc_mystic').mkdir(parents=True, exist_ok=True)
    for action, frames in [('idle', 4), ('walk', 8)]:
        for frame in range(frames):
            output_path = dirs['npc'] / 'npc_mystic' / f'npc_mystic_{action}_{frame+1:03d}.png'
            create_character_sprite((32, 32), COLORS['shadow_purple'], COLORS['spirit_blue'], output_path)

    # ========== 怪物 ==========
    print("\n### 2. 怪物")

    # 为每个怪物创建子目录
    for monster_name in ['enemy_slime', 'enemy_skeleton', 'enemy_ghost', 'enemy_orc', 'enemy_dragon', 'enemy_boss']:
        (dirs['enemy'] / monster_name).mkdir(parents=True, exist_ok=True)

    # Slime（史莱姆）- 24×24
    for action, frames in [('idle', 4), ('hurt', 2)]:
        for frame in range(frames):
            output_path = dirs['enemy'] / 'enemy_slime' / f'enemy_slime_{action}_{frame+1:03d}.png'
            create_solid_color_image((24, 24), COLORS['toxic_green'], output_path)

    # Skeleton（骷髅兵）- 32×32
    for action, frames in [('idle', 4), ('attack', 6)]:
        for frame in range(frames):
            output_path = dirs['enemy'] / 'enemy_skeleton' / f'enemy_skeleton_{action}_{frame+1:03d}.png'
            create_solid_color_image((32, 32), (220, 220, 220), output_path)

    # Ghost（幽灵）- 40×40
    for action, frames in [('idle', 4), ('attack', 6)]:
        for frame in range(frames):
            output_path = dirs['enemy'] / 'enemy_ghost' / f'enemy_ghost_{action}_{frame+1:03d}.png'
            create_solid_color_image((40, 40), (COLORS['frost_cyan'][0], COLORS['frost_cyan'][1], COLORS['frost_cyan'][2], 180), output_path)  # 半透明

    # Orc（兽人）- 48×48
    for action, frames in [('idle', 4), ('walk', 8), ('attack', 8)]:
        for frame in range(frames):
            output_path = dirs['enemy'] / 'enemy_orc' / f'enemy_orc_{action}_{frame+1:03d}.png'
            create_solid_color_image((48, 48), (107, 142, 35), output_path)

    # Dragon（幼龙）- 64×64
    for action, frames in [('idle', 4), ('attack', 8)]:
        for frame in range(frames):
            output_path = dirs['enemy'] / 'enemy_dragon' / f'enemy_dragon_{action}_{frame+1:03d}.png'
            create_solid_color_image((64, 64), COLORS['danger_red'], output_path)

    # Boss（黑暗领主）- 64×64
    for action, frames in [('idle', 6), ('attack', 10)]:
        for frame in range(frames):
            output_path = dirs['enemy'] / 'enemy_boss' / f'enemy_boss_{action}_{frame+1:03d}.png'
            create_solid_color_image((64, 64), COLORS['bg_darkest'], output_path)

    # ========== 场景 Tileset ==========
    print("\n### 3. 场景 Tileset")

    # 地面（4 种 × 4 变体）
    floor_colors = [
        (COLORS['bg_panel'], (50, 50, 60)),      # 石板
        (COLORS['bg_dark'], (101, 67, 33)),      # 木板
        (COLORS['danger_red'], (139, 0, 0)),     # 地毯
        ((60, 40, 20), (40, 30, 15)),           # 泥土
    ]

    for i, (bg_color, detail_color) in enumerate(floor_colors, 1):
        for j in range(1, 5):
            output_path = dirs['tiles'] / f'tile_floor_{i:02d}_{j:02d}.png'
            create_tile_with_border((16, 16), bg_color, detail_color, output_path)

    # 墙壁（4 种 × 4 变体）
    wall_colors = [
        (COLORS['bg_panel'], COLORS['bg_darkest']),  # 砖墙
        ((101, 67, 33), (60, 40, 20)),              # 木墙
        ((50, 50, 50), (30, 30, 30)),                # 栅栏
        (COLORS['bg_panel'], COLORS['bg_panel']),     # 暗门
    ]

    for i, (bg_color, detail_color) in enumerate(wall_colors, 1):
        for j in range(1, 5):
            output_path = dirs['tiles'] / f'tile_wall_{i:02d}_{j:02d}.png'
            create_tile_with_border((16, 16), bg_color, detail_color, output_path)

    # 装饰物（5 种）
    decorations = [
        ('decoration_torch_01.png', COLORS['flame_orange']),
        ('decoration_chest_01.png', COLORS['loot_gold']),
        ('decoration_bones_01.png', (220, 220, 220)),
        ('decoration_web_01.png', (200, 200, 200, 128)),  # 半透明
        ('decoration_crack_01.png', COLORS['bg_darkest']),
    ]

    for filename, color in decorations:
        output_path = dirs['tiles'] / filename
        create_solid_color_image((16, 16), color[:3] if len(color) == 3 else color, output_path)

    # ========== 道具图标 ==========
    print("\n### 4. 道具图标")

    # 武器（3 个）
    weapons = [
        ('item_weapon_sword.png', (192, 192, 192)),
        ('item_weapon_staff_fire.png', COLORS['flame_orange']),
        ('item_weapon_bow_ice.png', COLORS['frost_cyan']),
    ]

    for filename, color in weapons:
        output_path = dirs['items'] / filename
        create_solid_color_image((32, 32), color, output_path)

    # 防具（3 个）
    armors = [
        ('item_armor_leather.png', (101, 67, 33)),
        ('item_armor_shield.png', COLORS['spirit_blue']),
        ('item_armor_robe.png', COLORS['shadow_purple']),
    ]

    for filename, color in armors:
        output_path = dirs['items'] / filename
        create_solid_color_image((32, 32), color, output_path)

    # 消耗品（2 个）
    consumables = [
        ('item_consumible_potion.png', COLORS['health_red']),
        ('item_consumible_crystal.png', COLORS['spirit_blue']),
    ]

    for filename, color in consumables:
        output_path = dirs['items'] / filename
        create_solid_color_image((32, 32), color, output_path)

    # 特殊物品（2 个）
    specials = [
        ('item_special_scroll.png', (245, 222, 179)),
        ('item_special_bag.png', COLORS['loot_gold']),
    ]

    for filename, color in specials:
        output_path = dirs['items'] / filename
        create_solid_color_image((32, 32), color, output_path)

    # ========== 特效 ==========
    print("\n### 5. 特效")

    # 为每个特效创建子目录
    for fx_name in ['fx_sword_slash', 'fx_arrow_shot', 'fx_fire_blast', 'fx_ice_storm', 'fx_thunder_strike', 'fx_heal_light']:
        (dirs['fx'] / fx_name).mkdir(parents=True, exist_ok=True)

    # 攻击特效（3 种 × 4-8 帧）
    fx_attacks = [
        ('fx_sword_slash', COLORS['spirit_blue'], 6),
        ('fx_arrow_shot', (255, 255, 255), 4),
        ('fx_fire_blast', COLORS['flame_orange'], 8),
    ]

    for fx_name, color, frames in fx_attacks:
        for frame in range(frames):
            output_path = dirs['fx'] / fx_name / f'{fx_name}_{frame+1:03d}.png'
            # 渐变效果（从中心向外）
            img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
            draw = ImageDraw.Draw(img)
            size = int((frame + 1) * 8)
            draw.ellipse([32-size//2, 32-size//2, 32+size//2, 32+size//2],
                        fill=color + (200 - frame * 25,))
            img.save(output_path)
            print(f"✅ Created: {output_path}")

    # 法术特效（3 种 × 6-12 帧）
    fx_spells = [
        ('fx_ice_storm', COLORS['frost_cyan'], 12),
        ('fx_thunder_strike', COLORS['warning_yellow'], 6),
        ('fx_heal_light', COLORS['success_green'], 8),
    ]

    for fx_name, color, frames in fx_spells:
        for frame in range(frames):
            output_path = dirs['fx'] / fx_name / f'{fx_name}_{frame+1:03d}.png'
            img = Image.new('RGBA', (96, 96), (0, 0, 0, 0))
            draw = ImageDraw.Draw(img)
            size = int((frame + 1) * 6)
            draw.ellipse([48-size//2, 48-size//2, 48+size//2, 48+size//2],
                        fill=color + (180 - frame * 20,))
            img.save(output_path)
            print(f"✅ Created: {output_path}")

    # ========== 全屏背景 ==========
    print("\n### 6. 全屏背景")

    # 城镇场景
    create_gradient_image((1280, 720), (20, 30, 40), (13, 13, 19),
                         dirs['backgrounds'] / 'bg_town_01.png')

    # 地牢场景
    create_gradient_image((1280, 720), (10, 10, 15), (5, 5, 8),
                         dirs['backgrounds'] / 'bg_dungeon_01.png')

    # ========== 统计信息 ==========
    print("\n" + "="*60)
    print("🎉 占位符素材生成完成！")
    print("="*60)

    # 统计文件数量
    total_files = 0
    for dir_path in dirs.values():
        files = list(dir_path.rglob('*.png'))
        total_files += len(files)
        print(f"📁 {dir_path.relative_to(output_base)}: {len(files)} 个文件")

    print(f"\n📊 总计: {total_files} 个素材文件")
    print(f"📂 输出目录: {output_base.absolute()}")
    print("\n✅ 下一步：")
    print("   1. 运行校验：Arcadia.AssetTool validate --dir .tmp/arcadia-assets/")
    print("   2. 在客户端中加载并验收整体效果")
    print("   3. 根据需要替换为正式素材")

if __name__ == '__main__':
    main()
