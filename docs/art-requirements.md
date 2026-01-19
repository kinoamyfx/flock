# Arcadia 美术需求文档 v1.0.0

> **目标**: 为 Silk.NET 客户端提供"精美绝伦"的像素风游戏素材
> **风格**: 高像素密度 Pixel Art + 强光效/雾/粒子
> **交付标准**: 所有素材需通过 `Arcadia.AssetTool validate` 校验

---

## 1. 技术规范

### 1.1 Pixel Grid（像素网格）
- **基础单位**: 16×16 像素（1 tile = 1 游戏单位）
- **对齐规则**: 所有像素艺术必须严格对齐网格，无亚像素偏移
- **缩放比例**: 游戏内渲染时可能进行 2x/3x 放大

### 1.2 Scale Tokens（尺寸规格）

#### 角色与 NPC
- **主角**: 48×48 像素（3×3 tiles）
- **NPC**: 32×32 像素（2×2 tiles）
- **小怪物**: 24×24 像素（1.5×1.5 tiles）
- **大怪物**: 64×64 像素（4×4 tiles）

#### 场景 Tileset
- **地面/墙壁**: 16×16 像素（标准 tile）
- **大型装饰**: 32×32 或 48×48 像素
- **全屏背景**: 1280×720 像素（渲染分辨率）

#### UI 图标
- **道具图标**: 32×32 像素
- **技能图标**: 48×48 像素
- **小图标**: 16×16 像素（状态效果等）

#### 特效帧
- **攻击特效**: 64×64 像素（单帧）
- **法术特效**: 96×96 像素（单帧）
- **粒子纹理**: 16×16 或 32×32 像素

### 1.3 Color Palette（色板）

#### 主色调（Deep Dark Fantasy）
```yaml
背景基调:
  - bg_darkest: "#0D0D13"  (13,13,19)  # 深色背景
  - bg_dark: "#141419"     (20,20,25)  # 次级背景
  - bg_panel: "#1E1E26"    (30,30,38)  # 面板背景

语义色彩:
  - health_red: "#E63946"  (230,57,70)   # HP/危险
  - spirit_blue: "#48CAE4" (72,202,228)  # 精力/魔法
  - loot_gold: "#FFB703"   (255,183,3)   # 宝物/传说
  - warning_yellow: "#FB8500" (251,133,0) # 警告
  - danger_red: "#D00000"  (208,0,0)     # 致命危险
  - success_green: "#06D6A0" (6,214,160) # 成功/回复

光照色彩:
  - flame_orange: "#FF6B35" (255,107,53)  # 火焰
  - frost_cyan: "#90E0EF"   (144,224,239) # 冰霜
  - toxic_green: "#70E000" (112,224,0)   # 毒素
  - shadow_purple: "#7209B7" (114,9,183) # 暗影
```

#### 色板使用规则
- **背景**: 使用 `bg_darkest`/`bg_dark` 渐变（ Dungeon 环境）
- **角色**: 主色 + 2 级阴影色 + 1 级高光色
- **特效**: 语义色彩 + 混合模式叠加
- **UI**: 面板用 `bg_panel` + `loot_gold` 边框强调

### 1.4 帧动画规格

#### 角色动画
- **帧率**: 12 FPS（每帧 83ms）
- **方向**: 4 方向（前/后/左/右）或 8 方向
- **循环**: 是（Idle/Walk）或 一次性（Attack/Hurt）

#### 最小动画集
- **Idle**: 4-6 帧（呼吸动画）
- **Walk**: 8-12 帧（步行动画）
- **Attack**: 6-8 帧（攻击动作，非循环）
- **Hurt**: 2-4 帧（受击闪烁）
- **Death**: 8-12 帧（死亡倒地，非循环）

#### 特效动画
- **帧率**: 24 FPS（快节奏特效）
- **混合模式**: Additive（光效）或 Normal（物理）
- **生命周期**: 0.3s - 1.0s

---

## 2. 素材需求清单

### 2.1 角色与 NPC（4 套 × 4 方向 × 5 动作）

#### 主角（Adventurer）
- **尺寸**: 48×48 像素
- **视角**: 侧视图（RPG 风格）
- **配色**: 蓝色轻甲 + 金色装饰（辨识度高）
- **动画集**:
  - Idle（4 帧）
  - Walk（8 帧）
  - Attack（6 帧，挥剑）
  - Hurt（3 帧）
  - Death（10 帧）
- **精灵表**: 48×240 像素（5 动作 × 1 行）或 48×48×32（完整表）

#### NPC Merchant
- **尺寸**: 32×32 像素
- **配色**: 绿色长袍 + 棕色背包
- **动画集**: Idle（4 帧）+ Walk（8 帧）
- **表情**: 友好微笑

