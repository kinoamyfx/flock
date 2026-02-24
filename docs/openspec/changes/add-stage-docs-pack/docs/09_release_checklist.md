# Release｜发布检查表（v1.0.0）

## Outcome
把“能发布”变成可执行 checklist（含回滚）。

## Pre-Release Checklist
- [ ] `dotnet build Arcadia.sln` 成功
- [ ] `dotnet test Arcadia.sln` 成功
- [ ] `bash scripts/smoke_enet.sh` 成功（建议设置非默认端口）
- [ ] `bash scripts/smoke_playable_slice.sh` 成功
- [ ] UI 截图包更新：`.tmp/ui/*.png`（用于回归）
- [ ] 关键规则回归：断线 60s、全掉落、安全箱 9 格、保护期 10s、撤离读条可打断

## Evidence Pack（归档口径）
- `.tmp/smoke_gateway.log` / `.tmp/smoke_server.log` / `.tmp/smoke_loadtest*.log`
- `.tmp/smoke_playable_slice.log`
- `.tmp/ui/*.png`

## Rollback
- 任何改动导致关键路径失败：回退到上一个通过 smoke 的版本（以 changelog/检查点定位）。

