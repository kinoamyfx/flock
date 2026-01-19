using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;

namespace Arcadia.Client.Rendering;

/// <summary>
/// 现代 OpenGL 3.3+ 渲染器（清屏 + 2D Quad 绘制）
/// Why: 提供基础渲染能力，支持 2D 方块绘制（代表玩家/敌人/战利品）。
/// Context: MVP 阶段使用简单的 VBO 渲染，后续可升级为 Instancing / Sprite Batching。
/// Attention: 使用正交投影（Orthographic Projection）适配 2D 游戏坐标系。
/// </summary>
public sealed class ModernRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly IWindow _window;

    // Why: 深灰色背景（增加对比度，更容易看到绿色方块）。
    private readonly Vector3 _clearColor = new(0.15f, 0.15f, 0.18f);

    public ModernRenderer(IWindow window, GL gl)
    {
        _window = window;
        _gl = gl;

        Console.WriteLine("[ModernRenderer] OpenGL initialized.");
        Console.WriteLine($"[ModernRenderer] Vendor: {_gl.GetStringS(StringName.Vendor)}");
        Console.WriteLine($"[ModernRenderer] Renderer: {_gl.GetStringS(StringName.Renderer)}");
        Console.WriteLine($"[ModernRenderer] Version: {_gl.GetStringS(StringName.Version)}");

        // Why: 启用混合模式（Alpha 通道支持）。
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Why: 确保视口设置正确。
        _gl.Viewport(0, 0, (uint)window.Size.X, (uint)window.Size.Y);
        Console.WriteLine($"[ModernRenderer] Viewport set to {window.Size.X}x{window.Size.Y}");
    }

    /// <summary>
    /// 清屏为深色背景。
    /// </summary>
    public void Clear()
    {
        _gl.ClearColor(_clearColor.X, _clearColor.Y, _clearColor.Z, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    /// <summary>
    /// 获取正交投影矩阵（2D 游戏坐标系）。
    /// Why: 将世界坐标（单位：米）映射到屏幕坐标。
    /// Context: 使用 0,0 为世界中心，Y 轴向上。
    /// </summary>
    public Matrix4x4 GetProjectionMatrix()
    {
        var size = _window.Size;
        float aspect = (float)size.X / size.Y;

        // Why: 视野高度固定为 20 单位（约 20 米），宽度根据屏幕比例调整。
        float viewHeight = 20.0f;
        float viewWidth = viewHeight * aspect;

        return Matrix4x4.CreateOrthographicOffCenter(
            -viewWidth / 2, viewWidth / 2,   // Left, Right
            -viewHeight / 2, viewHeight / 2, // Bottom, Top
            -1.0f, 1.0f                      // Near, Far
        );
    }

    public void Dispose()
    {
        _gl?.Dispose();
    }
}
