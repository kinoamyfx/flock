# Proposal: Silk.NET + WGPU + ECS 客户端重写

## 背景（Why）

**当前问题**：
1. 客户端使用 Godot + GDScript，与服务端 C# 技术栈割裂
2. 无法共享代码（ECS 架构/消息协议/网络层需重复实现）
3. GDScript 动态类型带来运行时错误风险
4. Godot 引擎限制深度定制能力

**用户决策**：在 v1.0.0 使用 Silk.NET + WGPU + ECS 重写客户端

## 目标（What）

**技术栈**：
- **窗口/输入**：Silk.NET.Windowing + Silk.NET.Input
- **渲染**：WGPU（WebGPU .NET 绑定）
- **架构**：ECS（复用 `Arcadia.Core.Ecs`）
- **网络**：ENet-CSharp + ZoneWireCodec（复用服务端协议）

**功能对齐**：
- 达到当前 Godot 客户端的视觉效果（2D 像素风 + 光照 + HUD）
- 主菜单（ARCADIA 标题 + 按钮）
- 游戏场景（玩家移动 + HUD + 光照）
- 网络连接（Hello/Welcome 握手 + MoveIntent/Snapshot 同步）

**验收口径**：
- 帧率稳定 60 FPS（1080p）
- 内存占用 < 200MB
- 启动时间 < 3 秒
- 代码复用率 > 50%（共享 Core 层）

## 影响范围（Impact）

**新增**：
- `src/Arcadia.Client.SilkNet/` - 新的 C# 客户端工程
- Renderer/Camera/SpriteBatch/UIRenderer/NetworkClient 等核心类

**废弃**：
- `godot/arcadia_godot_client/` - GDScript 工程（保留作为参考，归档）
- `godot/arcadia_godot_client_csharp/` - Godot C# 骨架（不再使用）

**共享**：
- `src/Arcadia.Core/` - ECS/消息协议/网络层（客户端直接引用）

## 风险与依赖

**技术风险**：
- WGPU .NET 绑定成熟度（可能需要降级到 Veldrid）
- macOS Metal 后端兼容性
- 2D 渲染性能（需要 Sprite Batching 优化）

**时间预算**：
- Stage 1 (Discovery): 30 分钟
- Stage 2 (Solution): 1 小时
- Stage 3 (Build): 8-12 小时
- Stage 4 (Launch): 1 小时
- Stage 5 (Acceptance): 30 分钟
- **总计**: 11-15 小时

**回滚方案**：
- 若 WGPU 不可用 → 改用 Veldrid
- 若自建渲染失败 → 回退到 Godot C#（方案 A）

## 阶段计划

参考 Stage Plan（已在对话中展开）：
1. Stage 1: 发现（Hello Window + Hello Triangle）
2. Stage 2: 方案（架构设计）
3. Stage 3: 实现（渲染/光照/UI/网络/ECS）
4. Stage 4: 上线（打包发布）
5. Stage 5: 验收（端到端测试）
