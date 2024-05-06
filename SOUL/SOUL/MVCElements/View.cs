using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.MVCElements
{
    public class View
    {
        public Texture2D Texture;
        public Color color = Color.White;

        public View(Texture2D texture)
        {
            Texture = texture;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(Texture, position, color);
        }
    }
}
