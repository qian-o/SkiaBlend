using Silk.NET.OpenGLES;
using SkiaSharp;
using System.Drawing;

namespace SkiaBlend.Tools;

public abstract unsafe class Frame : IDisposable
{
    protected readonly GL _gl;
    protected readonly GRContext _grContext;
    protected readonly SKColorType _colorType;

    protected int width;
    protected int height;
    protected bool isReady;
    protected GRBackendRenderTarget renderTarget = null!;
    protected SKSurface surface = null!;

    public int Width => width;

    public int Height => height;

    public bool IsReady => isReady;

    public GRBackendRenderTarget RenderTarget => renderTarget;

    public SKSurface Surface => surface;

    protected Frame(GL gl, int w, int h)
    {
        _gl = gl;
        _grContext = GRContext.CreateGl(GRGlInterface.Create());
        _colorType = SKColorType.Rgba8888;

        Resize(w, h);
    }

    public void Begin()
    {
        Begin(Color.Black);
    }

    public void Begin(Color clearColor)
    {
        if (!isReady)
        {
            return;
        }

        _gl.BindFramebuffer(GLEnum.Framebuffer, GetFramebuffer());
        _gl.Viewport(0, 0, (uint)width, (uint)height);
        _gl.ClearColor(clearColor);
        _gl.Clear((uint)GLEnum.ColorBufferBit | (uint)GLEnum.DepthBufferBit | (uint)GLEnum.StencilBufferBit);
    }

    public void End()
    {
        if (!isReady)
        {
            return;
        }

        _grContext.Flush();
        _grContext.ResetContext();

        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

    /// <summary>
    /// 重置大小。
    /// </summary>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    public virtual void Resize(int w, int h)
    {
        if (width == w && height == h)
        {
            return;
        }

        Dispose();

        width = w;
        height = h;

        _gl.GetInteger(GLEnum.Stencil, out int stencil);
        _gl.GetInteger(GLEnum.Samples, out int samples);

        int maxSamples = _grContext.GetMaxSurfaceSampleCount(_colorType);
        if (samples > maxSamples)
        {
            samples = maxSamples;
        }
        renderTarget = new GRBackendRenderTarget(width, height, samples, stencil, new GRGlFramebufferInfo(GetFramebuffer(), _colorType.ToGlSizedFormat()));
        surface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, _colorType);

        isReady = true;
    }

    /// <summary>
    /// 绘制帧。
    /// </summary>
    /// <param name="frame">frame</param>
    /// <param name="ox">ox</param>
    /// <param name="oy">oy</param>
    /// <param name="sx">sx</param>
    /// <param name="sy">sy</param>
    public void DrawFrame(Frame frame, float ox, float oy, float sx, float sy)
    {
        if (!isReady || !frame.IsReady)
        {
            return;
        }

        SKMatrix matrix = SKMatrix.CreateIdentity();
        matrix = matrix.PostConcat(SKMatrix.CreateScale(sx, sy));
        matrix = matrix.PostConcat(SKMatrix.CreateTranslation(ox, oy));

        surface.Canvas.SetMatrix(matrix);

        if (frame is GLFrame gLFrame)
        {
            surface.Canvas.DrawImage(gLFrame.SKImage, new SKPoint(0, 0));
        }
        else
        {
            surface.Canvas.DrawSurface(frame.Surface, new SKPoint(0, 0));
        }

        surface.Canvas.ResetMatrix();
    }

    /// <summary>
    /// 获取帧缓冲。
    /// </summary>
    /// <returns></returns>
    protected abstract uint GetFramebuffer();

    public virtual void Destroy()
    {
        if (surface != null)
        {
            surface.Dispose();
            surface = null!;
        }

        if (renderTarget != null)
        {
            renderTarget.Dispose();
            renderTarget = null!;
        }

        isReady = false;
    }

    public void Dispose()
    {
        Destroy();

        GC.SuppressFinalize(this);
    }
}
