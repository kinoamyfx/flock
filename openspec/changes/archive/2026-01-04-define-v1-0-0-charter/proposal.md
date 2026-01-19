# Change: 定义 v1.0.0 版本画像（Charter）

## Why
目前仓库 `openspec/specs/` 为空，缺少“v1.0.0 上线版本到底要交付什么”的 Current Truth，会导致 Roadmap 与实现优先级容易漂移。需要先把 v1.0.0 的版本画像固化为可验收的规范与边界。

## What Changes
- 新增 `v1.0.0` 版本画像（Problem / Persona / KPIs&SLIs / Must-Should-Could / Non-goals / Acceptance）。
- 将已拍板的关键规则（断线60s、全掉落+安全箱、拾取保护10s、撤离点、重置策略、经济税费销毁、Mod 禁联网）纳入 v1.0.0 必要约束。

## Impact
- Affected specs:
  - `specs/release/v1.0.0.md`（ADDED，作为“版本画像”的真相源）
- Affected code:
  - 无（本变更为规范落盘，便于后续实现与验收对齐）

