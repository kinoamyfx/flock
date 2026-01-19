using Silk.NET.OpenGL;
using System.Numerics;

namespace Arcadia.Client.Rendering;

/// <summary>
/// HUD 渲染器 - 负责渲染玩家状态 UI（血条、灵力条等）
/// Why: 向玩家展示当前状态（HP, Spirit）
/// Context: MVP 版本使用彩色进度条代替文本渲染（简化实现）
/// Attention: HUD 渲染在最上层（Z-order），使用屏幕坐标系而非世界坐标系
/// </summary>
public sealed class HUDRenderer : IDisposable
{
    private readonly ModernQuadRenderer _quadRenderer;

    // Why: HUD 元素的屏幕位置和尺寸（世界坐标，左上角锚点）
    private readonly Vector2 _healthBarPos = new(5.0f, 67.0f);   // 左上角偏移 5 米
    private readonly Vector2 _spiritBarPos = new(5.0f, 64.0f);   // 血条下方 3 米
    private readonly Vector2 _barSize = new(20.0f, 2.0f);        // 进度条尺寸：20 米宽 x 2 米高

    // Why: 进度条颜色配置
    private readonly Vector4 _healthColor = new(0.8f, 0.2f, 0.2f, 1.0f);  // 红色（血条）
    private readonly Vector4 _spiritColor = new(0.2f, 0.5f, 1.0f, 1.0f);  // 蓝色（灵力条）
    private readonly Vector4 _bgColor = new(0.2f, 0.2f, 0.2f, 0.8f);      // 深灰色半透明（背景）

    public HUDRenderer(ModernQuadRenderer quadRenderer)
    {
        _quadRenderer = quadRenderer;
        Console.WriteLine("[HUDRenderer] Initialized with colored progress bars for HP/Spirit.");
    }

    /// <summary>
    /// 渲染 HUD（血条、灵力条）
    /// </summary>
    /// <param name="projectionMatrix">投影矩阵</param>
    /// <param name="currentHP">当前 HP 值</param>
    /// <param name="maxHP">最大 HP 值</param>
    /// <param name="currentSpirit">当前 Spirit 值</param>
    /// <param name="maxSpirit">最大 Spirit 值</param>
    public void Render(Matrix4x4 projectionMatrix, int currentHP, int maxHP, int currentSpirit, int maxSpirit)
    {
        _quadRenderer.SetProjectionMatrix(projectionMatrix);

        // Why: 渲染血条（背景 + 前景）
        RenderProgressBar(
            _healthBarPos,
            _barSize,
            currentHP,
            maxHP,
            _healthColor,
            _bgColor
        );

        // Why: 渲染灵力条（背景 + 前景）
        RenderProgressBar(
            _spiritBarPos,
            _barSize,
            currentSpirit,
            maxSpirit,
            _spiritColor,
            _bgColor
        );
    }

    /// <summary>
    /// 渲染单个进度条（背景 + 前景）
    /// </summary>
    private void RenderProgressBar(
        Vector2 position,
        Vector2 size,
        int current,
        int max,
        Vector4 foregroundColor,
        Vector4 backgroundColor)
    {
        // Why: 渲染背景条（全宽）
        _quadRenderer.DrawQuad(position, size, backgroundColor);

        // Why: 计算前景条宽度（根据当前值 / 最大值）
        float fillRatio = max > 0 ? Math.Clamp((float)current / max, 0.0f, 1.0f) : 0.0f;
        Vector2 fillSize = new(size.X * fillRatio, size.Y);

        // Why: 渲染前景条（当前值占比）
        if (fillRatio > 0.0f)
        {
            _quadRenderer.DrawQuad(position, fillSize, foregroundColor);
        }
    }

    public void Dispose()
    {
        // Why: HUDRenderer 复用 ModernQuadRenderer，无需释放额外资源
        Console.WriteLine("[HUDRenderer] Disposed (no resources to release).");
    }
}
