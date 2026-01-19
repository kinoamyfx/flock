using Silk.NET.OpenGL;
using System.Numerics;

namespace Arcadia.Client.Rendering;

/// <summary>
/// 背景渲染器 - 负责渲染场景背景（MVP 版本使用全屏图）
/// Why: 在游戏世界中渲染城镇、地牢等场景背景
/// Context: MVP 使用 1280x720 全屏场景图，后续可扩展为基于瓦片的地图系统
/// Attention: 背景渲染在最底层（Z-order），精灵渲染在上层
/// </summary>
public sealed class BackgroundRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly SpriteRenderer _spriteRenderer;

    // Why: 当前激活的背景纹理 ID
    private uint _currentBackgroundTexture = 0;

    // Why: 相机偏移（用于场景滚动，预留给未来扩展）
    private Vector2 _cameraOffset = Vector2.Zero;

    // Why: 背景渲染尺寸（世界坐标，米）
    // Context: 1280x720 像素场景对应 128x72 米的游戏世界（假设 10 像素 = 1 米）
    private readonly Vector2 _backgroundSize = new(128.0f, 72.0f);

    public BackgroundRenderer(GL gl, SpriteRenderer spriteRenderer)
    {
        _gl = gl;
        _spriteRenderer = spriteRenderer;
        Console.WriteLine("[BackgroundRenderer] Initialized (using SpriteRenderer for full-screen backgrounds).");
    }

    /// <summary>
    /// 设置当前背景纹理
    /// </summary>
    /// <param name="textureId">纹理 ID（从 TextureManager 获取）</param>
    public void SetBackground(uint textureId)
    {
        _currentBackgroundTexture = textureId;
        Console.WriteLine($"[BackgroundRenderer] Background set to texture ID: {textureId}");
    }

    /// <summary>
    /// 设置相机偏移（用于场景滚动）
    /// </summary>
    /// <param name="offset">世界坐标偏移（米）</param>
    public void SetCameraOffset(Vector2 offset)
    {
        _cameraOffset = offset;
    }

    /// <summary>
    /// 渲染背景
    /// </summary>
    /// <param name="projectionMatrix">投影矩阵</param>
    public void Render(Matrix4x4 projectionMatrix)
    {
        if (_currentBackgroundTexture == 0)
        {
            // Why: 没有背景纹理时跳过渲染（显示黑色背景）
            return;
        }

        // Why: 计算背景渲染位置（应用相机偏移）
        // Context: 背景锚点在世界坐标原点 (0, 0)，应用负的相机偏移实现滚动效果
        Vector2 backgroundPos = -_cameraOffset;

        // Why: 使用 SpriteRenderer 渲染全屏背景
        // Context: 背景纹理无需 sourceRect（使用整个纹理），无需 tint（保持原色）
        _spriteRenderer.DrawSprite(
            _currentBackgroundTexture,
            backgroundPos,
            _backgroundSize,
            projectionMatrix,
            tintColor: null,
            sourceRect: null
        );
    }

    /// <summary>
    /// 释放资源（BackgroundRenderer 本身不持有 OpenGL 资源，由 SpriteRenderer 管理）
    /// </summary>
    public void Dispose()
    {
        // Why: BackgroundRenderer 复用 SpriteRenderer，无需释放额外资源
        Console.WriteLine("[BackgroundRenderer] Disposed (no resources to release).");
    }
}
