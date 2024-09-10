using System.Drawing;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaSharp;

namespace SkiaBlend.Tools;

public abstract class Canvas : IDisposable
{
    protected readonly GL _gl;
    protected readonly GLEnum _glFormat;
    protected readonly GLEnum _glType;
    protected readonly GLEnum _glFormatAndType;
    protected readonly SKColorType _skFormatAndType;

    protected Canvas(GL gl)
    {
        _gl = gl;
        _glFormat = GLEnum.Rgba;
        _glType = GLEnum.UnsignedByte;
        _glFormatAndType = GLEnum.Rgba8;
        _skFormatAndType = SKColorType.Rgba8888;
    }

    public uint Width { get; private set; }

    public uint Height { get; private set; }

    public bool IsReady { get; private set; }

    public bool IsDisposed { get; private set; }

    public void Resize(Vector2D<uint> size)
    {
        if (size.X == 0 || size.Y == 0)
        {
            return;
        }

        if (Width == size.X && Height == size.Y)
        {
            return;
        }

        Destroy();

        Width = size.X;
        Height = size.Y;
        IsReady = Initialization();
    }

    public void Begin() => Begin(Color.White);

    public abstract void Begin(Color clearColor);

    public abstract void End();

    public abstract void DrawCanvas(Canvas canvas, Vector2D<float> offset, Vector2D<float> scale);

    protected abstract bool Initialization();

    protected abstract void Destroy();

    public void Dispose()
    {
        IsDisposed = true;

        Destroy();

        GC.SuppressFinalize(this);
    }
}
