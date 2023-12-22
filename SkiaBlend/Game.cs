using ImGuiNET;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.ImGui;
using Silk.NET.Windowing;
using SkiaBlend.Tools;
using System.Drawing;

namespace SkiaBlend;

public unsafe class Game : IDisposable
{
    private readonly IWindow _window;

    private GL gl = null!;
    private IInputContext inputContext = null!;
    private ImGuiController imGuiController = null!;

    private SkiaCanvas mainCanvas = null!;

    private GLCanvas subCanvas1 = null!;
    private GLCanvas subCanvas2 = null!;
    private Camera camera = null!;

    private PlotView plotView = null!;

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
        windowOptions.PreferredStencilBufferBits = 8;
        windowOptions.PreferredBitDepth = new Vector4D<int>(8);

        _window = Window.Create(windowOptions);
        _window.Load += Window_Load;
        _window.FramebufferResize += Window_FramebufferResize;
        _window.Update += Window_Update;
        _window.Render += Window_Render;
        _window.Closing += Window_Closing;
    }

    public void Run()
    {
        _window.Run();
    }

    private void Window_Load()
    {
        imGuiController = new ImGuiController(gl = _window.CreateOpenGLES(), _window, inputContext = _window.CreateInput());
        camera = new Camera
        {
            Position = new Vector3D<float>(0.0f, 2.0f, 8.0f),
            Fov = 45.0f
        };
        mainCanvas = new SkiaCanvas(gl, new Vector2D<uint>((uint)Width, (uint)Height), 0);

        subCanvas1 = new GLCanvas(gl, new Vector2D<uint>(600, 400), _window.Samples, mainCanvas);
        subCanvas2 = new GLCanvas(gl, new Vector2D<uint>(400, 200), _window.Samples, mainCanvas);

        plotView = new PlotView(inputContext);
        {
            plotView.ActualModel = ColorMapHot16Big();

            plotView.ActualModel.InvalidatePlot(true);
        }

        mouse = inputContext.Mice[0];
        keyboard = inputContext.Keyboards[0];
    }

    private void Window_FramebufferResize(Vector2D<int> obj)
    {
        gl.Viewport(obj);
        mainCanvas.Resize(new Vector2D<uint>((uint)obj.X, (uint)obj.Y));
        subCanvas1.Resize(new Vector2D<uint>(Convert.ToUInt32(obj.X * 0.5), Convert.ToUInt32(obj.Y * 0.5)));
        subCanvas2.Resize(new Vector2D<uint>(Convert.ToUInt32(obj.X * 0.3), Convert.ToUInt32(obj.Y * 0.3)));

        _window.DoRender();
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

                camera.Yaw += deltaX * 0.2f;
                camera.Pitch += -deltaY * 0.2f;

                lastPos = vector;
            }
        }
        else
        {
            firstMove = true;
        }

        if (keyboard.IsKeyPressed(Key.W))
        {
            camera.Position += camera.Front * 4.0f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.A))
        {
            camera.Position -= camera.Right * 4.0f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.S))
        {
            camera.Position -= camera.Front * 4.0f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.D))
        {
            camera.Position += camera.Right * 4.0f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.Q))
        {
            camera.Position -= camera.Up * 4.0f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.E))
        {
            camera.Position += camera.Up * 4.0f * (float)obj;
        }

        camera.Width = Width;
        camera.Height = Height;
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

    private void Window_Closing()
    {
        subCanvas2.Dispose();
        subCanvas1.Dispose();
        mainCanvas.Dispose();
    }

    private void DrawGL()
    {
        subCanvas1.Begin(Color.FromArgb(127, 0, 0, 127));
        subCanvas1.Demo(camera);
        subCanvas1.End();

        subCanvas2.Begin(Color.FromArgb(127, 0, 127, 127));
        subCanvas2.Demo(camera);
        subCanvas2.End();
    }

    private void DrawSkia()
    {
        mainCanvas.Begin(Color.White);

        plotView.Render(mainCanvas, new Vector2D<float>(0.0f, 0.0f), new Vector2D<float>(Width, Height));

        mainCanvas.Demo1();
        mainCanvas.DrawCanvas(subCanvas1, new Vector2D<float>(10.0f, 10.0f), Vector2D<float>.One);
        mainCanvas.DrawCanvas(subCanvas2, new Vector2D<float>(Width - subCanvas2.Width - 10, Height - subCanvas2.Height - 10), Vector2D<float>.One);
        mainCanvas.Demo2();

        mainCanvas.End();
    }

    public void Dispose()
    {
        imGuiController.Dispose();
        inputContext.Dispose();
        gl.Dispose();

        _window.Dispose();

        GC.SuppressFinalize(this);
    }

    public static PlotModel ColorMapHot16Big()
    {
        return CreateRandomScatterSeriesWithColorAxisPlotModel(30000, OxyPalettes.Hot(16), MarkerType.Square, AxisPosition.Right, OxyColors.Undefined, OxyColors.Undefined);
    }

    private static PlotModel CreateRandomScatterSeriesWithColorAxisPlotModel(int n, OxyPalette palette, MarkerType markerType, AxisPosition colorAxisPosition, OxyColor highColor, OxyColor lowColor)
    {
        PlotModel model = new() { Title = string.Format("ScatterSeries (n={0})", n), Background = OxyColors.LightGray };
        LinearColorAxis colorAxis = new() { Position = colorAxisPosition, Palette = palette, Minimum = -1, Maximum = 1, HighColor = highColor, LowColor = lowColor };
        model.Axes.Add(colorAxis);
        model.Series.Add(CreateRandomScatterSeries(n, markerType, false, true, colorAxis));
        return model;
    }

    private static ScatterSeries CreateRandomScatterSeries(int n, MarkerType markerType, bool setSize, bool setValue, LinearColorAxis colorAxis)
    {
        ScatterSeries s1 = new()
        {
            MarkerType = markerType,
            MarkerSize = 6,
            ColorAxisKey = colorAxis?.Key
        };
        Random random = new(13);
        for (int i = 0; i < n; i++)
        {
            ScatterPoint p = new((random.NextDouble() * 2.2) - 1.1, random.NextDouble());
            if (setSize)
            {
                p.Size = (random.NextDouble() * 5) + 5;
            }

            if (setValue)
            {
                p.Value = (random.NextDouble() * 2.2) - 1.1;
            }

            s1.Points.Add(p);
        }

        return s1;
    }
}
