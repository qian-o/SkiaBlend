﻿using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.EXT;
using SkiaBlend.Helpers;
using SkiaBlend.Shaders;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SkiaBlend.Tools;

public unsafe class GLFrame : Frame
{
    private readonly GL _gl;
    private readonly ExtMultisampledRenderToTexture _extMRT;
    private readonly uint _samples;

    private readonly Plane demoPlane = null!;
    private readonly Texture2D demoTex = null!;

    public uint Id { get; }

    public uint Framebuffer { get; }

    public uint DepthRenderBuffer { get; }

    public GLFrame(GL gl, int? samples, int w, int h)
    {
        _gl = gl;
        _gl.TryGetExtension(out _extMRT);
        _samples = samples != null ? (uint)samples : 1;

        Id = _gl.GenFramebuffer();
        Framebuffer = _gl.GenTexture();
        DepthRenderBuffer = _gl.GenRenderbuffer();

        _gl.BindTexture(GLEnum.Texture2D, Framebuffer);

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
        if (width == w && height == h)
        {
            return;
        }

        width = w;
        height = h;

        _gl.BindTexture(GLEnum.Texture2D, Framebuffer);
        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgb, (uint)Width, (uint)Height, 0, GLEnum.Rgb, GLEnum.UnsignedByte, null);
        _gl.BindTexture(GLEnum.Texture2D, 0);

        _gl.BindRenderbuffer(GLEnum.Renderbuffer, DepthRenderBuffer);
        if (_extMRT != null)
        {
            _extMRT.RenderbufferStorageMultisample((EXT)GLEnum.Renderbuffer, _samples, (EXT)GLEnum.Depth32fStencil8, (uint)Width, (uint)Height);
        }
        else
        {
            _gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.Depth32fStencil8, (uint)Width, (uint)Height);
        }
        _gl.BindRenderbuffer(GLEnum.Renderbuffer, 0);

        _gl.BindFramebuffer(GLEnum.Framebuffer, Id);
        if (_extMRT != null)
        {
            _extMRT.FramebufferTexture2DMultisample((EXT)GLEnum.Framebuffer, (EXT)GLEnum.ColorAttachment0, (EXT)GLEnum.Texture2D, Framebuffer, 0, _samples);
        }
        else
        {
            _gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, Framebuffer, 0);
        }
        _gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthStencilAttachment, GLEnum.Renderbuffer, DepthRenderBuffer);
        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);

        pixels = Marshal.AllocHGlobal(Width * Height * 4);

        isReady = true;
    }

    public override void DrawFrame(Frame frame, float ox, float oy, float sx, float sy)
    {
        throw new NotImplementedException();
    }

    public void Demo(ModelShader modelShader, Camera camera)
    {
        _gl.ClearColor(Color.Black);

        _gl.Clear((uint)GLEnum.ColorBufferBit | (uint)GLEnum.DepthBufferBit | (uint)GLEnum.StencilBufferBit);

        _gl.UseProgram(modelShader.Id);
        _gl.EnableVertexAttribArray(modelShader.InPos);
        _gl.EnableVertexAttribArray(modelShader.InUV);

        _gl.SetUniform(modelShader.UniMVP, Matrix4X4.CreateScale(2.0f, 0.0f, 2.0f) * camera.View * camera.Projection);
        _gl.SetUniform(modelShader.UniTex, 0);

        _gl.ActiveTexture(GLEnum.Texture0);
        _gl.BindTexture(GLEnum.Texture2D, demoTex.Id);

        demoPlane.Draw(modelShader);

        _gl.ReadPixels(0, 0, (uint)Width, (uint)Height, GLEnum.Rgba, GLEnum.UnsignedByte, (void*)pixels);
    }

    public override void Destroy()
    {
        _gl.DeleteFramebuffer(Id);
        _gl.DeleteTexture(Framebuffer);
        _gl.DeleteRenderbuffer(DepthRenderBuffer);

        if (pixels != 0x00)
        {
            Marshal.FreeHGlobal(pixels);
        }
    }
}
