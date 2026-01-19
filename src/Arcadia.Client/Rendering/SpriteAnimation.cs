using System.Numerics;

namespace Arcadia.Client.Rendering;

/// <summary>
/// 精灵动画系统 - 负责管理精灵表的帧选择和动画播放
/// Why: 根据玩家状态（移动方向）选择不同的精灵帧
/// Context: 假设精灵表布局为 4x3 网格（12 帧），每帧 320x240 像素（从 1280x720 总尺寸计算）
/// Attention: UV 坐标为归一化坐标（0.0-1.0），需要将像素坐标转换为 UV 坐标
/// </summary>
public class SpriteAnimation
{
    // Why: 精灵表的网格布局（列数 x 行数）
    public int Columns { get; }
    public int Rows { get; }

    // Why: 每帧的 UV 坐标尺寸（归一化）
    private readonly float _frameWidth;
    private readonly float _frameHeight;

    // Why: 当前动画状态
    private int _currentFrame = 0;
    private double _frameTimer = 0.0;

    // Why: 动画播放速度（秒/帧）
    public double FrameDuration { get; set; } = 0.15; // 默认 6.67 FPS

    // Why: 方向到帧索引的映射（简单实现：4 个方向各用 1 帧）
    // Context: 帧索引按从左到右、从上到下的顺序排列（0, 1, 2, ..., 11）
    private static readonly Dictionary<PlayerDirection, int> DirectionFrames = new()
    {
        { PlayerDirection.Idle, 0 },       // 第 1 行第 1 列：静止
        { PlayerDirection.Down, 4 },       // 第 2 行第 1 列：向下
        { PlayerDirection.Up, 5 },         // 第 2 行第 2 列：向上
        { PlayerDirection.Left, 6 },       // 第 2 行第 3 列：向左
        { PlayerDirection.Right, 7 },      // 第 2 行第 4 列：向右
    };

    public SpriteAnimation(int columns = 4, int rows = 3)
    {
        Columns = columns;
        Rows = rows;
        _frameWidth = 1.0f / columns;
        _frameHeight = 1.0f / rows;
    }

    /// <summary>
    /// 更新动画状态（切换帧）
    /// </summary>
    /// <param name="deltaTime">距离上次更新的时间（秒）</param>
    /// <param name="direction">当前移动方向</param>
    public void Update(double deltaTime, PlayerDirection direction)
    {
        // Why: 根据方向选择目标帧
        int targetFrame = DirectionFrames.GetValueOrDefault(direction, 0);

        // Why: 简单实现：直接切换到目标帧（无过渡动画）
        // Context: MVP 版本暂时不实现帧间平滑过渡和循环动画
        _currentFrame = targetFrame;

        // Why: 更新帧计时器（预留给未来的帧循环动画）
        _frameTimer += deltaTime;
        if (_frameTimer >= FrameDuration)
        {
            _frameTimer = 0.0;
        }
    }

    /// <summary>
    /// 获取当前帧的 UV 坐标矩形（用于 SpriteRenderer.DrawSprite 的 sourceRect 参数）
    /// </summary>
    /// <returns>Vector4(u, v, width, height)，归一化坐标</returns>
    public Vector4 GetCurrentFrameUV()
    {
        // Why: 将帧索引转换为网格坐标
        int col = _currentFrame % Columns;
        int row = _currentFrame / Columns;

        // Why: 计算 UV 坐标（OpenGL 纹理坐标原点在左下角，V 轴向上）
        // Context: 精灵表图片坐标原点在左上角，需要翻转 Y 轴
        float u = col * _frameWidth;
        float v = (Rows - row - 1) * _frameHeight; // 翻转 Y 轴：从底部开始计数

        return new Vector4(u, v, _frameWidth, _frameHeight);
    }

    /// <summary>
    /// 重置动画状态到初始帧
    /// </summary>
    public void Reset()
    {
        _currentFrame = 0;
        _frameTimer = 0.0;
    }
}

/// <summary>
/// 玩家移动方向枚举
/// </summary>
public enum PlayerDirection
{
    Idle,   // 静止
    Up,     // 向上
    Down,   // 向下
    Left,   // 向左
    Right   // 向右
}
