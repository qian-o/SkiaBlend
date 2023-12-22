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
    private OxyPlotController oxyPlotController = null!;
    private PlotModel plotModel = null!;

    private GLCanvas subCanvas1 = null!;
    private GLCanvas subCanvas2 = null!;
    private Camera camera = null!;

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
        oxyPlotController = new OxyPlotController(mainCanvas, inputContext);
        plotModel = oxyPlotController.ActualModel;

        {
            Random random = new();

            string xKey = Guid.NewGuid().ToString();
            string yKey = Guid.NewGuid().ToString();

            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "X", Key = xKey });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Y", Key = yKey });

            LineSeries lineSeries = new()
            {
                Title = "LineSeries",
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.Green,
                MarkerStrokeThickness = 1.5,
                Color = OxyColors.SkyBlue,
                StrokeThickness = 1.5
            };

            ScatterSeries scatterSeries = new()
            {
                Title = "ScatterSeries",
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
                MarkerStrokeThickness = 1.5
            };

            for (int i = 0; i < 10000; i++)
            {
                lineSeries.Points.Add(new DataPoint(i, random.NextDouble()));
                scatterSeries.Points.Add(new ScatterPoint(i, random.NextDouble()));
            }

            plotModel.Series.Add(lineSeries);
            plotModel.Series.Add(scatterSeries);
        }

        subCanvas1 = new GLCanvas(gl, new Vector2D<uint>(600, 400), _window.Samples, mainCanvas);
        subCanvas2 = new GLCanvas(gl, new Vector2D<uint>(400, 200), _window.Samples, mainCanvas);

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

    private bool first = true;

    private void DrawSkia()
    {
        mainCanvas.Begin(Color.White);

        plotModel.InvalidatePlot(first);

        if (first)
        {
            first = false;
        }

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
}
