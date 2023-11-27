using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using SkiaBlend.Helpers;
using SkiaBlend.Shaders;
using SkiaBlend.Tools;
using System.Runtime.InteropServices;
using Plane = SkiaBlend.Tools.Plane;

namespace SkiaBlend;

public unsafe class Game : IDisposable
{
    private readonly IWindow _window;

    private GL gl = null!;
    private IInputContext inputContext = null!;
    private SkiaFrame skiaFrame = null!;
    private GLFrame glFrame = null!;
    private Texture2D skiaTex = null!;

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

    #region Speeds
    private readonly float cameraSpeed = 4.0f;
    private readonly float cameraSensitivity = 0.2f;
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
        _window.Resize += Window_Resize;
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
        glFrame = new GLFrame(gl, _window.Samples, 400, 400);
        skiaTex = new Texture2D(gl);

        modelShader = new ModelShader(gl);
        plane = new Plane(gl);

        mouse = inputContext.Mice[0];
        keyboard = inputContext.Keyboards[0];
    }

    private void Window_Resize(Vector2D<int> obj)
    {
        gl.Viewport(obj);

        skiaFrame.Resize(obj.X, obj.Y);
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

                demoCamera.Yaw += deltaX * cameraSensitivity;
                demoCamera.Pitch += -deltaY * cameraSensitivity;

                lastPos = vector;
            }
        }
        else
        {
            firstMove = true;
        }

        if (keyboard.IsKeyPressed(Key.W))
        {
            demoCamera.Position += demoCamera.Front * cameraSpeed * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.A))
        {
            demoCamera.Position -= demoCamera.Right * cameraSpeed * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.S))
        {
            demoCamera.Position -= demoCamera.Front * cameraSpeed * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.D))
        {
            demoCamera.Position += demoCamera.Right * cameraSpeed * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.Q))
        {
            demoCamera.Position -= demoCamera.Up * cameraSpeed * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.E))
        {
            demoCamera.Position += demoCamera.Up * cameraSpeed * (float)obj;
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

        skiaTex.WriteImage((byte*)skiaFrame.GetPixels(), skiaFrame.Width, skiaFrame.Height);

        gl.Clear((uint)GLEnum.ColorBufferBit | (uint)GLEnum.DepthBufferBit | (uint)GLEnum.StencilBufferBit);

        gl.UseProgram(modelShader.Id);
        gl.EnableVertexAttribArray(modelShader.InPos);
        gl.EnableVertexAttribArray(modelShader.InUV);

        gl.SetUniform(modelShader.UniMVP, plane.Model * orthographic);
        gl.SetUniform(modelShader.UniTex, 0);

        gl.ActiveTexture(GLEnum.Texture0);
        gl.BindTexture(GLEnum.Texture2D, skiaTex.Id);

        gl.BindBuffer(GLEnum.ArrayBuffer, plane.VBO);
        gl.VertexAttribPointer(modelShader.InPos, 3, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)Marshal.OffsetOf<Vertex>(nameof(Vertex.Position)));
        gl.VertexAttribPointer(modelShader.InUV, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoords)));
        gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        gl.BindBuffer(GLEnum.ElementArrayBuffer, plane.EBO);
        gl.DrawElements(GLEnum.Triangles, (uint)plane.Indices.Length, GLEnum.UnsignedInt, (void*)0);
        gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
    }

    private void DrawGL()
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, glFrame.Id);
        gl.Viewport(0, 0, (uint)glFrame.Width, (uint)glFrame.Height);
        demoCamera.Width = glFrame.Width;
        demoCamera.Height = glFrame.Height;

        glFrame.Demo(modelShader, demoCamera);

        demoCamera.Height = Height;
        demoCamera.Width = Width;
        gl.Viewport(0, 0, (uint)Width, (uint)Height);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void DrawSkia()
    {
        skiaFrame.Demo1();
        skiaFrame.DrawFrame(glFrame, 10.0f, 10.0f, 1.0f, 1.0f);
        skiaFrame.Demo2();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
