using game.LevelInfo;
using game.MVCElements;
using game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace game.Entities
{
    public class Door : Model
    {
        private float shiftedDist;
        public bool IsOpened;
        private SoundEffect opening;
        private const float shiftByOpening = (float)Tile.Height;
        private const float pullingSpeed = 1.0f;
        public List<Button> ParentButtons = new List<Button>();

        public Door(Vector2 position, Texture2D texture, Level level)
        {
            opening = level.Content.Load<SoundEffect>("Sounds/Door");
            Position = new Vector2(position.X * Tile.Width, position.Y * Tile.Height);
            View = new View(texture);
            Level = level;
        }

        public override void Update(GameTime gameTime)
        {
            if (shiftedDist == 0)
                opening.Play();
            if (shiftedDist >= shiftByOpening)
            {
                Level.DoorsToUpdate.Remove(this);
                Level.RemoveCollision((int)(Position.X / Tile.Width), (int)(Position.Y / Tile.Height) - 1);
                Level.SetCollision((int)(Position.X / Tile.Width), (int)(Position.Y / Tile.Height));
                return;
            }
            Position = new Vector2(Position.X, Position.Y + pullingSpeed);
            shiftedDist += pullingSpeed;
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            View.Draw(spriteBatch, Position);
        }
    }
}
