using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace SkiaBlend.Tools;

public unsafe class Plane
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
            new Vertex(new Vector3D<float>( 0.5f, 0.0f, -0.5f), new Vector2D<float>(1.0f, 1.0f)),
            new Vertex(new Vector3D<float>( 0.5f, 0.0f,  0.5f), new Vector2D<float>(1.0f, 0.0f)),
            new Vertex(new Vector3D<float>( 0.5f, 0.0f,  0.5f), new Vector2D<float>(1.0f, 0.0f)),
            new Vertex(new Vector3D<float>(-0.5f, 0.0f,  0.5f), new Vector2D<float>(0.0f, 0.0f)),
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
}
