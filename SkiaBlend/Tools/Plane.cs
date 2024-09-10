using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaBlend.Shaders;

namespace SkiaBlend.Tools;

public unsafe class Plane : IDisposable
{
    private readonly GL _gl;
    private readonly Vertex[] _vertices;
    private readonly uint[] _indices;
    private readonly uint vBO;
    private readonly uint eBO;

    public Vertex[] Vertices => _vertices;

    public uint[] Indices => _indices;

    public uint VBO => vBO;

    public uint EBO => eBO;

    public Matrix4X4<float> Model { get; set; } = Matrix4X4<float>.Identity;

    public Plane(GL gl)
    {
        _gl = gl;

        _vertices =
        [
            new Vertex(new Vector3D<float>(-0.5f, 0.0f, -0.5f), new Vector2D<float>(0.0f, 1.0f)),
            new Vertex(new Vector3D<float>(0.5f, 0.0f, -0.5f), new Vector2D<float>(1.0f, 1.0f)),
            new Vertex(new Vector3D<float>(0.5f, 0.0f, 0.5f), new Vector2D<float>(1.0f, 0.0f)),
            new Vertex(new Vector3D<float>(0.5f, 0.0f, 0.5f), new Vector2D<float>(1.0f, 0.0f)),
            new Vertex(new Vector3D<float>(-0.5f, 0.0f, 0.5f), new Vector2D<float>(0.0f, 0.0f)),
            new Vertex(new Vector3D<float>(-0.5f, 0.0f, -0.5f), new Vector2D<float>(0.0f, 1.0f))
        ];

        _indices = _vertices.Select((a, b) => (uint)b).ToArray();

        vBO = _gl.GenBuffer();
        eBO = _gl.GenBuffer();

        _gl.BindBuffer(GLEnum.ArrayBuffer, vBO);
        _gl.BufferData<Vertex>(GLEnum.ArrayBuffer, (uint)(_vertices.Length * sizeof(Vertex)), _vertices, GLEnum.StaticDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.BindBuffer(GLEnum.ElementArrayBuffer, eBO);
        _gl.BufferData<uint>(GLEnum.ElementArrayBuffer, (uint)(_indices.Length * sizeof(uint)), _indices, GLEnum.StaticDraw);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
    }

    public void Draw(ModelShader modelShader)
    {
        _gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
        _gl.VertexAttribPointer(modelShader.InPos, 3, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)Marshal.OffsetOf<Vertex>(nameof(Vertex.Position)));
        _gl.VertexAttribPointer(modelShader.InUV, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoords)));
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
        _gl.DrawElements(GLEnum.Triangles, (uint)Indices.Length, GLEnum.UnsignedInt, (void*)0);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(VBO);
        _gl.DeleteBuffer(EBO);

        GC.SuppressFinalize(this);
    }
}
