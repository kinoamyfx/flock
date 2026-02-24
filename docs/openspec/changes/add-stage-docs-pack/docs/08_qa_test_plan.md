# QA｜测试矩阵与回归清单

## Outcome
让“质量”变成可执行清单：功能/回归/性能/安全边界都有可跑口径。

## Test Matrix（最小）
| 类型 | 覆盖范围 | 入口 | 证据 |
|---|---|---|---|
| Build | 全仓库 | `dotnet build Arcadia.sln` | build log |
| Unit | Core/Server/Rules | `dotnet test Arcadia.sln` | 通过数 |
| Smoke（鉴权） | Gateway+Zone+LoadTest | `bash scripts/smoke_enet.sh` | `.tmp/smoke_*.log` |
| Smoke（可玩闭环） | connect→move→death/drop→pickup | `bash scripts/smoke_playable_slice.sh` | `.tmp/smoke_playable_slice.log` |
| UI Regression | screenshots + tokens/theme checks | `bash scripts/ui_regression_gate.sh` | `.tmp/ui/*.png` + `.tmp/ui_style_consistency_report.md` |
| Load（64 baseline） | 64 clients baseline | `bash scripts/loadtest_64.sh` | `.tmp/loadtest64_*.log` |

## Regression Checklist（高风险点）
- token/kid 轮换与过期
- 断线留身 60s + 重连语义（入口/原地）
- 拾取保护期与并发拾取原子性
- 掉落/背包去重（防复制）

## Known Gaps（与现有 tasks 对齐）
本项已对齐并补齐到代码与脚本（以仓库内脚本/测试为准）：
- 输入限流 & 重放保护：服务端限流器 + 每 tick 意图门禁
- correlation id + metrics：链路 Cid 与 ZoneMetrics 周期日志
- 确定性测试 + 64 baseline：单测覆盖 + `scripts/loadtest_64.sh`
