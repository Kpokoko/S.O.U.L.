using game.LevelInfo;
using game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using game.MVCElements;
using game.MVCElements.Animations;

namespace game.Entities
{
    public class Player : ControlableEntity
    {
        public Soul Soul;
        public bool IsSoulAlive;
        public AnimationPlayer test;

        public Player(Level level, Vector2 Position, Texture2D texture)
        {
            View = new View(texture);
            Level = level;
            Reset(Position);
            test = new AnimationPlayer();
            test.PlayAnimation(new Animation(Level.Content.Load<Texture2D>("Player/Idle"), 0.1f, true));
        }

        public void Reset(Vector2 position)
        {
            Position = position;
            IsDead = false;
            Input = new Controller
            {
                Left = Keys.A,
                Right = Keys.D,
                Jump = Keys.Space,
                Grab = Keys.E,
                Up = Keys.W,
                Down = Keys.S,
                ThrowClone = Keys.Q
            };
            Soul = new Soul(this);
        }

        public override void Update(GameTime gameTime)
        {
            Move(gameTime);
            ApplyPhysics(gameTime);
            HorizontalMovement = 0.0f;
            VerticalMovement = 0.0f;
            isJumping = false;
            if (!IsSoulAlive)
                test.Unpause();
        }

        private void Move(GameTime gameTime)
        {
            var currentKey = Keyboard.GetState();

            //НЕ ЗАБУДЬ ЭТО НАПИСАТЬ ПО-ЧЕЛОЕЧЕСКИ!!!!!!
            if (currentKey.IsKeyDown(Keys.R))
            {
                IsDead = true;
                test.PlayAnimation(new Animation(Level.Content.Load<Texture2D>("Player/Die"), 0.1f, false));
                return;
            }


            if (currentKey.IsKeyDown(Input.Left))
                HorizontalMovement = -1.0f;
            if (currentKey.IsKeyDown(Input.Right))
                HorizontalMovement = 1.0f;
            isJumping = currentKey.IsKeyDown(Input.Jump);
            if (currentKey.IsKeyDown(Input.ThrowClone) && !IsSoulAlive)
                CreateClone();
            var neighbourTiles = GetNeighbourTiles(BoundingRectangle);
            var neighbourWallsAsTiles = GetNeighbourWallsAsTiles(neighbourTiles);
            if (neighbourWallsAsTiles[6] is TypeOfTile.Button)
                Level.ButtonPressed(new Vector2(neighbourTiles[1] - 1, neighbourTiles[3]), gameTime);
        }

        private void CreateClone()
        {
            Soul = Soul.Clone() as Soul;
            IsSoulAlive = true;
            Soul.Parent = this;
            Soul.IsBorn = true;
            test.Pause();
        }
        
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            test.Draw(gameTime, spriteBatch, new Vector2(Position.X + 5, Position.Y + 40), SpriteEffects.None);
            //View.Draw(spriteBatch, Position);
            //spriteBatch.Draw(this.texture, Position, color);
        }
    }
}
