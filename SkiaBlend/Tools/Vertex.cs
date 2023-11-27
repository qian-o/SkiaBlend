using Silk.NET.Maths;

namespace SkiaBlend.Tools;

public struct Vertex(Vector3D<float> position, Vector2D<float> texCoords)
{
    public Vector3D<float> Position = position;

    public Vector2D<float> TexCoords = texCoords;
}
