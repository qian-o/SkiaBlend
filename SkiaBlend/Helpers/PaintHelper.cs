using SkiaSharp;

namespace SkiaBlend.Helpers;

public static class PaintHelper
{
    private static SKPaint paint = null!;

    public static SKPaint GetPaint()
    {
        ResetPaint();

        return paint;
    }

    private static void ResetPaint()
    {
        if (paint == null || paint.Handle == 0x00)
        {
            paint = new SKPaint();
        }

        paint.Reset();
        paint.IsAntialias = true;
        paint.IsDither = true;
        paint.HintingLevel = SKPaintHinting.Full;
        paint.FilterQuality = SKFilterQuality.High;
    }
}