#### NPC Guard
- **尺寸**: 32×32 像素
- **配色**: 银色盔甲 + 红色披风
- **动画集**: Idle（4 帧）+ Walk（8 帧）
- **表情**: 严肃警惕

#### NPC Mystic
- **尺寸**: 32×32 像素
- **配色**: 紫色长袍 + 发光法杖
- **动画集**: Idle（4 帧，法杖浮动）+ Walk（8 帧）
- **特效**: 法杖粒子（16×16 粒子纹理）

### 2.2 怪物（6 种 × 2 状态）

#### Slime（史莱姆）
- **尺寸**: 24×24 像素
- **配色**: 绿色半透明（毒系）
- **动画**: Idle（4 帧，呼吸弹跳）+ Hurt（2 帧）
- **特效**: 死亡时分裂成小粒子

#### Skeleton（骷髅兵）
- **尺寸**: 32×32 像素
- **配色**: 骨白色 + 红色眼眶
- **动画**: Idle（4 帧，骨骼抖动）+ Attack（6 帧，挥砍）
- **武器**: 生锈铁剑

#### Ghost（幽灵）
- **尺寸**: 40×40 像素
- **配色**: 半透明青色 + 白色内光
- **动画**: Idle（4 帧，浮动波动）+ Attack（6 帧，冲撞）
- **特效**: 拖尾粒子（16×16，Additive 混合）

#### Orc（兽人）
- **尺寸**: 48×48 像素
- **配色**: 绿色皮肤 + 破旧皮甲
- **动画**: Idle（4 帧）+ Walk（8 帧）+ Attack（8 帧，重击）
- **武器**: 巨型战斧

#### Dragon（幼龙）
- **尺寸**: 64×64 像素
- **配色**: 红色鳞片 + 金色翅膀
- **动画**: Idle（4 帧，翅膀扇动）+ Attack（8 帧，喷火）
- **特效**: 火焰喷吐（64×64，帧动画）

#### Boss（黑暗领主）
- **尺寸**: 64×64 像素
- **配色**: 黑色盔甲 + 紫色光环
- **动画**: Idle（6 帧，能量聚集）+ Attack（10 帧，三连击）
- **特效**: 暗影能量场（96×96，帧动画）

### 2.3 场景 Tileset（12 种 × 4 变体）

#### Dungeon Floor（地牢地面）
- **尺寸**: 16×16 像素
- **变体**:
  1. 石板地面（灰色，有裂纹）
  2. 木板地面（棕色，旧质感）
  3. 地毯地面（红色，花纹）
  4. 泥土地面（褐色，粗糙）

#### Dungeon Wall（地牢墙壁）
- **尺寸**: 16×16 像素
- **变体**:
  1. 石砖墙（灰色，有苔藓）
  2. 木墙（棕色，钉子加固）
  3. 铁栅栏（黑色，可透视）
  4. 暗门（与墙壁同色，可打开）

#### Decoration（装饰物）
- **尺寸**: 16×16 或 32×32
- **清单**:
  - 火炬（16×16，火焰动画）
  - 宝箱（32×32，待机/打开两种状态）
  - 骨骸（16×16，散落骨头）
  - 蜘蛛网（16×16，半透明）
  - 裂缝（16×16，地面发光）

#### Light Source（光源）
- **尺寸**: 16×16 或 32×32
- **清单**:
  - 点亮火把（帧动画，Additive 混合）
  - 魔法光球（帧动画，彩色光芒）
  - 蜡烛（静态，小火焰）

### 2.4 道具与法宝（10 种）

#### 武器类（UI 图标：32×32）
1. **Iron Sword（铁剑）**: 银色剑身 + 木质剑柄
2. **Fire Staff（火焰法杖）**: 红色宝石 + 木色杖身
3. **Ice Bow（冰霜弓）**: 蓝色弓臂 + 冰晶箭

#### 防具类（UI 图标：32×32）
4. **Leather Armor（皮甲）**: 棕色皮革 + 铆钉
5. **Iron Shield（铁盾）**: 银色盾牌 + 红色纹章
6. **Magic Robe（法袍）**: 紫色长袍 + 金色刺绣

#### 消耗品（UI 图标：32×32）
7. **Health Potion（生命药水）**: 红色瓶身 + 发光液体
8. **Mana Crystal（法力水晶）**: 蓝色水晶 + 能量粒子

#### 特殊物品（UI 图标：32×32）
9. **Evacuation Scroll（撤离卷轴）**: 羊皮纸卷 + 金色封蜡
10. **Loot Bag（百宝袋）**: 棕色袋子 + 金色绳索（介子袋）

### 2.5 特效（攻击 3 种 + 法术 3 种）

