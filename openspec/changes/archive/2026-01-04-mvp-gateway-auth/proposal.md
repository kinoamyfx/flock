# Change: MVP Gateway 鉴权（签发 Token + Key Rotation + Zone 仅验证）

## Why
当前实现中客户端可本地生成 AuthToken（依赖同一份 secret），虽然便于联调，但在“秘境夺宝 + 全掉落”的设定下，这会让伪造身份、会话劫持与重连作弊成为系统性风险。需要把“签发”集中到 Gateway，将 Zone Server 降级为仅“验证并绑定身份”。

## What Changes
- 新增 Gateway 服务：负责签发 Zone Handshake Token。
- 引入 Key Rotation：支持多把 key（含 `kid`），便于无停机轮换与撤销。
- 修改握手语义：客户端 MUST 使用 Gateway 签发的 token；Zone MUST 仅验证 token 并绑定 playerId。
- 明确 A/B/C 路线：
  - A: Gateway token（先做，堵住身份伪造）
  - B: 掉落/背包/审计落库闭环（再做，堵住经济被打穿）
  - C: AOI 与同步底线（最后做，为动作战斗铺路）

## Impact
- Affected specs:
  - `specs/gateway-auth/spec.md` (ADDED)
  - `specs/zone-auth/spec.md` (MODIFIED)
- Affected code:
  - 新增：`Arcadia.Gateway` 服务（HTTP）
  - 变更：token 格式（新增 `kid`）、Zone 验证逻辑支持多 key、客户端/压测工具改为向 Gateway 获取 token

