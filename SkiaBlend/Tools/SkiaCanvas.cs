﻿using System.Drawing;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaBlend.Helpers;
using SkiaSharp;

namespace SkiaBlend.Tools;

public class SkiaCanvas : Canvas
{
    private readonly uint _fbo;

    private GRBackendRenderTarget backendRenderTarget = null!;

    public SkiaCanvas(GL gl, Vector2D<uint> size, uint fbo) : base(gl)
    {
        _fbo = fbo;

        Context = GRContext.CreateGl(GRGlInterface.CreateGles(proc =>
        {
            if (gl.Context.TryGetProcAddress(proc, out nint addr))
            {
                return addr;
            }

            return 0;
        }));

        Resize(size);
    }

    public GRContext Context { get; }

    public SKSurface Surface { get; private set; } = null!;

    public override void Begin(Color clearColor)
    {
        if (!IsReady)
        {
            return;
        }

        _gl.BindFramebuffer(GLEnum.Framebuffer, _fbo);
        _gl.Viewport(0, 0, Width, Height);

        _gl.ClearColor(clearColor);
        _gl.Clear((uint)GLEnum.ColorBufferBit | (uint)GLEnum.DepthBufferBit | (uint)GLEnum.StencilBufferBit);

        Context.PurgeResources();
        Context.ResetContext();
    }

    public override void End()
    {
        if (!IsReady)
        {
            return;
        }

        Context.Flush();

        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

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
            Surface.Canvas.DrawSurface(skiaCanvas.Surface, new SKPoint(0, 0), PaintHelper.GetPaint());
        }
        else if (canvas is GLCanvas gLCanvas)
        {
            Surface.Canvas.DrawImage(gLCanvas.Image, new SKPoint(0, 0), PaintHelper.GetPaint());
        }

        Surface.Canvas.ResetMatrix();
    }

    public void Demo1()
    {
        if (!IsReady)
        {
            return;
        }

        SKPaint paint = PaintHelper.GetPaint();

        paint.Color = SKColors.Red;
        Surface.Canvas.DrawCircle(Width / 2, Height / 2, 100, paint);

        paint.Color = SKColors.Green;
        Surface.Canvas.DrawCircle((Width / 2) + 100, Height / 2, 100, paint);

        paint.Color = SKColors.Blue;
        Surface.Canvas.DrawCircle((Width / 2) + 200, Height / 2, 100, paint);
    }

    public void Demo2()
    {
        if (!IsReady)
        {
            return;
        }

        SKPaint paint = PaintHelper.GetPaint();
        paint.Color = SKColors.Red;
        paint.TextSize = 50;
        paint.TextAlign = SKTextAlign.Center;

        Surface.Canvas.DrawText("Hello World!", 150, 50, paint);
    }

    protected override bool Initialization()
    {
        _gl.GetInteger(GLEnum.Samples, out int samples);
        _gl.GetFramebufferAttachmentParameter(GLEnum.Framebuffer, GLEnum.Stencil, GLEnum.FramebufferAttachmentStencilSize, out int stencil_bits);

        int maxSamples = Context.GetMaxSurfaceSampleCount(_skFormatAndType);
        if (samples > maxSamples)
        {
            samples = maxSamples;
        }

        backendRenderTarget = new((int)Width, (int)Height, samples, stencil_bits, new GRGlFramebufferInfo(_fbo, _skFormatAndType.ToGlSizedFormat()));
        Surface = SKSurface.Create(Context, backendRenderTarget, GRSurfaceOrigin.BottomLeft, _skFormatAndType);

        return true;
    }

    protected override void Destroy()
    {
        Surface?.Dispose();
        backendRenderTarget?.Dispose();

        if (IsDisposed)
        {
            Context.Dispose();
        }
    }
}
