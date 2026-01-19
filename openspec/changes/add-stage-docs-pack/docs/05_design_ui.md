# Design｜UI（tokens / 组件 / 证据）

## Outcome
保持“酷炫且一致”的 UI 风格，并能用截图与脚本重复验收。

## Style Pack（真相源）
- tokens 文档：`godot/arcadia_godot_client/docs/ui_tokens.md`
- UI spec：`openspec/specs/ui-style/spec.md`

## 关键界面（v1.0.0 Must）
截图证据（本地回归用，存在于 `.tmp/ui/`）：
- 主菜单：`.tmp/ui/main_menu.png`
- 设置：`.tmp/ui/settings.png`
- 背包：`.tmp/ui/inventory.png`
- HUD：`.tmp/ui/hud.png`
- 掉落拾取提示：`.tmp/ui/loot_prompt.png`

## 组件清单（最小）
- Button（normal/hover/pressed/disabled）
- Panel（9-slice 风格 + 边框/阴影）
- Slot（背包格子，hover/focus/disabled）
- ProgressBar（HP/Spirit/撤离读条）
- Tooltip / Prompt（拾取提示，保护期倒计时）

## Verification（脚本入口）
- 截图采集：`bash scripts/capture_ui_screenshots.sh`
- 一致性检查报告：`bash scripts/check_ui_style_consistency.sh`（产物：`.tmp/ui_style_consistency_report.md`）
- UI 回归门禁：`bash scripts/ui_regression_gate.sh`（截图 + 一致性检查）

## Mod Policy（表现层）
- 可覆盖 UI 资源（按优先级），必须可审计“谁覆盖了什么”（见 `openspec/specs/client-render/spec.md`）。
- 禁止 Mod 联网（MVP）。
