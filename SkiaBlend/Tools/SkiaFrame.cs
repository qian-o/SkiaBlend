using SkiaSharp;
using System.Runtime.InteropServices;

namespace SkiaBlend.Tools;

public unsafe class SkiaFrame : Frame
{
    private nint pixels;
    private SKSurface? surface;
    private SKCanvas? canvas;

    private static SKPaint demo1;
    private static SKPaint demo2;
    private static SKPaint drawFrame;

    static SkiaFrame()
    {
        demo1 = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            Color = SKColors.Red
        };

        demo2 = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            Color = SKColors.Red,
            TextAlign = SKTextAlign.Center,
            TextSize = 50
        };

        drawFrame = new SKPaint
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
        bitmap.SetPixels(frame.GetPixels());

        SKMatrix matrix = SKMatrix.CreateScale(1, -1);
        matrix.TransX = 0;
        matrix.TransY = bitmap.Height;

        matrix = matrix.PostConcat(SKMatrix.CreateScale(sx, sy));
        matrix = matrix.PostConcat(SKMatrix.CreateTranslation(ox, oy));

        canvas!.SetMatrix(matrix);

        canvas.DrawBitmap(bitmap, SKRect.Create(0, 0, frame.Width, frame.Height), drawFrame);

        canvas.ResetMatrix();
    }

    public override nint GetPixels()
    {
        return pixels;
    }

    public void Demo1()
    {
        if (!isReady)
        {
            return;
        }

        canvas!.Clear(SKColors.White);

        demo1.Color = SKColors.Red;
        canvas.DrawCircle(width / 2, height / 2, 100, demo1);

        demo1.Color = SKColors.Green;
        canvas.DrawCircle(width / 2 + 100, height / 2, 100, demo1);

        demo1.Color = SKColors.Blue;
        canvas.DrawCircle(width / 2 + 200, height / 2, 100, demo1);
    }

    public void Demo2()
    {
        if (!isReady)
        {
            return;
        }

        canvas!.DrawText("Hello World!", 150, 50, demo2);
    }

    public void Save()
    {
        if (!isReady)
        {
            return;
        }

        SKImage image = SKImage.FromPixelCopy(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul), pixels);
        SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

        File.WriteAllBytes("skia.png", data.ToArray());
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