#### 攻击特效（64×64，帧动画）
1. **Sword Slash（剑气斩）**: 蓝色新月形光弧 + 粒子尾迹（6 帧）
2. **Arrow Shot（箭矢射击）**: 白色箭矢 + 速度线（4 帧）
3. **Fire Blast（火焰爆发）**: 橙红色球体 + 烟雾扩散（8 帧）

#### 法术特效（96×96，帧动画）
4. **Ice Storm（冰霜风暴）**: 青色冰晶 + 暴风雪效果（12 帧）
5. **Thunder Strike（雷霆一击）**: 紫色闪电 + 闪光（6 帧）
6. **Heal Light（治疗之光）**: 绿色光柱 + 粒子上升（8 帧）

---

## 3. 命名与导入规则

### 3.1 文件命名规范

#### 格式
```
<category>_<name>_<variant>.png
```

#### 示例
- 角色精灵表: `char_adventurer_spritesheet.png`
- 怪物精灵表: `enemy_slime_spritesheet.png`
- 地面 Tile: `tile_floor_stone_01.png`
- 墙壁 Tile: `tile_wall_brick_01.png`
- 道具图标: `item_sword_iron.png`
- 特效帧序列: `fx_sword_slash_001.png` ~ `fx_sword_slash_006.png`

#### 规则
- 使用小写字母 + 下划线
- 序号用 2 位数字（01-99）
- 精灵表后缀 `_spritesheet.png`
- 帧动画用 `_001` ~ `_999`

### 3.2 目录结构

```
assets/
├── sprites/
│   ├── char/           # 角色精灵表
│   │   ├── adventurer/
│   │   │   └── char_adventurer_spritesheet.png
│   │   ├── npc_merchant/
│   │   └── npc_guard/
│   ├── enemy/          # 怪物精灵表
│   │   ├── slime/
│   │   ├── skeleton/
│   │   └── boss/
│   └── fx/             # 特效帧序列
│       ├── sword_slash/
│       └── ice_storm/
├── tiles/              # 场景贴图
│   ├── floor/
│   ├── wall/
│   └── decoration/
├── items/              # 道具图标
│   ├── weapons/
│   ├── armor/
│   └── consumables/
└── backgrounds/        # 全屏背景
    ├── town_scene_01.png
    └── dungeon_scene_01.png
```

### 3.3 ResourceKey 映射规则

#### 命名空间
```csharp
sprite.char.adventurer.spritesheet
sprite.enemy.slime.spritesheet
tile.floor.stone.01
tile.wall.brick.01
item.weapon.sword_iron
fx.sword_slash
bg.town_scene.01
```

#### 规则
- 路径分隔符 `/` → `.`
- 文件扩展名 `.png` 省略
- 序号前导零保留（01, 02, 03）

### 3.4 校验工具使用

#### 校验命令
```bash
# 校验所有素材
Arcadia.AssetTool validate --dir assets/

# 校验单个类别
Arcadia.AssetTool validate --dir assets/sprites/char

# 查看错误详情
Arcadia.AssetTool validate --verbose
```

#### 校验项
- ✅ 文件存在性
- ✅ 命名规范（小写 + 下划线）
- ✅ 尺寸符合 Scale Tokens
- ✅ ResourceKey 可解析
- ✅ PNG 格式 + 透明通道

---

## 4. 交付验收口径

### 4.1 文件格式
- **格式**: PNG（推荐 PNG-8 或 PNG-24，根据色彩复杂度）
- **透明通道**: 必需（Alpha Channel）
- **色彩空间**: sRGB
- **DPI**: 72（屏幕显示）

### 4.2 质量标准
- **像素清晰**: 无抗锯齿（无模糊边缘）
- **网格对齐**: 所有元素对齐 16×16 基础网格
- **色板一致**: 使用指定色板（Color Palette）
- **动画流畅**: 符合指定帧率（12/24 FPS）

### 4.3 交付清单

#### 最小素材包（MVP）
- [ ] 主角精灵表 × 1（含 5 动作）
- [ ] NPC 精灵表 × 3（Merchant/Guard/Mystic）
- [ ] 怪物精灵表 × 6（Slime/Skeleton/Ghost/Orc/Dragon/Boss）
- [ ] 地面 Tile × 4（石板/木板/地毯/泥土）
- [ ] 墙壁 Tile × 4（砖墙/木墙/栅栏/暗门）
- [ ] 装饰物 Tile × 5（火炬/宝箱/骸骨/蛛网/裂缝）
- [ ] 道具图标 × 10（武器/防具/消耗品/特殊）
- [ ] 攻击特效 × 3（剑气/箭矢/火焰）
- [ ] 法术特效 × 3（冰霜/雷霆/治疗）
- [ ] 全屏背景 × 2（城镇/地牢）

#### 文件数量统计
- **预期总量**: 约 40-50 个文件
- **总大小**: < 5 MB（PNG 压缩后）
- **精灵表**: ~10 张
- **单个 Tile**: ~20 个
- **图标**: ~10 个
- **特效序列**: ~8 个

