using OxyPlot;
using OxyPlot.SkiaSharp;
using Silk.NET.Input;
using Silk.NET.Maths;
using System.Numerics;
using CursorType = OxyPlot.CursorType;

namespace SkiaBlend.Tools;

public class PlotView : IPlotView
{
    private readonly IMouse _mouse;
    private readonly SkiaRenderContext _renderContext;
    private readonly IPlotController _plotController;

    public PlotView(IInputContext inputContext)
    {
        _mouse = inputContext.Mice[0];
        _renderContext = new SkiaRenderContext();
        _plotController = new PlotController();

        _mouse.MouseDown += Mouse_MouseDown;
        _mouse.MouseMove += Mouse_MouseMove;
        _mouse.MouseUp += Mouse_MouseUp;
        _mouse.Scroll += Mouse_Scroll;
    }

    private PlotModel actualModel = null!;

    public PlotModel ActualModel
    {
        get => actualModel;
        set
        {
            if (actualModel != value)
            {
                if (actualModel != null)
                {
                    ((IPlotModel)actualModel).AttachPlotView(null);
                }

                actualModel = value;

                if (actualModel != null)
                {
                    ((IPlotModel)actualModel).AttachPlotView(this);
                }
            }
        }
    }

    public IController ActualController => _plotController;

    public OxyRect ClientArea { get; set; }

    Model IView.ActualModel => actualModel;

    public void HideTracker()
    {

    }

    public void HideZoomRectangle()
    {
    }

    public void InvalidatePlot(bool updateData = true)
    {
        ((IPlotModel)ActualModel).Update(updateData);
    }

    public void SetClipboardText(string text)
    {
    }

    public void SetCursorType(CursorType cursorType)
    {
    }

    public void ShowTracker(TrackerHitResult trackerHitResult)
    {
    }

    public void ShowZoomRectangle(OxyRect rectangle)
    {
    }

    public void Render(SkiaCanvas skiaCanvas, Vector2D<float> offset, Vector2D<float> size)
    {
        ClientArea = new OxyRect(offset.X, offset.Y, size.X, size.Y);

        _renderContext.SkCanvas = skiaCanvas.Surface.Canvas;

        ((IPlotModel)ActualModel).Render(_renderContext, ClientArea);
    }

    private void Mouse_MouseDown(IMouse mouse, MouseButton button)
    {
        if (button != MouseButton.Middle)
        {
            OxyMouseButton oxyMouseButton = button switch
            {
                MouseButton.Left => OxyMouseButton.Left,
                MouseButton.Right => OxyMouseButton.Right,
                MouseButton.Button4 => OxyMouseButton.XButton1,
                MouseButton.Button5 => OxyMouseButton.XButton2,
                _ => OxyMouseButton.None,
            };

            ActualController.HandleMouseDown(this, new OxyMouseDownEventArgs
            {
                ChangedButton = oxyMouseButton,
                Position = new ScreenPoint(mouse.Position.X, mouse.Position.Y),
                ClickCount = 1
            });
        }
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
        if (button != MouseButton.Middle)
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
