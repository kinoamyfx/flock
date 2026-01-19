using Silk.NET.OpenGL;
using StbImageSharp;
using System.Collections.Generic;

namespace Arcadia.Client.Rendering;

/// <summary>
/// 纹理管理器 - 负责加载、缓存和管理 OpenGL 纹理
/// Why: 集中管理纹理资源,避免重复加载,提供统一的纹理访问接口
/// Context: 使用 StbImageSharp 加载 PNG/JPG 图片,创建 OpenGL 纹理对象
/// Attention: 所有纹理在 Dispose 时必须释放,避免 GPU 内存泄漏
/// </summary>
public sealed class TextureManager : IDisposable
{
    private readonly GL _gl;
    private readonly string _assetRoot;

    // Why: 缓存已加载的纹理,key=路径,value=纹理ID
    private readonly Dictionary<string, uint> _textureCache = new();

    public TextureManager(GL gl, string assetRoot)
    {
        _gl = gl;
        _assetRoot = assetRoot;
        Console.WriteLine($"[TextureManager] Initialized with asset root: {assetRoot}");
    }

    /// <summary>
    /// 加载纹理(如果已缓存则直接返回)
    /// </summary>
    /// <param name="relativePath">相对于 assetRoot 的路径(例如 "sprites/player.png")</param>
    /// <returns>OpenGL 纹理 ID</returns>
    public uint LoadTexture(string relativePath)
    {
        // Why: 检查缓存,避免重复加载
        if (_textureCache.TryGetValue(relativePath, out uint cachedTextureId))
        {
            Console.WriteLine($"[TextureManager] Texture already cached: {relativePath} (ID: {cachedTextureId})");
            return cachedTextureId;
        }

        string fullPath = Path.Combine(_assetRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"[TextureManager] Texture file not found: {fullPath}");
        }

        Console.WriteLine($"[TextureManager] Loading texture: {fullPath}");

        // Why: 使用 StbImageSharp 加载图片数据
        ImageResult image;
        using (var stream = File.OpenRead(fullPath))
        {
            // Why: RGBA 格式,每像素 4 字节(Red, Green, Blue, Alpha)
            image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        }

        if (image.Data == null || image.Data.Length == 0)
        {
            throw new InvalidDataException($"[TextureManager] Failed to load image data from: {fullPath}");
        }

        Console.WriteLine($"[TextureManager] Image loaded: {image.Width}x{image.Height}, {image.Data.Length} bytes");

        // Why: 创建 OpenGL 纹理对象
        uint textureId = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, textureId);

        // Why: 上传图片数据到 GPU
        unsafe
        {
            fixed (byte* dataPtr = image.Data)
            {
                _gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    InternalFormat.Rgba,
                    (uint)image.Width,
                    (uint)image.Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    dataPtr
                );
            }
        }

        // Why: 设置纹理过滤模式(像素艺术风格使用 Nearest,避免模糊)
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        // Why: 设置纹理环绕模式(ClampToEdge 避免边缘重复)
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        _gl.BindTexture(TextureTarget.Texture2D, 0);

        Console.WriteLine($"[TextureManager] Texture created: ID={textureId}, Size={image.Width}x{image.Height}");

        // Why: 缓存纹理 ID
        _textureCache[relativePath] = textureId;

        return textureId;
    }

    /// <summary>
    /// 获取已加载的纹理 ID(不存在则返回 0)
    /// </summary>
    public uint GetTexture(string relativePath)
    {
        return _textureCache.TryGetValue(relativePath, out uint textureId) ? textureId : 0;
    }

    /// <summary>
    /// 绑定纹理到当前 OpenGL 上下文
    /// </summary>
    public void BindTexture(uint textureId)
    {
        _gl.BindTexture(TextureTarget.Texture2D, textureId);
    }

    public void Dispose()
    {
        // Why: 释放所有纹理,避免 GPU 内存泄漏
        Console.WriteLine($"[TextureManager] Disposing {_textureCache.Count} textures...");
        foreach (var (path, textureId) in _textureCache)
        {
            _gl.DeleteTexture(textureId);
            Console.WriteLine($"[TextureManager] Deleted texture: {path} (ID: {textureId})");
        }
        _textureCache.Clear();
    }
}
