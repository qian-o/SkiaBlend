using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaBlend.Helpers;
using SkiaSharp;
using System.Drawing;

namespace SkiaBlend.Tools;

public class SkiaCanvas : Canvas
{
    private readonly uint _fbo;

    private GRBackendRenderTarget renderTarget = null!;

    public SkiaCanvas(GL gl, Vector2D<int> size, uint fbo) : base(gl)
    {
        _fbo = fbo;

        Context = GRContext.CreateGl(GRGlInterface.Create());

        Resize(size);
    }

    public GRContext Context { get; }

    public SKSurface Surface { get; private set; } = null!;

    public override void DrawCanvas(Canvas canvas, Vector2D<float> offset, Vector2D<float> scale)
    {
        if (!IsReady || !canvas.IsReady)
        {
            return;
        }

        SKMatrix matrix = SKMatrix.CreateIdentity();
        matrix = matrix.PostConcat(SKMatrix.CreateScale(scale.X, scale.Y));
        matrix = matrix.PostConcat(SKMatrix.CreateTranslation(offset.X, offset.Y));

        Surface.Canvas.SetMatrix(matrix);

        if (canvas is SkiaCanvas skiaCanvas)
        {
            Surface.Canvas.DrawSurface(skiaCanvas.Surface, new SKPoint(0, 0), PaintHelper.GetPaint(PaintParameter.Default));
        }
        else if (canvas is GLCanvas gLCanvas)
        {
            Surface.Canvas.DrawImage(gLCanvas.SKImage, new SKPoint(0, 0), PaintHelper.GetPaint(PaintParameter.Default));
        }

        Surface.Canvas.ResetMatrix();
    }

    public void Begin()
    {
        Begin(Color.White);
    }

    public void Begin(Color clearColor)
    {
        if (!IsReady)
        {
            return;
        }

        _gl.BindFramebuffer(GLEnum.Framebuffer, _fbo);
        _gl.Viewport(0, 0, (uint)Width, (uint)Height);

        _gl.ClearColor(clearColor);
        _gl.Clear((uint)GLEnum.ColorBufferBit | (uint)GLEnum.DepthBufferBit | (uint)GLEnum.StencilBufferBit);
    }

    public void End()
    {
        if (!IsReady)
        {
            return;
        }

        Context.Flush();
        Context.ResetContext();

        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

    public void Demo1()
    {
        if (!IsReady)
        {
            return;
        }

        Surface.Canvas.DrawCircle(Width / 2, Height / 2, 100, PaintHelper.GetPaint(PaintParameter.Red));
        Surface.Canvas.DrawCircle(Width / 2 + 100, Height / 2, 100, PaintHelper.GetPaint(PaintParameter.Green));
        Surface.Canvas.DrawCircle(Width / 2 + 200, Height / 2, 100, PaintHelper.GetPaint(PaintParameter.Blue));
    }

    public void Demo2()
    {
        if (!IsReady)
        {
            return;
        }

        PaintParameter paintParameter = new(SKColors.Red, 50, SKTextAlign.Center, SKPaintStyle.Fill);

        Surface.Canvas.DrawText("Hello World!", 150, 50, PaintHelper.GetPaint(paintParameter));
    }

    protected override bool Initialization()
    {
        _gl.GetInteger(GLEnum.Stencil, out int stencil);
        _gl.GetInteger(GLEnum.Samples, out int samples);

        int maxSamples = Context.GetMaxSurfaceSampleCount(_skColorAndType);
        if (samples > maxSamples)
        {
            samples = maxSamples;
        }
        renderTarget = new GRBackendRenderTarget(Width, Height, samples, stencil, new GRGlFramebufferInfo(_fbo, _skColorAndType.ToGlSizedFormat()));
        Surface = SKSurface.Create(Context, renderTarget, GRSurfaceOrigin.BottomLeft, _skColorAndType);

        return true;
    }

    protected override void Destroy()
    {
        Surface?.Dispose();
        renderTarget?.Dispose();

        if (IsDisposed)
        {
            Context.Dispose();
        }
    }
}
