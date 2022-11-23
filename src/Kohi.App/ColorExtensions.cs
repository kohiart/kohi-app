using Microsoft.Xna.Framework;

namespace Kohi;

public static class ColorExtensions
{
    public static System.Numerics.Vector4 ToImGuiVector4(this Color value)
    {
        return new System.Numerics.Vector4(value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
    }

    public static Color ToXnaColor(this System.Numerics.Vector4 value)
    {
        return new Color(value.X, value.Y, value.Z, value.W);
    }
}