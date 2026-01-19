## Context
v1.0.0 已明确：2D 俯视 + 灯光/迷雾 + 酷炫视觉效果是核心体验交付，且需要对标“精致度”。

本变更聚焦：把“画风”从口号变成可执行的规范（Art Bible）与可落地的素材包/管线。

## Decision Gate（需要老板拍板，仅此一项）
请选择 v1.0.0 的基础画风方向（其余我默认推进）：
- Option A：高像素密度 Pixel Art（更接近 RimWorld 的生产效率，但整体更精致；对 mod 友好）
- Option B：手绘/半写实 2D（氛围更强、更“作品感”，但产能与一致性成本更高）
- Option C：干净矢量/扁平 + 材质光效（UI 与 VFX 可很酷，但角色/地形容易“塑料感”需把控）

决策（2026-01-02）：选择 Option A（高像素密度 Pixel Art）+ 强光效（发光/雾/粒子）作为“精致感”来源，兼顾产能与一致性。

## Art Bible（交付物定义）
必须至少包含：
- Pixel Density / Scale：世界单位到像素的比例、相机缩放规则、描边/阴影策略
- Color System：主色/辅色/强调色/危险/成功/中性；日夜/洞窟两套；雾与光源的色彩语言
- Material Language：木/石/土/金属/灵力科技的材质表现与边界
- Lighting：2D 光源类型、衰减、阴影/遮挡策略（可简化但必须一致）
- Fog：探索迷雾（永久）与战术迷雾（可见性）视觉规则（纹理/噪声/过渡）
- VFX：法宝/技能/受击/拾取/撤离读条的特效语言（粒子/拖尾/屏幕特效）
- UI/World Consistency：HUD 与场景共享色彩 token，避免割裂

## v1.0.0 Minimal Asset Pack（最小可玩闭环素材包）
（数量是“口径”，可迭代，但 v1.0.0 必须覆盖闭环）
- Biome Tileset：至少 1 套地表（草/土/石/水/边缘过渡）
- Dungeon Tileset：至少 1 套洞窟/秘境地表（墙/地/门/机关占位）
- Buildings/Props：至少 10 个（农田、工作台、箱子、篝火、路标等）
- Characters：
  - 玩家角色：至少 1 套（idle/run/attack/skill/cast/hit/death）
  - NPC/怪物：至少 3 套基础（用于秘境）
- Items/Icons：至少 40 个图标（资源/材料/装备/法宝占位）
- VFX：至少 12 个（受击、暴击、拾取、掉落闪光、撤离读条、技能弹道/爆炸等）
- UI Assets：与 `mvp-ui-art-direction` 对齐（按钮/面板/槽位/tooltip/字体）

## Pipeline (Godot)
- 统一命名与目录：避免导入后资源重复/漂移
- Atlas/打包策略：保证 draw call 与加载时间可控
- Mod 覆盖：允许更高优先级 mod 替换素材资源（记录审计：被哪个 mod 替换了什么）

## Naming & Folder Conventions（默认约定，后续可扩展）
> Why: Pixel 素材量大且迭代频繁；没有命名与目录约束会导致资源重复、覆盖失效、Mod 替换不可控。

- 目录建议（示意）：
  - `assets/tiles/biome/<biome>/...`
  - `assets/tiles/dungeon/<theme>/...`
  - `assets/chars/player/<set>/...`
  - `assets/chars/npc/<set>/...`
  - `assets/items/icons/...`
  - `assets/vfx/...`
  - `assets/ui/...`
- 命名建议（示意）：
  - tiles: `tile_<theme>_<name>_<variant>`
  - chars: `char_<kind>_<name>_<anim>_<dir>_<frame>`
  - vfx: `vfx_<category>_<name>_<variant>`
  - ui: `ui_<component>_<state>`

### Naming Rules（可执行约束）
- 只允许：小写字母、数字、下划线（`a-z0-9_`），禁止空格与中文文件名（避免跨平台与打包问题）。
- 目录/文件命名必须可推导 `ResourceKey`（用于 Mod 覆盖与审计）：
  - 例如：`assets/items/icons/icon_wood.png` → `ResourceKey(namespace="texture", path="items/icons/icon_wood")`
- 同一语义资源禁止多份“近似重复”：
  - 例如 `icon_wood.png` 与 `wood_icon.png` 同时存在，必须合并并在资源注册表中记录替换关系（避免 Mod 覆盖不可解释）。

### Folder Ownership（职责边界）
- `assets/`：官方基准资源（core），作为默认候选。
- `mods/<modId>/assets/`：Mod 资源包，允许覆盖 `assets/` 同 key 的资源（按 priority/loadOrder 规则）。
- `generated/`：构建产物（atlas、预处理纹理、缓存），禁止手工编辑；不作为 Mod 覆盖输入源。

## Import Settings（Pixel Art 基线）
> Attention: 这里的“导入设置”是必须可验证的工程约束；避免美术/程序不同机器导入导致贴图变糊或比例漂移。

- Filtering：默认关闭平滑过滤（避免像素糊），必要时只对特定非像素纹理开启。
- Mipmaps：默认关闭（像素风），如需远景缩放再按规则启用并验证不抖动。
- Compression：保持可控（优先无损或可接受的有损），并明确 UI/字体资源的压缩策略。

## Atlas / Packing Strategy（v1.0.0）
> Why: 像素素材数量多，若不打包会导致 draw call 与加载开销失控。

- icon atlas：物品图标/小图标合并为 atlas（便于 UI 批处理）。
- vfx atlas：可复用的小粒子/贴图 atlas 化（减少材质切换）。
- tileset：tiles 以 tileset/atlas 组织（按 biome/theme 维度分包，避免单 atlas 过大）。
- Acceptance：必须提供一份“atlas 清单”（每个 atlas 包含哪些源资源 key），便于审计与 Mod 覆盖排错。

## Mod Override Audit（可观测性口径）
> Why: Mod 覆盖是“能力”，也是“事故源”；必须可追溯“谁覆盖了什么”，否则无法定位画风漂移/材质错乱。

- 覆盖规则：更高 `priority` 覆盖；同优先级下更高 `loadOrder` 覆盖（与现有 `ResourceRegistry` 一致）。
- 审计要求：
  - 每次覆盖必须记录：`ResourceKey`、fromMod/fromPriority/fromLoadOrder、toMod/toPriority/toLoadOrder。
  - 至少提供“按 key 查询候选列表”的调试能力（用于复盘为什么选中了某个资源）。

## Performance Budget（v1.0.0 口径）
> Attention: 先把预算“可观测”做出来，再迭代优化；不要到后期才开始补性能。

- 目标：PC 上 UI + 场景渲染 60 FPS 可用（正常交互无明显卡顿）。
- 约束：新增后处理必须说明对 FPS 的影响与可开关策略（例如低配预设禁用）。

### Profiling Checklist（最小）
- UI：界面打开/关闭/切换背包时无明显卡顿（帧时间尖刺可观测）。
- 场景：开启 fog+lighting+基础 postfx 后，仍能保持稳定帧时间（至少能输出 profiling 报告）。
- 资源：首次进入秘境时加载峰值可观测（避免黑屏/长时间卡死）；缓存策略可解释。
