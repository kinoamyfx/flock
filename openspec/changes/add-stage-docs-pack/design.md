## Context
本变更不解决“系统能力缺失”，而是解决“资料资产缺失”：
- 让每次迭代都有可复用的对齐基准；
- 让验收从“口头描述”变成“可跑的清单与证据”；
- 把决策与假设写清楚，避免未来反复翻旧账。

## Deliverable Contract（每份文档都必须包含）
- Outcome：该阶段要解决什么问题
- Assumptions：默认假设（哪些还没拍板）
- Deliverables：交付物清单（可链接到现有 specs/脚本/截图）
- Acceptance：验收口径（可测/可回归）
- Risks：风险与依赖
- Rollback：回滚与替代方案（文档层面的：撤销假设/切换方案）

## Structure
- `openspec/changes/add-stage-docs-pack/docs/`：资料包正文
- `openspec/project.md`：工程约定与工具链（真相源）
- `openspec/specs/**`：能力真相源（已实现能力的 requirements + scenarios）

## Update Policy（如何维护不腐烂）
- 每次新增/修改关键规则，优先更新相应 specs（能力真相）；
- 每次“方向性变化”（商业/定位/架构），新增 change-id 并把资料包里的假设升级为决策；
- Release/Ops/QA 文档必须在每次里程碑前做一次走查（确保脚本/命令仍可跑）。

