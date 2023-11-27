using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaBlend.Tools;
using SkiaSharp;
using System.Drawing;

namespace SkiaBlend.Helpers;

public static unsafe class TextureExtensions
{
    public static void WriteImage(this Texture2D texture, byte* image, int width, int height)
    {
        texture.FlushTexture(image, new Vector2D<uint>((uint)width, (uint)height), GLEnum.Rgba, GLEnum.UnsignedByte);
    }

    public static void WriteLinearColor(this Texture2D texture, Color[] colors, PointF begin, PointF end)
    {
        byte[] bytes = new byte[1024 * 1024 * 4];

        fixed (byte* ptr = bytes)
        {
            using SKSurface surface = SKSurface.Create(new SKImageInfo(1024, 1024, SKColorType.Rgba8888), (nint)ptr);

            using SKPaint paint = new()
            {
                IsAntialias = true,
                IsDither = true,
                FilterQuality = SKFilterQuality.High,
                Shader = SKShader.CreateLinearGradient(new SKPoint(begin.X * 1024, begin.Y * 1024), new SKPoint(end.X * 1024, end.Y * 1024), colors.Select(c => new SKColor(c.R, c.G, c.B, c.A)).ToArray(), null, SKShaderTileMode.Repeat)
            };
            surface.Canvas.DrawRect(0, 0, 1024, 1024, paint);

            texture.FlushTexture(ptr, new Vector2D<uint>(1024, 1024), GLEnum.Rgba, GLEnum.UnsignedByte);
        }
    }
}
