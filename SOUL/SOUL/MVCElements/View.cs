using game.MVCElements.Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.MVCElements
{
    public class View
    {
        public AnimationPlayer animPlayer;
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

        public void DrawAnimation(GameTime gameTime, SpriteBatch spriteBatch,
            Vector2 posWithOffset, SpriteEffects flip)
        {
            animPlayer.Draw(gameTime, spriteBatch, posWithOffset, flip);
        }

        public void CallAnimation(Animation animation)
        {
            animPlayer.PlayAnimation(animation);
        }

        public void PauseAnimation() => animPlayer.Pause();

        public void UnpauseAnimation() => animPlayer.Unpause();
    }
}
