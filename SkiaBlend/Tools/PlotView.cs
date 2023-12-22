using OxyPlot;
using OxyPlot.SkiaSharp;
using Silk.NET.Input;
using Silk.NET.Maths;
using CursorType = OxyPlot.CursorType;

namespace SkiaBlend.Tools;

public class PlotView : IPlotView
{
    private readonly IMouse _mouse;
    private readonly IKeyboard _keyboard;
    private readonly SkiaRenderContext _renderContext;
    private readonly IPlotController _plotController;
    private readonly PlotModel _model;

    public PlotView(IInputContext inputContext)
    {
        _mouse = inputContext.Mice[0];
        _keyboard = inputContext.Keyboards[0];
        _renderContext = new SkiaRenderContext();
        _plotController = new PlotController();
        _model = new PlotModel();
        ((IPlotModel)_model).AttachPlotView(this);
    }

    public PlotModel ActualModel => _model;

    public IController ActualController => _plotController;

    public OxyRect ClientArea { get; set; }

    Model IView.ActualModel => _model;

    public void HideTracker()
    {
        throw new NotImplementedException();
    }

    public void HideZoomRectangle()
    {
        throw new NotImplementedException();
    }

    public void InvalidatePlot(bool updateData = true)
    {
        ((IPlotModel)ActualModel).Update(updateData);
    }

    public void SetClipboardText(string text)
    {
        throw new NotImplementedException();
    }

    public void SetCursorType(CursorType cursorType)
    {
        throw new NotImplementedException();
    }

    public void ShowTracker(TrackerHitResult trackerHitResult)
    {
        throw new NotImplementedException();
    }

    public void ShowZoomRectangle(OxyRect rectangle)
    {
        throw new NotImplementedException();
    }

    public void Render(SkiaCanvas skiaCanvas, Vector2D<float> offset, Vector2D<float> size)
    {
        ClientArea = new OxyRect(offset.X, offset.Y, size.X, size.Y);

        _renderContext.SkCanvas = skiaCanvas.Surface.Canvas;

        ((IPlotModel)ActualModel).Render(_renderContext, ClientArea);
    }
}
