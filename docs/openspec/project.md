# Project Context（Arcadia / 桃源牧歌）

> 本文件是 **工程约定真相源**：用于让协作者/智能体在“开始编码前”对齐目录职责、工具链、验证口径与边界约束。

## Purpose
Arcadia（中文名“桃源牧歌”）是一款 **2D 俯视像素风** 的“种田经营 + 秘境夺宝（合作为主，允许 PvP）”游戏：
- 客户端：渲染/输入/表现（**不权威**）。
- 服务端：秘境战斗/死亡/掉落/背包变更/撤离结算 **全权威**，并且必须可审计复盘。

## Tech Stack
- .NET：`dotnet`（当前工程 TargetFramework 多为 `net10.0`）
- Server：LiteNetLib（可靠 UDP）+ 自研协议（`Arcadia.Core.Net.Zone` / `ZoneWireCodec`）
- Client（现状）：Godot 渲染层（GDScript/Godot C# 骨架） + `Arcadia.Client`（Silk.NET 试验性客户端存在 proposal）
- Specs：OpenSpec（`openspec/`）

## Project Conventions

### Code Style
- **自解释代码**：命名清晰，避免弱类型容器。
- **注释三要素（中文）**：Why / Context / Attention（接口与复杂分支必须写）。
- **日志格式**：`类名|方法名|事件|Key=Value|Key=Value`（见 `Arcadia.Core.Logging.ArcadiaLog`）。
- **敏感信息**：token/密钥/密码等严禁写入仓库与日志；仅允许通过环境变量注入。

### Architecture Patterns
- **Server Authoritative**：战斗、掉落、背包变更、结算必须在服务端权威域完成。
- **Client Non-Authoritative**：客户端只做输入→意图、渲染展示；允许纯表现预测，但服务端结果必须可纠正。
- **Transport Replaceable**：传输层可替换（LiteNetLib/ENet/QUIC），但协议与业务语义必须保持解耦。
- **Auditability First**：必须能复盘一次 Kill → Drop → Loot 链路（位置、角色、物品、时间）。

### Assets & Naming
- `assets/` 是官方 core 基准资源。
- 命名约束：只允许 `a-z0-9_`，禁止空格与中文文件名（见 `assets/_docs/readme.md`）。
- `ResourceKey` 可从路径推导，用于 Mod 覆盖与审计：`assets/items/icons/icon_wood.png` → `texture:items/icons/icon_wood`。

### Testing Strategy（验证门禁）
最低门槛（每次改动后必须满足）：
- `dotnet build Arcadia.sln` 成功
- `dotnet test Arcadia.sln` 成功

关键路径（涉及网络/权威规则时必须跑）：
- ENet/握手/负向鉴权：`bash scripts/smoke_enet.sh`（建议通过设置 `ARCADIA_ENET_PORT/ARCADIA_GATEWAY_PORT` 避免端口占用）
- 可玩闭环冒烟：`bash scripts/smoke_playable_slice.sh`

输出卫生（防刷屏）：
- 长输出必须重定向到 `.tmp/last_cmd.log` 或脚本指定的 `.tmp/*.log`。

### Git Workflow
- 变更留痕优先写入：`openspec/CHECKPOINT.md`（过程快照）以及对应 `openspec/changes/<change-id>/`。
- 不默认执行 `git commit/push`（除非明确指令）。

## Domain Context（关键业务背景）
- 秘境规则（已拍板）：断线留身 60 秒可被击杀并掉落（安全箱不掉落）；死亡后 10 秒拾取保护（击杀者队伍）；撤离长读条可被打断；秘境无人/通关后 10 分钟重置；单线软上限 64，靠自动分线扩容。
- 经济：交易税/手续费系统销毁作为回收池；数值细节后续迭代。
- Mod：MVP 只做表现层覆盖，禁止 Mod 联网。

## External Dependencies
- LiteNetLib：网络传输（Server/Client MVP）
- ENet-CSharp：历史/备选传输（当前以 LiteNetLib 跑通语义闭环）
- Godot：渲染层（4.4+ 推荐；C# 工程见 `godot/arcadia_godot_client_csharp/`）
