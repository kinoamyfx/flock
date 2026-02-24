# Change: v1 可玩秘境切片（Vertical Slice）— 从“能连通”到“能游玩”

## Why
目前项目已具备：Gateway 鉴权闭环、Zone 骨架、掉落/安全箱规则与资产管线/渲染基线，但还缺少“玩家可上手游玩”的端到端闭环（进入秘境→移动/交互→战斗/死亡→掉落/拾取→撤离→结算）。

v1.0.0 的核心风险在于：如果没有尽早跑通可玩闭环，后续数值/内容/美术再精致也会堆在不可验证的地基上。

## What Changes
- 定义“最早可玩”验收口径（可玩闭环 + 反作弊硬规则 + 观测/审计）。
- 补齐客户端（Godot 渲染层）到 Zone 的最小交互：移动、拾取、撤离（输入→意图→服务端权威结果）。
- 补齐 Zone 的最小权威模拟：玩家实体位置、基础战斗占位（伤害/死亡触发）、掉落容器拾取权限（10s 击杀者队伍保护）。
- 补齐可玩回归资产：可执行的截图/录屏验收脚本 + 最小素材包占位（已存在）与 UI/HUD 占位。

## Impact
- Affected specs:
  - `specs/playable-slice/spec.md`（ADDED）
  - `specs/dungeon-zone/spec.md`（MODIFIED：补充可玩闭环要求与验收）
  - `specs/client-render/spec.md`（MODIFIED：补充可玩闭环 UI/HUD 与输入→意图边界）
- Affected code:
  - 客户端：Godot C# 工程与 `Arcadia.Client` 解耦集成（状态缓存/渲染绑定）
  - 服务端：Zone 输入协议、最小状态同步、拾取/撤离/死亡路径打通

