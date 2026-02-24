# Design｜UX（IA / 流程 / 状态 / 文案）

## Outcome
保证关键路径最短、错误可恢复、状态齐全，并形成工程可落地交接包。

## IA（信息架构，v1.0.0 最小）
- Main Menu
  - Start Game
  - Settings
  - Exit
- In-Game
  - Inventory（携带/安全箱/整理）
  - HUD（HP/Spirit/撤离/拾取提示）

## Key Flows（关键流程表）
| Step | User Intent | UI State | System Action | Errors | Recovery |
|---|---|---|---|---|---|
| 1 | 进入秘境 | Loading → InDungeon | Connect + Hello/Welcome | auth_failed | 返回主菜单并提示 |
| 2 | 移动 | HUD 正常 | Send MoveIntent | 断线 | 显示断线提示/重连 |
| 3 | 拾取 | LootPrompt | Send PickupIntent | 保护期不可拾取 | 显示剩余秒数 |
| 4 | 撤离 | CastBar | Send EvacIntent | 被打断 | 显示“撤离中断” |

## State Matrix（状态矩阵）
| Surface | Default | Loading | Empty | Error | Disabled | Success |
|---|---|---|---|---|---|---|
| 主菜单 | 按钮可点 | - | - | - | - | - |
| 设置 | 表单可编辑 | - | - | 保存失败 | 无效选项 | 保存成功提示 |
| 背包 | 显示格子 | 加载中 | 无物品 | 同步失败 | 保护期/不可操作 | 整理完成 |
| HUD | 常态显示 | - | - | 断线/重连 | - | 撤离完成提示 |
| LootPrompt | 看到掉落 | - | 无掉落 | 同步失败 | 保护期禁用 | 拾取成功反馈 |

## Microcopy（关键文案表）
| Surface | Trigger | Copy | Action | Notes |
|---|---|---|---|---|
| Toast | 鉴权失败 | “连接失败：鉴权无效” | 返回主菜单 | 不泄露内部原因 |
| LootPrompt | 保护期 | “保护期：{sec}s（仅击杀者队伍）” | - | 显示倒计时 |
| HUD | 断线 | “连接中断，尝试重连…” | - | 允许取消 |
| Evac | 被打断 | “撤离被打断” | - | 原因可选：移动/受击 |

## Acceptance
- 关键路径步骤最少且无歧义（入口→完成不绕）。
- 错误可恢复：鉴权失败/断线/保护期都有明确反馈与下一步。
- 状态齐全：loading/empty/error/disabled/success 不缺位。

