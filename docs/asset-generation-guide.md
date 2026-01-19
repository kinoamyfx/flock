# 🎨 Arcadia 素材快速生成指南

## 📋 **目录**
1. [RunningHub 网页端生成](#1-runninghub-网页端生成)
2. [Aseprite 手工绘制](#2-aseprite-手工绘制)
3. [商业素材采购](#3-商业素材采购)
4. [质量验收标准](#质量验收标准)

---

## 1. RunningHub 网页端生成 ⭐ 推荐

### 🎯 **优势**
- ✅ 专门优化像素风生成
- ✅ 中文界面，易于使用
- ✅ 无需安装软件
- ✅ 可批量生成

### 📝 **操作步骤**

#### **Step 1: 访问并登录**
```
URL: https://www.runninghub.cn/ai-detail/1957729299266727938

登录方式：微信扫码（免费注册）
```

#### **Step 2: 输入提示词**

**示例：主角 Idle 动画**
```
pixel art game character, young adventurer warrior,
blue light armor with gold trim, idle standing pose,
side view, 48x48 pixels, clean sharp pixels,
deep dark fantasy style, transparent background,
high quality, masterpiece, RPG character sprite
```

**技巧**：
- 保持 `pixel art` 关键词在开头
- 明确尺寸（如 `48x48 pixels`）
- 添加 `transparent background` 确保透明
- 添加 `side view` 指定视角

#### **Step 3: 生成并下载**

1. 点击"生成"按钮
2. 等待 10-30 秒
3. 查看生成结果
4. 满意则下载（PNG 格式）
5. 不满意则调整提示词重新生成

#### **Step 4: 批量生成动画帧**

**主角 Walk 动画（8 帧）**

逐帧修改提示词：
```
帧 1: pixel art ... walk cycle, left foot forward, ...
帧 2: pixel art ... walk cycle, transition pose, ...
帧 3: pixel art ... walk cycle, left foot mid-step, ...
...
帧 8: pixel art ... walk cycle, right foot forward, ...
```

**快捷方法**：
- 先生成 1 帧，满意后批量生成
- 保持主体描述不变，只修改动作描述

#### **Step 5: 后处理**

生成完成后运行：
```bash
# 1. 将下载的图片放入输入目录
mkdir -p .tmp/arcadia-assets-generated
# 复制图片到该目录

# 2. 调整尺寸到规范
python3 scripts/process_ai_assets.py

# 3. 移动到资源目录
cp -r .tmp/arcadia-assets-processed/* assets/

# 4. 验证
dotnet test Arcadia.sln --filter AssetTool
```

---

## 2. Aseprite 手工绘制

### 💻 **安装 Aseprite**

```bash
# macOS
brew install --cask aseprite

# Windows
# 访问 https://www.aseprite.org 下载安装包

# Linux
sudo apt install aseprite
```

### 🎨 **绘制技巧**

#### **新建项目**
```
File → New
尺寸: 48×48（主角）或 32×32（NPC）
色彩模式: RGBA
```

#### **绘制主角（48×48）**

**工具**：
- ✏️ Pencil Tool（铅笔）：绘制像素
- 🎨 Eraser（橡皮）：擦除像素
- 🖌️ Bucket Fill（油漆桶）：填充区域
- 🔍 Zoom In：放大到 400-800%

**步骤**：
1. **轮廓**：用 1px 铅笔勾勒角色外形
2. **填色**：用油漆桶填充主色（蓝色 #48CAE4）
3. **阴影**：添加 2 级阴影色（深蓝 + 深蓝灰）
4. **高光**：添加高光色（浅蓝 + 白色）
5. **武器**：绘制金色武器（#FFB703）
6. **透明**：确保背景透明（Alpha 通道）

#### **动画制作**

```
Window → Animation
点击 "+" 添加帧
逐帧绘制
洋葱皮（Onion Skin）查看上一帧
```

**帧率设置**：
```
File → Sprite Properties
FPS: 12（角色）或 24（特效）
```

#### **导出**
```
File → Export Sprite Sheet
格式: PNG
布局: Horizontal（水平排列）
```

---

## 3. 商业素材采购

### 🛒 **推荐资源**

#### ** itch.io**
- **URL**: https://itch.io/game-assets/free/tag-pixel-art
- **价格**: $0-20
- **优点**: 像素风专用，可商用
- **推荐搜索**:
  - "pixel art RPG"
  - "dark fantasy pixel art"
  - "pixel art dungeon"

#### ** OpenGameArt**
- **URL**: https://opengameart.org
- **价格**: 免费
- **优点**: 完全免费，可商用
- **许可**: CC0 或 CC-BY

#### ** Kenney Assets**
- **URL**: https://kenney.nl/assets
- **价格**: 免费
- **优点**: 高质量，分类清晰
- **推荐包**:
  - "Dungeon Asset Pack"
  - "RPG Pack"

### ⚠️ **采购注意事项**

1. **许可协议**：
   - CC0：完全免费，无限制
   - CC-BY：免费，需署名
   - Commercial：需购买商用许可

2. **色板调整**：
   - 购买后可能需要调整到 Deep Dark Fantasy 色板
   - 使用 Aseprite 批量调整色调

3. **尺寸调整**：
   - 统一到项目规范（48×48 / 32×32 等）

---

## 质量验收标准

### ✅ **必须满足**

1. **尺寸正确**
   - 主角：48×48
   - NPC：32×32
   - 怪物：24×24 ~ 64×64
   - Tile：16×16
   - 道具：32×32

2. **透明背景**
   - RGBA 格式
   - Alpha 通道启用
   - 无白色/黑色背景

3. **像素清晰**
   - 无抗锯齿（无模糊）
   - 网格对齐（16×16 基础单位）
   - 边缘锐利

4. **色板一致**
   - 使用 Deep Dark Fantasy 色板
   - 主色 + 2 级阴影 + 1 级高光
   - 同类素材色彩统一

5. **命名规范**
   - 小写字母 + 下划线
   - 序号 3 位数字（001, 002...）
   - 分类前缀清晰

### ⭐ **加分项**

- 动画流畅（12/24 FPS）
- 细节丰富（纹理/阴影/高光）
- 风格统一（Dead Cells/Celeste 参考）
- 光效强烈（Additive 混合）

---

## 🚀 **快速开始**

### **方案 A：AI 生成（1-2 天）**
```bash
# 1. 阅读提示词
cat .tmp/arcadia-assets/prompts/character_prompts.md

# 2. 访问 RunningHub
open https://www.runninghub.cn/ai-detail/1957729299266727938

# 3. 生成主角（5 个动作，31 帧）
# 逐个输入提示词并下载

# 4. 后处理
python3 scripts/process_ai_assets.py

# 5. 集成测试
dotnet run --project src/Arcadia.Client/Arcadia.Client.csproj
```

### **方案 B：手工绘制（1-2 周）**
```bash
# 1. 安装 Aseprite
brew install --cask aseprite

# 2. 新建项目
# File → New → 48×48

# 3. 绘制主角
# 参考 .tmp/arcadia-assets/prompts/character_prompts.md

# 4. 导出 Sprite Sheet
# File → Export Sprite Sheet

# 5. 集成测试
```

### **方案 C：采购素材（1 天）**
```bash
# 1. 访问 itch.io
open https://itch.io/game-assets/free/tag-pixel-art

# 2. 搜索并购买
# "dark fantasy dungeon"

# 3. 下载并解压
# 4. 调整色板和尺寸
# 5. 重命名到规范
```

---

## 📊 **时间估算**

| 方案 | 时间 | 成本 | 质量 |
|------|------|------|------|
| **AI 生成** | 1-2 天 | 免费~$50 | ⭐⭐⭐☆☆ |
| **手工绘制** | 1-2 周 | $0 | ⭐⭐⭐⭐⭐ |
| **采购素材** | 1 天 | $10-50 | ⭐⭐⭐⭐☆ |

---

## 💬 **需要帮助？**

如果遇到问题：
1. 查看提示词文档：`cat .tmp/arcadia-assets/prompts/character_prompts.md`
2. 运行生成指南：`bash scripts/generate_assets_guide.sh`
3. 查看后处理脚本：`python3 scripts/process_ai_assets.py`

---

**文档版本**: v1.0.0
**创建日期**: 2026-01-17
**适用项目**: Arcadia（Silk.NET 客户端）
