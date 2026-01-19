# Change: 补齐“按阶段交付”的资料包（Discovery → Ops）

## Why
当前仓库的工程实现与部分 specs 已经可验收，但“编码前/迭代前应具备的资料”存在结构性缺口：
- 商业/市场/竞品没有形成可复用的一页纸与验证计划；
- 需求与 UX 流程缺少集中化的“主流程+异常流+状态矩阵+文案表”；
- 发布/运维/QA 的检查表与 Runbook 未落盘，容易在迭代中反复返工。

本变更目标是：把这些缺口补成“可验收、可维护、可迭代”的文档资产，并且不引入运行时代码变更。

## What Changes
- 新增一套“阶段资料包”（docs），覆盖：
  - Discovery：商业策略、市场/竞品
  - Definition：需求（PRD/验收用例/数据口径）
  - Design：UX（IA/流程/状态/文案）、UI（tokens/组件/证据引用）
  - Production：素材清单/许可证/管线验收
  - Level：首个秘境切片关卡设计口径（最小可玩）
  - QA：测试矩阵/回归清单/性能与压测计划
  - Release：发布检查表/回滚/证据归档口径
  - Ops：SLO/告警/Runbook/常见故障定位路径
- 明确这些文档与现有 OpenSpec specs 的关系：文档用于“阶段交付与协作”，specs 继续作为“能力真相源”。

## Impact
- Affected specs: 无（本变更不修改 `openspec/specs/**`）
- Affected code: 无
- New docs: `openspec/changes/add-stage-docs-pack/docs/**`

## Decision Gates（若要从“草案”升级为“定稿”需要拍板）
> 该变更会先按默认假设补齐文档，不阻塞工程推进；但以下内容最终需要明确：
- 发行与商业化：买断/订阅/F2P（含内购）选择与优先级
- 目标市场与渠道：国内/海外、Steam/自建/其它平台优先级

