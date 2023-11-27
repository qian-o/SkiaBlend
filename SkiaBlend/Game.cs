using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.ImGui;
using Silk.NET.Windowing;
using SkiaBlend.Shaders;
using SkiaBlend.Tools;
using System.Drawing;

namespace SkiaBlend;

public unsafe class Game : IDisposable
{
    private readonly IWindow _window;

    private GL gl = null!;
    private IInputContext inputContext = null!;
    private ImGuiController imGuiController = null!;

    private SkiaFrame skiaFrame = null!;

    private GLFrame demoFrame1 = null!;
    private GLFrame demoFrame2 = null!;
    private Camera demoCamera = null!;

    private ModelShader modelShader = null!;

    #region Input
    private IMouse mouse = null!;
    private IKeyboard keyboard = null!;
    private bool firstMove = true;
    private Vector2D<float> lastPos;
    #endregion

    public int Width => _window.Size.X;

    public int Height => _window.Size.Y;

    public Game()
    {
        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.API = new GraphicsAPI(ContextAPI.OpenGLES, new APIVersion(3, 2));
        windowOptions.Samples = 8;
        windowOptions.VSync = false;
        windowOptions.PreferredDepthBufferBits = 32;
        windowOptions.PreferredStencilBufferBits = 32;
        windowOptions.PreferredBitDepth = new Vector4D<int>(8);

        _window = Window.Create(windowOptions);
        _window.Load += Window_Load;
        _window.FramebufferResize += Window_Resize;
        _window.Update += Window_Update;
        _window.Render += Window_Render;
    }

    public void Run()
    {
        _window.Run();
    }

    private void Window_Load()
    {
        imGuiController = new ImGuiController(gl = _window.CreateOpenGLES(), _window, inputContext = _window.CreateInput());
        demoCamera = new Camera
        {
            Position = new Vector3D<float>(0.0f, 2.0f, 8.0f),
            Fov = 45.0f
        };
        skiaFrame = new SkiaFrame(gl, Width, Height);
        demoFrame1 = new GLFrame(gl, _window.Samples, 400, 400);
        demoFrame2 = new GLFrame(gl, _window.Samples, 400, 200);

        modelShader = new ModelShader(gl);

        mouse = inputContext.Mice[0];
        keyboard = inputContext.Keyboards[0];
    }

    private void Window_Resize(Vector2D<int> obj)
    {
        gl.Viewport(obj);

        if (obj.X > 0 && obj.Y > 0)
        {
            skiaFrame.Resize(obj.X, obj.Y);
        }
    }

    private void Window_Update(double obj)
    {
        if (mouse.IsButtonPressed(MouseButton.Middle))
        {
            Vector2D<float> vector = new(mouse.Position.X, mouse.Position.Y);

            if (firstMove)
            {
                lastPos = vector;

                firstMove = false;
            }
            else
            {
                float deltaX = vector.X - lastPos.X;
                float deltaY = vector.Y - lastPos.Y;

                demoCamera.Yaw += deltaX * 0.2f;
                demoCamera.Pitch += -deltaY * 0.2f;

                lastPos = vector;
            }
        }
        else
        {
            firstMove = true;
        }

        if (keyboard.IsKeyPressed(Key.W))
        {
            demoCamera.Position += demoCamera.Front * 4.0f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.A))
        {
            demoCamera.Position -= demoCamera.Right * 4.0f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.S))
        {
            demoCamera.Position -= demoCamera.Front * 4.0f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.D))
        {
            demoCamera.Position += demoCamera.Right * 4.0f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.Q))
        {
            demoCamera.Position -= demoCamera.Up * 4.0f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.E))
        {
            demoCamera.Position += demoCamera.Up * 4.0f * (float)obj;
        }

        demoCamera.Width = Width;
        demoCamera.Height = Height;
    }

    private void Window_Render(double obj)
    {
        DrawGL();
        DrawSkia();

        imGuiController.Update((float)obj);

        ImGui.Begin("Info");
        ImGui.Value("FPS", ImGui.GetIO().Framerate);
        ImGui.End();

        imGuiController.Render();
    }

    private void DrawGL()
    {
        // 此处不使用Begin、End包裹，因为GLFrame中直接使用了GL的方法。
        demoFrame1.Demo(modelShader, demoCamera);
        demoFrame2.Demo(modelShader, demoCamera);
    }

    private void DrawSkia()
    {
        // 使用Begin、End包裹，可以将将Skia内容提交到GL中进行绘制。
        skiaFrame.Begin(Color.White);

        skiaFrame.Demo1();

        skiaFrame.DrawFrame(demoFrame1, 10.0f, 10.0f, 1.0f, 1.0f);
        skiaFrame.DrawFrame(demoFrame2, Width - demoFrame2.Width, Height - demoFrame2.Height, 1.0f, 1.0f);

        skiaFrame.Demo2();

        skiaFrame.End();
    }

    public void Dispose()
    {
        demoFrame1.Dispose();
        demoFrame2.Dispose();
        skiaFrame.Dispose();

        GC.SuppressFinalize(this);
    }
}
