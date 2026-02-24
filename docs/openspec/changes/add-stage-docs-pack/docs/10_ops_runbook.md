# Ops｜SLO / 告警 / Runbook（MVP）

## Outcome
保证“可值守、可定位、可回退”。

## SLO/SLI（建议口径）
- 鉴权失败率（按 code 分类：auth_failed/auth_config_error/unauthenticated）
- tick overrun 比例（> tick interval）
- P50/P95 输入→快照延迟（先可观测）
- Kill→Drop→Loot 审计复盘成功率

## Alerts（最小）
- Gateway/Zone 进程崩溃或无法监听端口
- auth_config_error（配置缺失）出现
- tick overrun 持续超阈值

## Runbook（常见故障）
### 1) 端口占用导致服务起不来
- 现象：日志出现 `Address already in use`
- 处理：为冒烟设置非默认端口（示例：`ARCADIA_ENET_PORT=17777 ARCADIA_GATEWAY_PORT=18080 bash scripts/smoke_enet.sh`）

### 2) 全部鉴权失败（auth_config_error）
- 现象：客户端收到 `auth_config_error|missing_auth_keys`
- 处理：确认 `ARCADIA_AUTH_KEYS`/`ARCADIA_AUTH_ACTIVE_KID` 已设置（不要把 secret 写进仓库日志）。

### 3) 负向鉴权冒烟不稳定
- 处理：检查负向冒烟日志 `.tmp/smoke_loadtest_negative.log` 是否包含 `Code=auth_failed`，并对齐断开返回的 ZoneError。

