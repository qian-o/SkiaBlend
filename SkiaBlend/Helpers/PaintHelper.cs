using SkiaSharp;

namespace SkiaBlend.Helpers;

public static class PaintHelper
{
    private static readonly Dictionary<PaintParameter, SKPaint> _cache;

    static PaintHelper()
    {
        _cache = [];
    }

    public static SKPaint GetPaint(PaintParameter parameter)
    {
        if (!_cache.TryGetValue(parameter, out SKPaint? paint))
        {
            paint = CreateDefaultPaint();

            paint.Color = parameter.Color;
            paint.TextSize = parameter.TextSize;
            paint.TextAlign = parameter.TextAlign;
            paint.Style = parameter.Style;

            _cache.Add(parameter, paint);
        }

        return paint;
    }

    private static SKPaint CreateDefaultPaint()
    {
        return new SKPaint()
        {
            IsAntialias = true,
            IsDither = true,
            FilterQuality = SKFilterQuality.High
        };
    }
}

public class PaintParameter(SKColor color, float textSize, SKTextAlign textAlign, SKPaintStyle style)
{
    public SKColor Color { get; set; } = color;

    public float TextSize { get; set; } = textSize;

    public SKTextAlign TextAlign { get; set; } = textAlign;

    public SKPaintStyle Style { get; set; } = style;

    public override bool Equals(object? obj)
    {
        return obj is PaintParameter config &&
               Color.Equals(config.Color) &&
               TextSize == config.TextSize &&
               TextAlign == config.TextAlign &&
               Style == config.Style;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Color, TextSize, TextAlign, Style);
    }

    public static bool operator ==(PaintParameter? left, PaintParameter? right)
    {
        return EqualityComparer<PaintParameter?>.Default.Equals(left, right);
    }

    public static bool operator !=(PaintParameter? left, PaintParameter? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"Color: {Color}, TextSize: {TextSize}, TextAlign: {TextAlign}, Style: {Style}";
    }

    public static PaintParameter Default => new(SKColors.Black, 12, SKTextAlign.Left, SKPaintStyle.Fill);

    public static PaintParameter Red => new(SKColors.Red, 12, SKTextAlign.Left, SKPaintStyle.Fill);

    public static PaintParameter Green => new(SKColors.Green, 12, SKTextAlign.Left, SKPaintStyle.Fill);

    public static PaintParameter Blue => new(SKColors.Blue, 12, SKTextAlign.Left, SKPaintStyle.Fill);
}