using Silk.NET.OpenGLES;
using SkiaBlend.Helpers;

namespace SkiaBlend.Shaders;

public class ModelShader : IDisposable
{
    private readonly GL _gl;

    public uint Id { get; }

    public uint InPos { get; }

    public uint InUV { get; }

    public int UniMVP { get; }

    public int UniTex { get; }

    public ModelShader(GL gl)
    {
        _gl = gl;

        Id = _gl.CreateShaderProgram("Resources/Shader/model.vert", "Resources/Shader/model.frag");

        // Attributes
        InPos = (uint)_gl.GetAttribLocation(Id, "in_Pos");
        InUV = (uint)_gl.GetAttribLocation(Id, "in_UV");

        // Uniforms
        UniMVP = _gl.GetUniformLocation(Id, "u_MVP");
        UniTex = _gl.GetUniformLocation(Id, "u_Tex");
    }

    public void Use()
    {
        _gl.UseProgram(Id);

        _gl.EnableVertexAttribArray(InPos);
        _gl.EnableVertexAttribArray(InUV);
    }

    public void Unuse()
    {
        _gl.DisableVertexAttribArray(InPos);
        _gl.DisableVertexAttribArray(InUV);

        _gl.UseProgram(0);
    }

    public void Dispose()
    {
        _gl.DeleteProgram(Id);

        GC.SuppressFinalize(this);
    }
}
