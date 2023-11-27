using SkiaSharp;
using System.Runtime.InteropServices;

namespace SkiaBlend.Tools;

public unsafe class SkiaFrame : Frame
{
    private static readonly SKPaint _demo1;
    private static readonly SKPaint _demo2;
    private static readonly SKPaint _drawFrame;

    private SKSurface? surface;
    private SKCanvas? canvas;

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

        _drawFrame = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High
        };
    }

    public SkiaFrame(int w, int h)
    {
        Resize(w, h);
    }

    public override void Resize(int w, int h)
    {
        Destroy();

        width = w;
        height = h;

        pixels = Marshal.AllocHGlobal(width * height * 4);

        surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul), pixels);
        canvas = surface.Canvas;

        isReady = true;
    }

    public override void DrawFrame(Frame frame, float ox, float oy, float sx, float sy)
    {
        if (!isReady || !frame.IsReady)
        {
            return;
        }

        SKBitmap bitmap = new(frame.Width, frame.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        bitmap.SetPixels(frame.Pixels);

        SKMatrix matrix = SKMatrix.CreateScale(1, -1);
        matrix.TransX = 0;
        matrix.TransY = bitmap.Height;

        matrix = matrix.PostConcat(SKMatrix.CreateScale(sx, sy));
        matrix = matrix.PostConcat(SKMatrix.CreateTranslation(ox, oy));

        canvas!.SetMatrix(matrix);

        canvas.DrawBitmap(bitmap, SKRect.Create(0, 0, frame.Width, frame.Height), _drawFrame);

        canvas.ResetMatrix();
    }

    public void Demo1()
    {
        if (!isReady)
        {
            return;
        }

        canvas!.Clear(SKColors.White);

        _demo1.Color = SKColors.Red;
        canvas.DrawCircle(width / 2, height / 2, 100, _demo1);

        _demo1.Color = SKColors.Green;
        canvas.DrawCircle(width / 2 + 100, height / 2, 100, _demo1);

        _demo1.Color = SKColors.Blue;
        canvas.DrawCircle(width / 2 + 200, height / 2, 100, _demo1);
    }

    public void Demo2()
    {
        if (!isReady)
        {
            return;
        }

        canvas!.DrawText("Hello World!", 150, 50, _demo2);
    }

    public override void Destroy()
    {
        canvas?.Dispose();
        surface?.Dispose();

        if (pixels != 0x00)
        {
            Marshal.FreeHGlobal(pixels);
        }
    }
}