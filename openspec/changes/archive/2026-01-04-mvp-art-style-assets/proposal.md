# Change: v1.0.0 游戏画风与素材包（Art Bible + Asset Pack + Pipeline）

## Why
v1.0.0 的“完成度”很大一部分来自画风与素材质量：如果角色/地形/物品/特效/UI 不统一或精度不够，会直接拉低对标（了不起的修仙模拟器 / RimWorld / 星露谷）的主观观感，即使系统玩法正确也难以被认可。

## What Changes
- 定义 v1.0.0 的 Art Bible（画风规则）：颜色体系、材质语言、线条/描边、光影、像素密度/分辨率策略、VFX 风格、UI 与场景一致性。
- 定义 v1.0.0 的最小素材包（Asset Pack）：地形/建筑/角色/物品/特效/图标/字体等“可玩闭环”所需最小集合。
- 定义 Godot 导入与资源管线（Pipeline）：命名规范、目录结构、打包与热更新策略、Mod 资源覆盖规则（高优先级覆盖）。
- 明确验收口径：关键场景截图/录屏、性能预算、可扩展（后续内容可按同一规则增量引入）。

## Impact
- Affected specs:
  - `specs/art-style/spec.md`（ADDED）
  - `specs/client-render/spec.md`（MODIFIED：强化“酷炫灯光/迷雾/特效”与素材管线的验收）
- Affected code:
  - （后续实现期）Godot 资源导入、渲染/后处理、资产打包与 Mod 覆盖审计

