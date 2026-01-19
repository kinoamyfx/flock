using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
using System.Text.Json;
using Arcadia.Client.Rendering;
using Arcadia.Client.Net;
using Arcadia.Core.Net.Zone;

namespace Arcadia.Client;

/// <summary>
/// Arcadia Client - Silk.NET + OpenGL + ECS
/// MVP 可玩版本：窗口 + OpenGL 渲染 + 网络 + ECS
/// </summary>
public class Program
{
    private static IWindow? _window;
    private static IInputContext? _inputContext;
    private static GL? _gl;
    private static ModernRenderer? _renderer;
    private static ModernQuadRenderer? _quadRenderer;
    private static SpriteRenderer? _spriteRenderer;
    private static BackgroundRenderer? _backgroundRenderer;
    private static HUDRenderer? _hudRenderer;
    private static TextureManager? _textureManager;
    private static NetworkClient? _networkClient;
    private static SpriteAnimation? _playerAnimation;

    // Why: 纹理 ID（从 TextureManager 加载）。
    private static uint _playerSpriteTexture;
    private static uint _townScene1Texture;

    // Why: 玩家位置（世界坐标，会被服务端 Snapshot 更新）。
    private static Vector2 _playerPos = Vector2.Zero;
    private static readonly Vector2 _playerSize = new(2.0f, 2.0f); // 2x2 米精灵（像素艺术风格）
    private static PlayerDirection _playerDirection = PlayerDirection.Idle;

    // Why: 玩家状态（MVP 版本使用本地变量，后续会被服务端 Snapshot 同步）。
    private static int _playerHP = 80;
    private static int _playerMaxHP = 100;
    private static int _playerSpirit = 45;
    private static int _playerMaxSpirit = 60;

    // Why: Zone Server 地址（连接 localhost:7777）。
    private const string ZoneHost = "127.0.0.1";
    private const ushort ZonePort = 7777;

    // Why: 玩家移动速度（米/秒）。
    private const float MoveSpeed = 5.0f;

    // Why: 键盘状态（WASD 是否按下）。
    private static readonly HashSet<Key> _pressedKeys = new();

    public static void Main(string[] args)
    {
        Console.WriteLine("[Arcadia.Client] Initializing...");

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1280, 720);
        options.Title = "Arcadia - MVP Client";
        options.VSync = true;

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClosing;

