# Change: v1.0.0 UI 画风（Art Direction）与关键界面交付

## Why
v1.0.0 的 UI 画风属于“核心产品体验”，直接影响第一印象、留存与付费。若 UI 风格不统一或缺少动效/氛围（灯光/迷雾/战斗 HUD），即使核心玩法系统正确，也难以被用户感知为“完成度高的作品”。

## What Changes
- 新增 UI 风格规范（Style Guide）：颜色系统、字体与字号层级、图标与材质风格、布局栅格、动效节奏、可访问性（对比度/字号）。
- 明确 Godot 渲染层的 UI 组件与主题（Theme/StyleBox/Shader）落地方式，并约束“表现不影响权威状态”。
- 交付 v1.0.0 关键界面（按 Charter）：主菜单、设置、背包（携带/安全箱/整理）、秘境 HUD（血量/灵力/技能/撤离读条）、掉落拾取提示。
- 约束 Mod 能力：表现层 mod 可替换 UI 资源（按优先级覆盖），但 MVP 禁止 mod 联网。

## Impact
- Affected specs:
  - `specs/ui-style/spec.md`（ADDED）
  - `specs/client-render/spec.md`（MODIFIED：补充 UI 画风与 mod 禁联网的更细验收）
- Affected code:
  - （后续实现期）Godot UI 主题与组件库；资源覆盖策略与审计；关键界面场景

