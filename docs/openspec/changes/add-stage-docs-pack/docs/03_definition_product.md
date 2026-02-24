# Definition｜需求（PRD + 验收用例总表）

## Outcome
把“要做什么”落到 **可实现、可测试、可回滚** 的需求与验收用例。

## Assumptions
- v1.0.0 以“可玩闭环 + 公平可信 + 画风统一”为 Must。
- 账号体系非目标：MVP 以 Gateway 签发 token + dev issue key 保护。

## User Stories（最小集合）
1) 作为玩家，我能进入秘境并完成一次回合（移动/交互→撤离/死亡→结算）。
2) 作为玩家，我能理解并接受高风险规则（全掉落/安全箱/保护期/断线留身）。
3) 作为玩家，我能在 UI 中完成关键操作（背包整理、安全箱、拾取提示、撤离读条）。

## E2E Flow（主流程）
1) 启动客户端 → 主菜单 → 开始游戏
2) 进入秘境 → HUD 可见（HP/Spirit/撤离读条占位/拾取提示）
3) 移动（输入→意图→服务端权威位置→快照）
4) 触发死亡（或撤离）→ 掉落容器生成（安全箱不掉落）
5) 拾取：10 秒保护期（击杀者队伍）生效
6) 结算：撤离成功带回；死亡掉落留在秘境

## Exceptions（异常流）
- 鉴权失败：无效 token → 断开并提示短错误（不泄露内部信息）。
- 断线：60 秒留身可被击杀并掉落；重连按 reset 决策入口/原地。
- 并发拾取：必须原子校验与去重（防复制）。

## Data Contract（关键数据）
- playerId：只能由 Gateway token 可信来源绑定，不允许客户端伪造。
- lootId：必须可唯一标识一次掉落容器，且服务端去重约束成立。
- resetVersion：用于重连“入口/原地”的权威决策。

## Acceptance（GWT 用例总表，最小）
### A. 连接与鉴权
- **WHEN** 客户端使用无效 token 连接
- **THEN** 服务端 MUST 断开并返回 `auth_failed`（短提示）

### B. 断线留身
- **WHEN** 玩家断线
- **THEN** 角色 MUST 留在原地 60 秒可被击杀

### C. 死亡掉落与安全箱
- **WHEN** 玩家死亡
- **THEN** 携带物 MUST 全掉落
- **AND THEN** 安全箱 MUST 不掉落

### D. 拾取保护
- **WHEN** 掉落生成后 10 秒内
- **THEN** 仅击杀者队伍可拾取

### E. 撤离
- **WHEN** 玩家开始撤离
- **THEN** 必须长读条，可被移动/受击打断（以服务端状态为准）

## Links（真相源）
- 可玩闭环 specs：`openspec/specs/playable-slice/spec.md`
- Zone 权威规则：`openspec/specs/dungeon-zone/spec.md`
- Client 非权威与 UI：`openspec/specs/client-render/spec.md`、`openspec/specs/ui-style/spec.md`

