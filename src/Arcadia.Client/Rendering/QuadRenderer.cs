using Silk.NET.OpenGL.Legacy;
using System.Numerics;

namespace Arcadia.Client.Rendering;

/// <summary>
/// 2D 方块渲染器（简化的 Sprite 渲染）
/// Why: 快速实现 MVP 阶段的 2D 渲染需求（玩家/敌人用彩色方块表示）。
/// Context: 使用 Immediate Mode（glBegin/glEnd），后续可升级为 VBO + Shader。
/// Attention: 性能不是最优，但足够 MVP 使用（< 100 个方块时无压力）。
/// </summary>
public sealed class QuadRenderer
{
    private readonly GL _gl;
    private bool _debugLogged = false;

    public QuadRenderer(GL gl)
    {
        _gl = gl;
    }

    /// <summary>
    /// 绘制一个彩色方块。
    /// </summary>
    /// <param name="position">世界坐标（米）</param>
    /// <param name="size">方块大小（米）</param>
    /// <param name="color">RGBA 颜色</param>
    public void DrawQuad(Vector2 position, Vector2 size, Vector4 color)
    {
        float x = position.X;
        float y = position.Y;
        float w = size.X;
        float h = size.Y;

        // Why: 第一次渲染时打印调试信息。
        if (!_debugLogged)
        {
            Console.WriteLine($"[QuadRenderer] DrawQuad called: pos=({x:F2}, {y:F2}), size=({w:F2}, {h:F2}), color=({color.X:F2}, {color.Y:F2}, {color.Z:F2}, {color.W:F2})");
            _debugLogged = true;
        }

        // Why: 使用 Immediate Mode 绘制四边形（临时方案，后续用 VBO）。
        _gl.Begin(PrimitiveType.Quads);
        _gl.Color4(color.X, color.Y, color.Z, color.W);

        _gl.Vertex2(x - w / 2, y - h / 2); // 左下
        _gl.Vertex2(x + w / 2, y - h / 2); // 右下
        _gl.Vertex2(x + w / 2, y + h / 2); // 右上
        _gl.Vertex2(x - w / 2, y + h / 2); // 左上

        _gl.End();
    }

    /// <summary>
    /// 设置投影矩阵。
    /// </summary>
    public void SetProjectionMatrix(Matrix4x4 projection)
    {
        _gl.MatrixMode(MatrixMode.Projection);
        _gl.LoadMatrix(GetMatrixArray(projection));
        _gl.MatrixMode(MatrixMode.Modelview);
        _gl.LoadIdentity();
    }

    private float[] GetMatrixArray(Matrix4x4 matrix)
    {
        // Why: OpenGL 使用列主序（Column-Major），需要转置矩阵。
        return new float[]
        {
            matrix.M11, matrix.M21, matrix.M31, matrix.M41,
            matrix.M12, matrix.M22, matrix.M32, matrix.M42,
            matrix.M13, matrix.M23, matrix.M33, matrix.M43,
            matrix.M14, matrix.M24, matrix.M34, matrix.M44
        };
    }
}