        Console.WriteLine("[Arcadia.Client] Starting window...");
        _window.Run();
        Console.WriteLine("[Arcadia.Client] Window closed.");
    }

    private static void OnLoad()
    {
        Console.WriteLine("[Arcadia.Client] Window loaded!");

        // Why: 创建现代 OpenGL 上下文（3.3+）。
        _gl = _window!.CreateOpenGL();
        _renderer = new ModernRenderer(_window!, _gl);
        _quadRenderer = new ModernQuadRenderer(_gl);
        _spriteRenderer = new SpriteRenderer(_gl);
        _playerAnimation = new SpriteAnimation(columns: 4, rows: 3); // 4x3 网格，12 帧
        _backgroundRenderer = new BackgroundRenderer(_gl, _spriteRenderer); // 复用 SpriteRenderer 渲染背景
        _hudRenderer = new HUDRenderer(_quadRenderer); // 复用 QuadRenderer 渲染 HUD 进度条

        // Why: 创建纹理管理器并加载资产。
        // Context: 从 bin/Debug/net10.0/ 回退 5 级到项目根（bin -> Debug -> net10.0 -> Arcadia.Client -> src -> Arcadia）
        string assetRoot = Path.Combine(AppContext.BaseDirectory, "../../../../..", "assets");
        _textureManager = new TextureManager(_gl, assetRoot);

        // Why: 加载游戏资产（角色精灵图 + 城镇场景）。
        try
        {
            _playerSpriteTexture = _textureManager.LoadTexture("sprites/player_spritesheet.png");
            _townScene1Texture = _textureManager.LoadTexture("scenes/town_scene_1.png");
            uint townScene2Texture = _textureManager.LoadTexture("scenes/town_scene_2.png");

            Console.WriteLine($"[Arcadia.Client] Assets loaded successfully:");
            Console.WriteLine($"  - Player Sprite: Texture ID {_playerSpriteTexture}");
            Console.WriteLine($"  - Town Scene 1: Texture ID {_townScene1Texture}");
            Console.WriteLine($"  - Town Scene 2: Texture ID {townScene2Texture}");

            // Why: 设置默认背景为城镇场景 1
            _backgroundRenderer!.SetBackground(_townScene1Texture);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Arcadia.Client] WARNING: Failed to load assets: {ex.Message}");
        }

        // Why: 创建输入上下文。
        _inputContext = _window.CreateInput();
        foreach (var keyboard in _inputContext.Keyboards)
        {
            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;
        }

        // TODO: 创建网络客户端并连接服务器（暂时禁用，ENet 版本问题）。
        // _networkClient = new NetworkClient();
        // _networkClient.Connected += OnNetworkConnected;
        // _networkClient.MessageReceived += OnNetworkMessage;
        // _networkClient.Connect(ZoneHost, ZonePort);

        Console.WriteLine($"[Arcadia.Client] Input initialized. Keyboards: {_inputContext.Keyboards.Count}");
        Console.WriteLine("[Arcadia.Client] [LOCAL MODE] Network disabled. Use WASD to move locally.");
    }

    private static double _elapsedTime = 0.0;
    private static int _frameCount = 0;

    private static void OnUpdate(double deltaTime)
    {
        // Why: FPS 统计（每秒打印一次）。
        _elapsedTime += deltaTime;
        _frameCount++;

        if (_elapsedTime >= 1.0)
        {
            Console.WriteLine($"[Arcadia.Client] FPS: {_frameCount} | Pos: ({_playerPos.X:F1}, {_playerPos.Y:F1})");
            _elapsedTime = 0.0;
            _frameCount = 0;
        }

        // Why: 轮询网络事件（接收 Snapshot 等消息）。
        _networkClient?.Poll();

        // Why: 处理 WASD 移动输入（本地模式）。
        var moveDir = Vector2.Zero;
        PlayerDirection newDirection = PlayerDirection.Idle;

        if (_pressedKeys.Contains(Key.W)) { moveDir.Y += 1; newDirection = PlayerDirection.Up; }    // 上
        if (_pressedKeys.Contains(Key.S)) { moveDir.Y -= 1; newDirection = PlayerDirection.Down; }  // 下
        if (_pressedKeys.Contains(Key.A)) { moveDir.X -= 1; newDirection = PlayerDirection.Left; }  // 左
        if (_pressedKeys.Contains(Key.D)) { moveDir.X += 1; newDirection = PlayerDirection.Right; } // 右

        if (moveDir != Vector2.Zero)
        {
            // Why: 归一化移动向量（防止斜向移动速度过快）。
            moveDir = Vector2.Normalize(moveDir);
            _playerPos += moveDir * MoveSpeed * (float)deltaTime;

            // Why: 更新玩家朝向（斜向移动时优先使用水平方向）。
            // Context: 如果同时按下多个方向键，优先显示最后按下的方向
            if (_pressedKeys.Contains(Key.D)) newDirection = PlayerDirection.Right;
            else if (_pressedKeys.Contains(Key.A)) newDirection = PlayerDirection.Left;
            else if (_pressedKeys.Contains(Key.W)) newDirection = PlayerDirection.Up;
            else if (_pressedKeys.Contains(Key.S)) newDirection = PlayerDirection.Down;
        }

        // Why: 更新玩家方向和动画状态。
        _playerDirection = newDirection;
        _playerAnimation?.Update(deltaTime, _playerDirection);
    }

    private static void OnRender(double deltaTime)
    {
        // Why: 清屏为深色背景。
        _renderer!.Clear();

        // Why: 设置正交投影矩阵（2D 坐标系）。
        var projection = _renderer.GetProjectionMatrix();

        // Why: 渲染背景（最底层，Z-order = 0）。
        _backgroundRenderer?.Render(projection);

        // Why: 绘制玩家精灵（使用动画系统选择当前帧）。
        // Context: 动画系统根据移动方向自动选择对应的精灵帧
        if (_playerSpriteTexture != 0 && _playerAnimation != null)
        {
            Vector4 frameUV = _playerAnimation.GetCurrentFrameUV();
            _spriteRenderer!.DrawSprite(
                _playerSpriteTexture,
                _playerPos,
                _playerSize,
                projection,
                sourceRect: frameUV
            );
        }
        else
        {
            // Why: 如果纹理加载失败，降级为绿色方块（用于调试）。
            _quadRenderer!.SetProjectionMatrix(projection);
            _quadRenderer.DrawQuad(_playerPos, _playerSize, new Vector4(0.2f, 1.0f, 0.3f, 1.0f));
        }

        // Why: 渲染 HUD（最上层，Z-order = 999）。
        _hudRenderer?.Render(projection, _playerHP, _playerMaxHP, _playerSpirit, _playerMaxSpirit);
    }

    private static void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
        {
            Console.WriteLine("[Arcadia.Client] ESC pressed, closing window...");
            _window?.Close();
            return;
        }

        // Why: 记录按键状态（用于 WASD 移动）。
        if (key == Key.W || key == Key.A || key == Key.S || key == Key.D)
        {
            _pressedKeys.Add(key);
        }
    }

    private static void OnKeyUp(IKeyboard keyboard, Key key, int keyCode)
    {
        // Why: 移除按键状态。
        _pressedKeys.Remove(key);
    }

    private static void OnClosing()
    {
        Console.WriteLine("[Arcadia.Client] Cleaning up...");
        _networkClient?.Dispose();
        _hudRenderer?.Dispose();
        _backgroundRenderer?.Dispose();
        _spriteRenderer?.Dispose();
        _textureManager?.Dispose();
        _renderer?.Dispose();
        _inputContext?.Dispose();
    }

    private static void OnNetworkConnected()
    {
        Console.WriteLine("[Arcadia.Client] Network connected! Waiting for Welcome message...");
    }

    private static void OnNetworkMessage(ZoneWireMessageType type, JsonElement payload)
    {
        switch (type)
        {
            case ZoneWireMessageType.Welcome:
                // Why: 服务端发来 Welcome 消息，握手完成。
                Console.WriteLine("[Arcadia.Client] Received Welcome! Ready to play.");
                break;

            case ZoneWireMessageType.Snapshot:
                // Why: 服务端同步世界状态（玩家位置/HP/Spirit）。
                var snapshot = payload.Deserialize<ZoneSnapshot>();
                if (snapshot != null)
                {
                    // Why: 更新玩家位置。
                    _playerPos = new Vector2(snapshot.PlayerPos.X, snapshot.PlayerPos.Y);
                }
                break;

            case ZoneWireMessageType.Error:
                // Why: 服务端返回错误。
                Console.WriteLine($"[Arcadia.Client] Server error: {payload}");
                break;

            default:
                Console.WriteLine($"[Arcadia.Client] Unhandled message type: {type}");
                break;
        }
    }
}
