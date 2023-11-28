using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaBlend.Helpers;
using SkiaBlend.Shaders;
using SkiaSharp;
using System.Drawing;

namespace SkiaBlend.Tools;

public unsafe class GLCanvas : Canvas
{
    private readonly uint _samples;
    private readonly SkiaCanvas _skiaCanvas;
    private readonly ModelShader _modelShader;
    private readonly Plane _plane;
    private readonly Texture2D _linearColor;

    public GLCanvas(GL gl, Vector2D<uint> size, int? samples, SkiaCanvas skiaCanvas) : base(gl)
    {
        _samples = samples != null ? (uint)samples : 1;
        _skiaCanvas = skiaCanvas;
        _modelShader = new ModelShader(_gl);
        _plane = new Plane(_gl);
        _linearColor = new Texture2D(_gl);
        _linearColor.WriteLinearColor([Color.Blue, Color.Red], new PointF(0.0f, 0.0f), new PointF(1.0f, 1.0f));

        Id = _gl.GenFramebuffer();
        ColorBuffer = _gl.GenRenderbuffer();
        DepthRenderBuffer = _gl.GenRenderbuffer();
        PresentId = _gl.GenFramebuffer();
        PresentColorBuffer = _gl.GenTexture();

        _gl.BindTexture(GLEnum.Texture2D, ColorBuffer);

        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

        _gl.BindTexture(GLEnum.Texture2D, 0);

        Resize(size);
    }

    public uint Id { get; }

    public uint ColorBuffer { get; }

    public uint DepthRenderBuffer { get; }

    public uint PresentId { get; }

    public uint PresentColorBuffer { get; }

    public SKImage SKImage { get; private set; } = null!;

    public override void DrawCanvas(Canvas canvas, Vector2D<float> offset, Vector2D<float> scale)
    {

    }

    public void Present()
    {
        _gl.BindFramebuffer(GLEnum.ReadFramebuffer, Id);
        _gl.BindFramebuffer(GLEnum.DrawFramebuffer, PresentId);
        _gl.BlitFramebuffer(0, 0, (int)Width, (int)Height, 0, 0, (int)Width, (int)Height, ClearBufferMask.ColorBufferBit, GLEnum.Nearest);
    }

    public void Demo(Camera camera)
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Id);
        _gl.Viewport(0, 0, Width, Height);

        camera.Width = (int)Width;
        camera.Height = (int)Height;

        _gl.ClearColor(Color.Black);

        _gl.Clear((uint)GLEnum.ColorBufferBit | (uint)GLEnum.DepthBufferBit | (uint)GLEnum.StencilBufferBit);

        _modelShader.Use();

        _gl.SetUniform(_modelShader.UniMVP, Matrix4X4.CreateScale(2.0f, 0.0f, 2.0f) * camera.View * camera.Projection);
        _gl.SetUniform(_modelShader.UniTex, 0);

        _gl.ActiveTexture(GLEnum.Texture0);
        _gl.BindTexture(GLEnum.Texture2D, _linearColor.Id);

        _plane.Draw(_modelShader);

        _modelShader.Unuse();

        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

    protected override bool Initialization()
    {
        _gl.BindRenderbuffer(GLEnum.Renderbuffer, ColorBuffer);
        _gl.RenderbufferStorageMultisample(GLEnum.Renderbuffer, _samples, _glColorAndType, Width, Height);
        _gl.BindRenderbuffer(GLEnum.Renderbuffer, 0);

        _gl.BindRenderbuffer(GLEnum.Renderbuffer, DepthRenderBuffer);
        _gl.RenderbufferStorageMultisample(GLEnum.Renderbuffer, _samples, GLEnum.Depth32fStencil8, Width, Height);
        _gl.BindRenderbuffer(GLEnum.Renderbuffer, 0);

        _gl.BindFramebuffer(GLEnum.Framebuffer, Id);
        _gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Renderbuffer, ColorBuffer);
        _gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthStencilAttachment, GLEnum.Renderbuffer, DepthRenderBuffer);
        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);

        _gl.BindTexture(GLEnum.Texture2D, PresentColorBuffer);
        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)_glColorAndType, Width, Height, 0, _glColor, _glColorType, null);
        _gl.BindTexture(GLEnum.Texture2D, 0);

        _gl.BindFramebuffer(GLEnum.Framebuffer, PresentId);
        _gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, PresentColorBuffer, 0);
        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);

        SKImage = SKImage.FromAdoptedTexture(_skiaCanvas.Context, new GRBackendTexture((int)Width, (int)Height, true, new GRGlTextureInfo((uint)GLEnum.Texture2D, PresentColorBuffer, _skColorAndType.ToGlSizedFormat())), GRSurfaceOrigin.BottomLeft, _skColorAndType);

        return true;
    }

    protected override void Destroy()
    {
        SKImage?.Dispose();
    }
}
