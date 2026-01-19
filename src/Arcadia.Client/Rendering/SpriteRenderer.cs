using Silk.NET.OpenGL;
using System.Numerics;

namespace Arcadia.Client.Rendering;

/// <summary>
/// 精灵渲染器 - 负责渲染带纹理的 2D 精灵
/// Why: 将 TextureManager 加载的纹理绘制到屏幕上
/// Context: 支持精灵表（Sprite Sheet）的帧选择，用于角色动画
/// Attention: 使用 Nearest 过滤以保持像素艺术风格，UV 坐标需正确映射精灵表帧
/// </summary>
public sealed class SpriteRenderer : IDisposable
{
    private readonly GL _gl;

    // Why: VBO 存储顶点数据（位置 + UV 坐标）
    private uint _vbo;
    private uint _vao;
    private uint _shaderProgram;

    // Why: 精灵渲染使用带纹理的 Shader
    private const string VertexShaderSource = @"
#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

uniform mat4 uProjection;
uniform mat4 uModel;

void main()
{
    gl_Position = uProjection * uModel * vec4(aPosition, 0.0, 1.0);
    TexCoord = aTexCoord;
}
";

    private const string FragmentShaderSource = @"
#version 330 core
in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D uTexture;
uniform vec4 uTintColor;

void main()
{
    vec4 texColor = texture(uTexture, TexCoord);
    FragColor = texColor * uTintColor;
}
";

    public SpriteRenderer(GL gl)
    {
        _gl = gl;
        InitializeBuffers();
        InitializeShaders();
        Console.WriteLine("[SpriteRenderer] Initialized with texture support.");
    }

