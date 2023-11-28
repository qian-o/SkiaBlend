using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaSharp;

namespace SkiaBlend.Tools;

public abstract class Canvas : IDisposable
{
    protected readonly GL _gl;
    protected readonly GLEnum _glColor;
    protected readonly GLEnum _glColorType;
    protected readonly SKColorType _skColorAndType;

    protected Canvas(GL gl)
    {
        _gl = gl;
        _glColor = GLEnum.Rgba;
        _glColorType = GLEnum.UnsignedByte;
        _skColorAndType = SKColorType.Rgba8888;
    }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public bool IsReady { get; private set; }

    public bool IsDisposed { get; private set; }

    public void Resize(Vector2D<int> size)
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
