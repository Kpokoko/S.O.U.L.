using game.LevelInfo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using game.MVCElements;
using game.Tiles;
using Microsoft.Xna.Framework.Audio;

namespace game.Entities
{
    public class Button : Model
    {
        private const float shiftByPressing = (float)Tile.Height;
        private SoundEffect pressingSound;
        public bool IsPressed;
        private const float pressingSpeed = 5f;
        private float shiftedDist;
        public Door DependentDoor;

        public Button(Vector2 position, Level level, Texture2D texture, Door door)
        {
            pressingSound = level.Content.Load<SoundEffect>("Sounds/Button");
            View = new View(texture);
            Position = new Vector2(position.X * Tile.Width, position.Y * Tile.Height);
            Level = level;
            DependentDoor = door;
        }

        public override void Update(GameTime gameTime)
        {
            if (shiftedDist >= shiftByPressing)
            {
                Level.ButtonsToUpdate.Remove(this);
                DependentDoor.IsOpened = true;
                return;
            }
            Position = new Vector2(Position.X, Position.Y + pressingSpeed);
            shiftedDist += pressingSpeed;
            base.Update(gameTime);
        }

        public void PressButton(GameTime gameTime)
        {
            pressingSound.Play();
            IsPressed = true;
            DependentDoor.IsOpened = true;
            Level.RemoveCollision((int)(Position.X / Tile.Width), (int)(Position.Y / Tile.Height));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            View.Draw(spriteBatch, Position);
        }
    }
}
