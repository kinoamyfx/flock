using Silk.NET.OpenGL;
using System.Numerics;

namespace Arcadia.Client.Rendering;

/// <summary>
/// 现代 OpenGL 3.3+ 方块渲染器（VBO + VAO + Shader）
/// Why: Apple M1 Metal 不支持 Immediate Mode，必须用现代 OpenGL。
/// Context: 使用 VBO 存储顶点数据，Shader 处理渲染管线。
/// Attention: 每次 Draw 都更新 VBO（性能足够 MVP，后续可优化为 Instancing）。
/// </summary>
public sealed class ModernQuadRenderer : IDisposable
{
    private readonly GL _gl;
    private uint _shaderProgram;
    private uint _vao;
    private uint _vbo;
    private int _projectionLoc;
    private Matrix4x4 _currentProjection = Matrix4x4.Identity;

    private const string VertexShaderSource = @"
#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec4 aColor;

out vec4 vColor;

uniform mat4 uProjection;

void main()
{
    gl_Position = uProjection * vec4(aPos, 0.0, 1.0);
    vColor = aColor;
}
";

    private const string FragmentShaderSource = @"
#version 330 core
in vec4 vColor;
out vec4 FragColor;

void main()
{
    FragColor = vColor;
}
";

    public ModernQuadRenderer(GL gl)
    {
        _gl = gl;

        // Why: 编译 Shader 并创建 Program。
        _shaderProgram = CreateShaderProgram();
        _projectionLoc = _gl.GetUniformLocation(_shaderProgram, "uProjection");

        // Why: 创建 VAO 和 VBO。
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        // Why: 定义顶点布局（位置 vec2 + 颜色 vec4 = 6 个 float）。
        unsafe
        {
            var stride = 6 * sizeof(float);
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)stride, (void*)0);
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, (uint)stride, (void*)(2 * sizeof(float)));
            _gl.EnableVertexAttribArray(1);
        }

        _gl.BindVertexArray(0);

        Console.WriteLine("[ModernQuadRenderer] Initialized with VBO + Shader.");
    }

    /// <summary>
    /// 设置投影矩阵。
    /// </summary>
    public void SetProjectionMatrix(Matrix4x4 projection)
    {
        _currentProjection = projection;
    }

    /// <summary>
    /// 绘制一个彩色方块。
    /// </summary>
    public void DrawQuad(Vector2 position, Vector2 size, Vector4 color)
    {
        float x = position.X;
        float y = position.Y;
        float w = size.X / 2.0f;
        float h = size.Y / 2.0f;

        // Why: 6 个顶点（每个顶点 6 个 float：x, y, r, g, b, a）。
        float[] vertices = new float[]
        {
            // Quad vertices (两个三角形组成方块)
            // Triangle 1
            x - w, y - h, color.X, color.Y, color.Z, color.W, // 左下
            x + w, y - h, color.X, color.Y, color.Z, color.W, // 右下
            x + w, y + h, color.X, color.Y, color.Z, color.W, // 右上

            // Triangle 2
            x + w, y + h, color.X, color.Y, color.Z, color.W, // 右上
            x - w, y + h, color.X, color.Y, color.Z, color.W, // 左上
            x - w, y - h, color.X, color.Y, color.Z, color.W, // 左下
        };

        // Why: 上传顶点数据到 VBO。
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        unsafe
        {
            fixed (float* v = vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.DynamicDraw);
            }
        }

        // Why: 绘制方块。
        _gl.UseProgram(_shaderProgram);

        // Why: 上传投影矩阵到 Shader。
        unsafe
        {
            var proj = _currentProjection;
            _gl.UniformMatrix4(_projectionLoc, 1, false, (float*)&proj);
        }

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        _gl.BindVertexArray(0);
    }

    private uint CreateShaderProgram()
    {
        // Why: 编译顶点 Shader。
        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, VertexShaderSource);
        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
        {
            string log = _gl.GetShaderInfoLog(vertexShader);
            throw new Exception($"Vertex shader compilation failed: {log}");
        }

        // Why: 编译片段 Shader。
        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, FragmentShaderSource);
        _gl.CompileShader(fragmentShader);

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int)GLEnum.True)
        {
            string log = _gl.GetShaderInfoLog(fragmentShader);
            throw new Exception($"Fragment shader compilation failed: {log}");
        }

        // Why: 链接 Shader Program。
        uint program = _gl.CreateProgram();
        _gl.AttachShader(program, vertexShader);
        _gl.AttachShader(program, fragmentShader);
        _gl.LinkProgram(program);

        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
        {
            string log = _gl.GetProgramInfoLog(program);
            throw new Exception($"Shader program linking failed: {log}");
        }

        // Why: 清理 Shader 对象（已链接到 Program）。
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        return program;
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteProgram(_shaderProgram);
    }
}
