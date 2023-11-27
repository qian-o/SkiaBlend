using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using SkiaBlend.Helpers;
using SkiaBlend.Shaders;
using SkiaBlend.Tools;
using Plane = SkiaBlend.Tools.Plane;

namespace SkiaBlend;

public unsafe class Game : IDisposable
{
    private readonly IWindow _window;

    private GL gl = null!;
    private IInputContext inputContext = null!;

    private SkiaFrame skiaFrame = null!;
    private Texture2D skiaTex = null!;

    private GLFrame demoFrame1 = null!;
    private GLFrame demoFrame2 = null!;
    private Camera demoCamera = null!;

    private ModelShader modelShader = null!;
    private Plane plane = null!;

    private Matrix4X4<float> orthographic = Matrix4X4<float>.Identity;

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
        gl = _window.CreateOpenGLES();
        inputContext = _window.CreateInput();
        demoCamera = new Camera
        {
            Position = new Vector3D<float>(0.0f, 2.0f, 8.0f),
            Fov = 45.0f
        };
        skiaFrame = new SkiaFrame(Width, Height);
        demoFrame1 = new GLFrame(gl, _window.Samples, 400, 400);
        demoFrame2 = new GLFrame(gl, _window.Samples, 400, 200);
        skiaTex = new Texture2D(gl);

        modelShader = new ModelShader(gl);
        plane = new Plane(gl);

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

        float aspectRatio = Width / (float)Height;
        plane.Model = Matrix4X4.CreateRotationX(MathHelper.DegreesToRadians(-90.0f)) * Matrix4X4.CreateScale(aspectRatio, 1.0f, 1.0f);

        orthographic = Matrix4X4.CreateOrthographic(1.0f * aspectRatio, 1.0f, -1.0f, 1.0f);
    }

    private void Window_Render(double obj)
    {
        DrawGL();
        DrawSkia();

        skiaTex.WriteImage((byte*)skiaFrame.Pixels, skiaFrame.Width, skiaFrame.Height);

        gl.Viewport(0, 0, (uint)Width, (uint)Height);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        gl.Clear((uint)GLEnum.ColorBufferBit | (uint)GLEnum.DepthBufferBit | (uint)GLEnum.StencilBufferBit);

        modelShader.Use();

        gl.SetUniform(modelShader.UniMVP, plane.Model * orthographic);
        gl.SetUniform(modelShader.UniTex, 0);

        gl.ActiveTexture(GLEnum.Texture0);
        gl.BindTexture(GLEnum.Texture2D, skiaTex.Id);

        plane.Draw(modelShader);

        modelShader.Unuse();
    }

    private void DrawGL()
    {
        demoFrame1.Demo(modelShader, demoCamera);
        demoFrame2.Demo(modelShader, demoCamera);
    }

    private void DrawSkia()
    {
        skiaFrame.Demo1();

        skiaFrame.DrawFrame(demoFrame1, 10.0f, 10.0f, 1.0f, 1.0f);
        skiaFrame.DrawFrame(demoFrame2, Width - demoFrame2.Width, Height - demoFrame2.Height, 1.0f, 1.0f);

        skiaFrame.Demo2();
    }

    public void Dispose()
    {
        skiaTex.Dispose();
        demoFrame1.Dispose();
        skiaFrame.Dispose();

        GC.SuppressFinalize(this);
    }
}
