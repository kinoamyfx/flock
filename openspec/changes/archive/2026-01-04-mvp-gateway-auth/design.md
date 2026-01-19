## Context
桃源牧歌的秘境是高对抗场景（PVP夺宝 + 全掉落 + 断线60s留身），任何“身份伪造/会话劫持/回滚复制”都会被放大成经济与信任灾难。

当前状态（已实现但仅用于联调）：
- 客户端本地用 shared secret 生成 token，Zone 直接验证并绑定。

## Goals / Non-Goals
- Goals:
  - Token 只能由 Gateway 签发，Zone 不负责签发。
  - 支持 key rotation（多 key + `kid`），便于撤销/轮换。
  - 保持协议/业务逻辑解耦：传输层可替换（LiteNetLib/ENet/QUIC），鉴权语义不变。
- Non-Goals:
  - 本变更不实现完整账号体系（OAuth/手机号等），仅提供 MVP 的签发入口（可用 dev key/本地模式保护）。
  - 本变更不实现完整风控与封禁体系，只预留扩展点。

## Decisions
- Decision: 使用 HMAC-SHA256 签名，token header 中携带 `kid`。
  - Why: MVP 最小依赖且性能可控；`kid` 支持轮换与撤销。
- Decision: Gateway 通过 HTTP 端点签发 token（Zone 不签发）。
  - Why: 易于接入 Godot/C# 客户端与压测工具；便于后续替换为 gRPC。
- Decision: Zone 验证使用“key set”（多 key）并按 `kid` 定位。
  - Why: 避免全量尝试导致性能与攻击面上升，同时支持平滑轮换。

## Risks / Trade-offs
- HMAC key 泄漏风险：
  - Mitigation: key 仅在 Gateway/Zone 侧配置；客户端不再持有；支持轮换与撤销。
- Gateway 成为入口单点：
  - Mitigation: MVP 单实例即可；后续可水平扩展并将 key 管理下沉到配置中心/密钥管理。

## Migration Plan
1. 增补 spec：Gateway 签发、Zone 验证语义与 `kid` 格式。
2. 实现 Gateway MVP：`POST /auth/token` 签发（受 dev key/本地模式保护）。
3. 改造客户端/压测：从 Gateway 获取 token 后再进入 Zone 握手。
4. Zone 支持多 key 校验，废弃“客户端自签”路径。

## Open Questions
- dev key 的保护策略：是否允许本地模式跳过，还是必须提供 `ARCADIA_DEV_ISSUE_KEY`？
- token 过期策略：默认 10 分钟是否合适？是否需要 refresh？

