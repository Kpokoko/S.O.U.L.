using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.Tiles
{
    public struct Tile
    {
        public Texture2D Texture;
        public TypeOfTile Collision;

        public const int Height = 60;
        public const int Width = 60;

        public static readonly Vector2 Size = new Vector2(Width, Height);

        public Tile(Texture2D texture, TypeOfTile collision)
        {
            Texture = texture;
            Collision = collision;
        }
    }
}
