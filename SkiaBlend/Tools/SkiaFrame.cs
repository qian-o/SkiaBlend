using Silk.NET.OpenGLES;
using SkiaSharp;

namespace SkiaBlend.Tools;

public unsafe class SkiaFrame(GL gl, int w, int h) : Frame(gl, w, h)
{
    private static readonly SKPaint _demo1;
    private static readonly SKPaint _demo2;

    static SkiaFrame()
    {
        _demo1 = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            Color = SKColors.Red
        };

        _demo2 = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            Color = SKColors.Red,
            TextAlign = SKTextAlign.Center,
            TextSize = 50
        };
    }

    public void Demo1()
    {
        if (!isReady)
        {
            return;
        }

        _demo1.Color = SKColors.Red;
        surface.Canvas.DrawCircle(width / 2, height / 2, 100, _demo1);

        _demo1.Color = SKColors.Green;
        surface.Canvas.DrawCircle(width / 2 + 100, height / 2, 100, _demo1);

        _demo1.Color = SKColors.Blue;
        surface.Canvas.DrawCircle(width / 2 + 200, height / 2, 100, _demo1);
    }

    public void Demo2()
    {
        if (!isReady)
        {
            return;
        }

        surface.Canvas.DrawText("Hello World!", 150, 50, _demo2);
    }

    public override void Destroy()
    {
        surface?.Dispose();
        renderTarget?.Dispose();
    }

    protected override uint GetFramebuffer() => 0;
}