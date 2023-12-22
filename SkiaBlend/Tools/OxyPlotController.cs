using OxyPlot;
using OxyPlot.SkiaSharp;
using Silk.NET.Input;
using SkiaSharp;
using System.Numerics;

namespace SkiaBlend.Tools;

public class OxyPlotController : IPlotView
{
    private readonly SkiaCanvas _skiaCanvas;
    private readonly IMouse _mouse;
    private readonly IKeyboard _keyboard;
    private readonly SkiaRenderContext _renderContext;
    private readonly IPlotController _plotController;
    private readonly PlotModel _model;

    public OxyPlotController(SkiaCanvas skiaCanvas, IInputContext input)
    {
        _skiaCanvas = skiaCanvas;
        _mouse = input.Mice[0];
        _keyboard = input.Keyboards[0];
        _renderContext = new SkiaRenderContext();
        _plotController = new PlotController();
        _model = new PlotModel();
        ((IPlotModel)_model).AttachPlotView(this);

        _mouse.MouseDown += Mouse_MouseDown;
        _mouse.MouseMove += Mouse_MouseMove;
        _mouse.MouseUp += Mouse_MouseUp;
        _mouse.Scroll += Mouse_Scroll;
    }

    public PlotModel ActualModel => _model;

    public IController ActualController => _plotController;

    public OxyRect ClientArea => new(0, 0, _skiaCanvas.Width, _skiaCanvas.Height);

    Model IView.ActualModel => _model;

    public void HideTracker()
    {
    }

    public void HideZoomRectangle()
    {
    }

    public void InvalidatePlot(bool updateData = true)
    {
        _renderContext.SkCanvas = _skiaCanvas.Surface.Canvas;

        ((IPlotModel)ActualModel).Update(updateData);
        ((IPlotModel)ActualModel).Render(_renderContext, ClientArea);
    }

    public void SetClipboardText(string text)
    {
    }

    public void SetCursorType(OxyPlot.CursorType cursorType)
    {
    }

    public void ShowTracker(TrackerHitResult trackerHitResult)
    {
    }

    public void ShowZoomRectangle(OxyRect rectangle)
    {
    }

    private void Mouse_MouseDown(IMouse mouse, MouseButton button)
    {
        OxyMouseButton oxyMouseButton = button switch
        {
            MouseButton.Left => OxyMouseButton.Left,
            MouseButton.Right => OxyMouseButton.Right,
            MouseButton.Middle => OxyMouseButton.Middle,
            MouseButton.Button4 => OxyMouseButton.XButton1,
            MouseButton.Button5 => OxyMouseButton.XButton2,
            _ => OxyMouseButton.None,
        };

        ActualController.HandleMouseDown(this, new OxyMouseDownEventArgs
        {
            ChangedButton = oxyMouseButton,
            Position = new ScreenPoint(mouse.Position.X, mouse.Position.Y),
            ClickCount = 1,
            ModifierKeys = OxyModifierKeys.None
        });
    }

    private void Mouse_MouseMove(IMouse mouse, Vector2 vector)
    {
        ActualController.HandleMouseMove(this, new OxyMouseEventArgs
        {
            Position = new ScreenPoint(mouse.Position.X, mouse.Position.Y),
            ModifierKeys = OxyModifierKeys.None
        });
    }

    private void Mouse_MouseUp(IMouse mouse, MouseButton button)
    {
        if (button == MouseButton.Left)
        {
            ActualController.HandleMouseUp(this, new OxyMouseEventArgs
            {
                Position = new ScreenPoint(mouse.Position.X, mouse.Position.Y),
                ModifierKeys = OxyModifierKeys.None
            });
        }
    }

    private void Mouse_Scroll(IMouse mouse, ScrollWheel wheel)
    {
        ActualController.HandleMouseWheel(this, new OxyMouseWheelEventArgs
        {
            Position = new ScreenPoint(mouse.Position.X, mouse.Position.Y),
            ModifierKeys = OxyModifierKeys.None,
            Delta = wheel.Y < 0 ? 120 : -120
        });
    }
}
