using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kohi;

internal sealed class LoadingScreen
{
    private readonly Kohi game;
    private readonly Texture2D texture;
    private readonly Color backgroundColor = Color.Black;

    public LoadingScreen(Kohi game)
    {
        this.game = game;
        texture = Texture2D.FromStream(game.GraphicsDevice, File.OpenRead("Content\\logo.png"));
    }

    public void Draw()
    {
        game.sb.GraphicsDevice.Clear(backgroundColor);

        var viewport = game.sb.GraphicsDevice.Viewport;

        var position = new Vector2(
            viewport.Bounds.Width / 2f - texture.Width / 2f,
            viewport.Bounds.Height / 2f - texture.Height / 2f);

        game.sb.Begin(0, null, SamplerState.PointClamp, null, null, null);
        game.sb.Draw(texture, position, null, Color.White, 0f, Vector2.One, 1f, 0, 0);
        game.sb.End();
    }
}