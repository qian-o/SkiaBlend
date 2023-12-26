using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.EXT;
using SkiaBlend.Helpers;
using SkiaBlend.Shaders;
using SkiaSharp;
using System.Drawing;

namespace SkiaBlend.Tools;

public unsafe class GLCanvas : Canvas
{
    private readonly ExtMultisampledRenderToTexture _extMRT;
    private readonly uint _samples;
    private readonly SkiaCanvas _skiaCanvas;
    private readonly ModelShader _modelShader;
    private readonly Plane _plane;
    private readonly Texture2D _linearColor;

    private GRBackendTexture backendTexture = null!;

    public GLCanvas(GL gl, Vector2D<uint> size, int? samples, SkiaCanvas skiaCanvas) : base(gl)
    {
        _gl.TryGetExtension(out _extMRT);
        _samples = samples != null ? (uint)samples : 1;
        _skiaCanvas = skiaCanvas;
        _modelShader = new ModelShader(_gl);
        _plane = new Plane(_gl);
        _linearColor = new Texture2D(_gl);
        _linearColor.WriteLinearColor([Color.Blue, Color.Red], new PointF(0.0f, 0.0f), new PointF(1.0f, 1.0f));

        Id = _gl.GenFramebuffer();
        ColorBuffer = _gl.GenTexture();
        DepthRenderBuffer = _gl.GenRenderbuffer();

        _gl.BindTexture(GLEnum.Texture2D, ColorBuffer);

        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

        _gl.BindTexture(GLEnum.Texture2D, 0);

        Resize(size);
    }

    public uint Id { get; }

    public uint ColorBuffer { get; }

    public uint DepthRenderBuffer { get; }

    public SKImage Image { get; private set; } = null!;

    public override void Begin(Color clearColor)
    {
        _gl.Enable(EnableCap.Multisample);

        _gl.Enable(GLEnum.DepthTest);
        _gl.DepthFunc(GLEnum.Less);
        _gl.DepthMask(true);

        _gl.Enable(GLEnum.StencilTest);
        _gl.StencilFunc(GLEnum.Always, 1, 0xFF);

        _gl.Enable(GLEnum.Blend);
        _gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        _gl.BindFramebuffer(GLEnum.Framebuffer, Id);
        _gl.Viewport(0, 0, Width, Height);
        _gl.Scissor(0, 0, Width, Height);

        _gl.ClearColor(clearColor);
        _gl.Clear((uint)GLEnum.ColorBufferBit | (uint)GLEnum.DepthBufferBit | (uint)GLEnum.StencilBufferBit);
    }

    public override void End()
    {
        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

    public override void DrawCanvas(Canvas canvas, Vector2D<float> offset, Vector2D<float> scale)
    {

    }

    public void Demo(Camera camera)
    {
        camera.Width = (int)Width;
        camera.Height = (int)Height;

        _modelShader.Use();

        _gl.SetUniform(_modelShader.UniMVP, Matrix4X4.CreateScale(2.0f, 0.0f, 2.0f) * camera.View * camera.Projection);
        _gl.SetUniform(_modelShader.UniTex, 0);

        _gl.ActiveTexture(GLEnum.Texture0);

        _gl.BindTexture(GLEnum.Texture2D, _linearColor.Id);

        _plane.Draw(_modelShader);

        _gl.BindTexture(GLEnum.Texture2D, 0);

        _modelShader.Unuse();
    }

    protected override bool Initialization()
    {
        _gl.BindTexture(GLEnum.Texture2D, ColorBuffer);
        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)_glFormatAndType, Width, Height, 0, _glFormat, _glType, null);
        _gl.BindTexture(GLEnum.Texture2D, 0);

        _gl.BindRenderbuffer(GLEnum.Renderbuffer, DepthRenderBuffer);
        if (_extMRT != null)
        {
            _extMRT.RenderbufferStorageMultisample((EXT)GLEnum.Renderbuffer, _samples, (EXT)GLEnum.Depth32fStencil8, Width, Height);
        }
        else
        {
            _gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.Depth32fStencil8, Width, Height);
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

        backendTexture = new((int)Width, (int)Height, true, new GRGlTextureInfo((uint)GLEnum.Texture2D, ColorBuffer, _skFormatAndType.ToGlSizedFormat()));
        Image = SKImage.FromTexture(_skiaCanvas.Context, backendTexture, GRSurfaceOrigin.BottomLeft, _skFormatAndType);

        return true;
    }

    protected override void Destroy()
    {
        Image?.Dispose();
        backendTexture?.Dispose();

        if (IsDisposed)
        {
            _linearColor.Dispose();
            _plane.Dispose();
            _modelShader.Dispose();

            _gl.DeleteFramebuffer(Id);
            _gl.DeleteTexture(ColorBuffer);
            _gl.DeleteRenderbuffer(DepthRenderBuffer);

            _extMRT?.Dispose();
        }
    }
}