    private unsafe void InitializeBuffers()
    {
        // Why: 创建一个单位方块（0,0 到 1,1），通过 Model 矩阵缩放到实际大小
        // Context: UV 坐标默认覆盖整个纹理（0,0 到 1,1），可通过参数指定精灵表的子区域
        float[] vertices = {
            // Position      UV
            0.0f, 0.0f,     0.0f, 1.0f,  // 左下
            1.0f, 0.0f,     1.0f, 1.0f,  // 右下
            1.0f, 1.0f,     1.0f, 0.0f,  // 右上

            1.0f, 1.0f,     1.0f, 0.0f,  // 右上
            0.0f, 1.0f,     0.0f, 0.0f,  // 左上
            0.0f, 0.0f,     0.0f, 1.0f   // 左下
        };

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* v = &vertices[0])
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
        }

        // Why: Position 属性（location 0）：2 个 float
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)0);
        _gl.EnableVertexAttribArray(0);

        // Why: UV 属性（location 1）：2 个 float
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        _gl.BindVertexArray(0);
    }

    private void InitializeShaders()
    {
        // Why: 编译顶点着色器
        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, VertexShaderSource);
        _gl.CompileShader(vertexShader);
        CheckShaderCompileErrors(vertexShader, "VERTEX");

        // Why: 编译片段着色器
        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, FragmentShaderSource);
        _gl.CompileShader(fragmentShader);
        CheckShaderCompileErrors(fragmentShader, "FRAGMENT");

        // Why: 链接着色器程序
        _shaderProgram = _gl.CreateProgram();
        _gl.AttachShader(_shaderProgram, vertexShader);
        _gl.AttachShader(_shaderProgram, fragmentShader);
        _gl.LinkProgram(_shaderProgram);
        CheckProgramLinkErrors(_shaderProgram);

        // Why: 着色器已链接，删除中间对象
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }

    private void CheckShaderCompileErrors(uint shader, string type)
    {
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = _gl.GetShaderInfoLog(shader);
            throw new Exception($"[SpriteRenderer] Shader compilation failed ({type}):\n{infoLog}");
        }
    }

    private void CheckProgramLinkErrors(uint program)
    {
        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = _gl.GetProgramInfoLog(program);
            throw new Exception($"[SpriteRenderer] Program linking failed:\n{infoLog}");
        }
    }

    /// <summary>
    /// 绘制精灵
    /// </summary>
    /// <param name="textureId">OpenGL 纹理 ID（从 TextureManager 获取）</param>
    /// <param name="position">世界坐标位置（米）</param>
    /// <param name="size">精灵大小（米）</param>
    /// <param name="projectionMatrix">投影矩阵</param>
    /// <param name="tintColor">着色颜色（默认白色，乘以纹理颜色）</param>
    /// <param name="sourceRect">精灵表中的源区域（归一化 UV 坐标：x, y, width, height），默认 null 使用整个纹理</param>
    public unsafe void DrawSprite(
        uint textureId,
        Vector2 position,
        Vector2 size,
        Matrix4x4 projectionMatrix,
        Vector4? tintColor = null,
        Vector4? sourceRect = null)
    {
        _gl.UseProgram(_shaderProgram);

        // Why: 构建 Model 矩阵（平移 + 缩放）
        var modelMatrix = Matrix4x4.CreateScale(size.X, size.Y, 1.0f) *
                          Matrix4x4.CreateTranslation(position.X, position.Y, 0.0f);

        // Why: 传递 Uniform 变量到着色器
        int projLoc = _gl.GetUniformLocation(_shaderProgram, "uProjection");
        int modelLoc = _gl.GetUniformLocation(_shaderProgram, "uModel");
        int tintLoc = _gl.GetUniformLocation(_shaderProgram, "uTintColor");

        _gl.UniformMatrix4(projLoc, 1, false, (float*)&projectionMatrix);
        _gl.UniformMatrix4(modelLoc, 1, false, (float*)&modelMatrix);

        Vector4 finalTint = tintColor ?? new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        _gl.Uniform4(tintLoc, finalTint.X, finalTint.Y, finalTint.Z, finalTint.W);

        // Why: 绑定纹理
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, textureId);
        _gl.Uniform1(_gl.GetUniformLocation(_shaderProgram, "uTexture"), 0);

        // Why: 如果指定了 sourceRect，需要更新 VBO 的 UV 坐标
        // Context: 用于从精灵表中选择特定帧
        if (sourceRect.HasValue)
        {
            UpdateUVCoordinates(sourceRect.Value);
        }

        // Why: 绘制精灵
        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        _gl.BindVertexArray(0);

        // Why: 如果修改了 UV，恢复默认（整个纹理）
        if (sourceRect.HasValue)
        {
            ResetUVCoordinates();
        }
    }

    private unsafe void UpdateUVCoordinates(Vector4 rect)
    {
        // Why: rect = (u, v, width, height) 归一化坐标
        float u = rect.X;
        float v = rect.Y;
        float w = rect.Z;
        float h = rect.W;

        // Why: 更新 UV 坐标以匹配精灵表中的帧
        // Context: OpenGL 纹理坐标原点在左下角（V=0）
        float[] uvData = {
            // Position      UV
            0.0f, 0.0f,     u,     v + h,  // 左下
            1.0f, 0.0f,     u + w, v + h,  // 右下
            1.0f, 1.0f,     u + w, v,      // 右上

            1.0f, 1.0f,     u + w, v,      // 右上
            0.0f, 1.0f,     u,     v,      // 左上
            0.0f, 0.0f,     u,     v + h   // 左下
        };

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* data = &uvData[0])
        {
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(uvData.Length * sizeof(float)), data);
        }
    }

    private unsafe void ResetUVCoordinates()
    {
        // Why: 恢复默认 UV（0,0 到 1,1）
        float[] defaultVertices = {
            // Position      UV
            0.0f, 0.0f,     0.0f, 1.0f,  // 左下
            1.0f, 0.0f,     1.0f, 1.0f,  // 右下
            1.0f, 1.0f,     1.0f, 0.0f,  // 右上

            1.0f, 1.0f,     1.0f, 0.0f,  // 右上
            0.0f, 1.0f,     0.0f, 0.0f,  // 左上
            0.0f, 0.0f,     0.0f, 1.0f   // 左下
        };

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* data = &defaultVertices[0])
        {
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(defaultVertices.Length * sizeof(float)), data);
        }
    }

    public void Dispose()
    {
        // Why: 释放 GPU 资源
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteProgram(_shaderProgram);
        Console.WriteLine("[SpriteRenderer] Disposed.");
    }
}
