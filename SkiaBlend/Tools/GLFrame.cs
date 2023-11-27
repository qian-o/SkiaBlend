using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.EXT;
using SkiaBlend.Helpers;
using SkiaBlend.Shaders;
using SkiaSharp;
using System.Drawing;

namespace SkiaBlend.Tools;

public unsafe class GLFrame : Frame
{
    private readonly ExtMultisampledRenderToTexture _extMRT;
    private readonly uint _samples;

    private readonly Plane demoPlane = null!;
    private readonly Texture2D demoTex = null!;

    private SKImage sKImage = null!;

    public uint Id { get; }

    public uint ColorBuffer { get; }

    public uint DepthRenderBuffer { get; }

    public SKImage SKImage => sKImage;

    public GLFrame(GL gl, int? samples, int w, int h) : base(gl, w, h)
    {
        _gl.TryGetExtension(out _extMRT);
        _samples = samples != null ? (uint)samples : 1;

        Id = _gl.GenFramebuffer();
        ColorBuffer = _gl.GenTexture();
        DepthRenderBuffer = _gl.GenRenderbuffer();

        _gl.BindTexture(GLEnum.Texture2D, ColorBuffer);

        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

        _gl.BindTexture(GLEnum.Texture2D, 0);

        Resize(w, h);

        demoPlane = new Plane(gl);
        demoTex = new Texture2D(gl);
        demoTex.WriteLinearColor([Color.Blue, Color.Red], new PointF(0.0f, 0.0f), new PointF(1.0f, 1.0f));
    }

    public override void Resize(int w, int h)
    {
        _gl.BindTexture(GLEnum.Texture2D, ColorBuffer);
        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgb, (uint)w, (uint)h, 0, GLEnum.Rgb, GLEnum.UnsignedByte, null);
        _gl.BindTexture(GLEnum.Texture2D, 0);

        _gl.BindRenderbuffer(GLEnum.Renderbuffer, DepthRenderBuffer);
        if (_extMRT != null)
        {
            _extMRT.RenderbufferStorageMultisample((EXT)GLEnum.Renderbuffer, _samples, (EXT)GLEnum.Depth32fStencil8, (uint)w, (uint)h);
        }
        else
        {
            _gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.Depth32fStencil8, (uint)w, (uint)h);
        }
        _gl.BindRenderbuffer(GLEnum.Renderbuffer, 0);

        _gl.BindFramebuffer(GLEnum.Framebuffer, Id);
        if (_extMRT != null)
        {
            _extMRT.FramebufferTexture2DMultisample((EXT)GLEnum.Framebuffer, (EXT)GLEnum.ColorAttachment0, (EXT)GLEnum.Texture2D, ColorBuffer, 0, _samples);
        }
        else
        {
            _gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, ColorBuffer, 0);
        }
        _gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthStencilAttachment, GLEnum.Renderbuffer, DepthRenderBuffer);
        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);

        base.Resize(w, h);

        GRBackendTexture backendTexture = new(w, h, false, new GRGlTextureInfo(0x0DE1, ColorBuffer, _colorType.ToGlSizedFormat()));

        sKImage = SKImage.FromTexture(surface.Context, backendTexture, GRSurfaceOrigin.BottomLeft, _colorType);
    }

    public void Demo(ModelShader modelShader, Camera camera)
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Id);
        _gl.Viewport(0, 0, (uint)Width, (uint)Height);
        camera.Width = Width;
        camera.Height = Height;

        _gl.ClearColor(Color.Black);

        _gl.Clear((uint)GLEnum.ColorBufferBit | (uint)GLEnum.DepthBufferBit | (uint)GLEnum.StencilBufferBit);

        modelShader.Use();

        _gl.SetUniform(modelShader.UniMVP, Matrix4X4.CreateScale(2.0f, 0.0f, 2.0f) * camera.View * camera.Projection);
        _gl.SetUniform(modelShader.UniTex, 0);

        _gl.ActiveTexture(GLEnum.Texture0);
        _gl.BindTexture(GLEnum.Texture2D, demoTex.Id);

        demoPlane.Draw(modelShader);

        modelShader.Unuse();
    }

    public override void Destroy()
    {
        _gl.DeleteFramebuffer(Id);
        _gl.DeleteTexture(ColorBuffer);
        _gl.DeleteRenderbuffer(DepthRenderBuffer);
    }

    protected override uint GetFramebuffer() => Id;
}