### 4.4 验收流程

#### 自动校验
```bash
# 1. 命名校验
Arcadia.AssetTool validate --dir assets/

# 2. 资源加载测试
dotnet run --project src/Arcadia.Client/Arcadia.Client.csproj

# 3. 检查日志输出
[TextureManager] Texture created: ID=1, Size=48x240 (char_adventurer_spritesheet)
[TextureManager] Texture created: ID=2, Size=64x64 (enemy_slime_spritesheet)
```

#### 人工验收
- [ ] 视觉质量（像素清晰、色彩一致、风格统一）
- [ ] 动画流畅（无卡顿、帧率正确）
- [ ] 渲染效果（光照、混合模式、粒子）
- [ ] 集成测试（游戏中正常显示）

#### 回归测试
```bash
# 启动 Silk.NET 客户端
dotnet run --project src/Arcadia.Client/Arcadia.Client.csproj

# 检查项
- 精灵渲染正常（无拉伸/变形）
- 动帧播放流畅（12/24 FPS）
- 特效混合正确（Additive/Normal）
- 性能稳定（60 FPS）
```

---

## 5. 美术工具推荐

### 5.1 像素艺术软件
- **Aseprite**（推荐）: 专业像素艺术编辑器，支持帧动画、图层、调色板
- **LibreSprite**: Aseprite 开源替代版
- **Piskel**: 免费在线像素艺术编辑器
- **Photoshop**: 支持，但需关闭抗锯齿

### 5.2 特效工具
- **After Effects**: 粒子特效 + 导出 Sprite Sheet
- **Godot Particle System**: 实时预览粒子效果
- **Unity VFX Graph**: 可视化特效编辑

### 5.3 资源打包
- **TexturePacker**: 自动打包 Sprite Sheet
- **Aseprite**: 导出 Sprite Sheet 内置功能
- **ShoeBox**: 免费 Sprite Sheet 打包工具

---

## 6. AI 辅助生成（可选）

### 6.1 RunningHub 工具
- **URL**: https://www.runninghub.cn/ai-detail/1957729299266727938
- **模型**: pixcel像素_flux_V1-lora-000008.safetensors
- **用途**: 快速生成像素风格素材

#### 提示词模板
```
[角色描述]，像素风格，[服装描述]，侧视图/正面/背面，
行走动画，高清，8K，像素艺术
```

#### 示例
```
年轻女剑士，像素风格，轻甲战袍，侧视图，行走动画，
高清，8K，像素艺术
```

### 6.2 后期处理
AI 生成的素材需要：
1. 调整到指定尺寸（Scale Tokens）
2. 统一色板（Color Palette）
3. 透明背景（PNG Alpha Channel）
4. 网格对齐（Pixel Grid）
5. 命名规范化（符合 3.1 规则）

---

## 7. 联系与反馈

### 7.1 技术问题
- **命名/导入**: 查看 `openspec/specs/art-style/spec.md`
- **校验失败**: 运行 `Arcadia.AssetTool validate --verbose`
- **渲染问题**: 检查 `src/Arcadia.Client/Rendering/` 渲染器代码

### 7.2 美术风格确认
- **参考风格**: Dead Cells、Celeste、Blasphemous（高密度像素艺术）
- **配色参考**: 深色奇幻（Dark Fantasy）+ 霓虹光效
- **动画参考**: 12-24 FPS 流畅动画，非"一帧一停"

### 7.3 交付方式
- **首选**: Git LFS（大文件存储）
- **备选**: 百度网盘 / Google Drive / Mega（提供下载链接）
- **验收**: 自动校验 + 人工审查 + 游戏内测试

---

## 附录 A: 快速参考

### 最小素材包（精简版）
```
assets/
├── sprites/
│   ├── char_adventurer_spritesheet.png    (48×240, 5 actions)
│   ├── enemy_slime_spritesheet.png        (24×24, 2 states)
│   └── fx_sword_slash_001~006.png         (64×64, 6 frames)
├── tiles/
│   ├── tile_floor_stone_01.png            (16×16)
│   └── tile_wall_brick_01.png             (16×16)
├── items/
│   └── item_sword_iron.png                (32×32)
└── backgrounds/
    └── town_scene_01.png                  (1280×720)
```

### 验收命令（一键执行）
```bash
# 校验所有素材
Arcadia.AssetTool validate --dir assets/ --verbose

# 启动客户端测试
dotnet run --project src/Arcadia.Client/Arcadia.Client.csproj

# 性能压测（64 客户端）
bash scripts/loadtest_64.sh
```

---

**文档版本**: v1.0.0
**最后更新**: 2026-01-17
**适用客户端**: Silk.NET v0.1.0+
**下次更新**: 美术交付后根据反馈调整
