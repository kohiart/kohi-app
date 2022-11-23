using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;

namespace Kohi;

public static class VertexPositionTextureColor
{
    public static readonly VertexDeclaration Declaration;

    public static readonly int Size;

    static VertexPositionTextureColor()
    {
        unsafe { Size = sizeof(ImDrawVert); }

        Declaration = new VertexDeclaration(
            Size,

            // Position
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),

            // UV
            new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),

            // Color
            new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );
    }
}