using game.MVCElements;
using game.MVCElements.Animations;
using game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace game.Entities
{
    public class Soul : ControlableEntity
    {
        private float lifeTime;
        private const float maxLifeTime = 3.0f;
        public bool CloneLifeEnded;
        public bool IsBorn;
        public bool IsSplashing;
        public ControlableEntity Parent;
        public Player player;
        private Vector2 previousPosition;
        public AnimationPlayer test;

        public Soul(ControlableEntity parent)
        {
            IsSplashing = false;
            player = parent as Player;
            Level = parent.Level;
            Position = parent.Position;
            View = new View(parent.View.Texture);
            previousPosition = Position;
            Input = new Controller()
            {
                Left = Keys.A,
                Right = Keys.D,
                Jump = Keys.Space,
                SplashDown = Keys.S
            };
            test = new AnimationPlayer();
            test.PlayAnimation(new Animation(Level.Content.Load<Texture2D>("Player/Idle"), 0.1f, true));
        }

        public override void Update(GameTime gameTime)
        {
            if (IsBorn)
            {
                Position.Y -= 10.0f;
                HandleCollisions();
                if (previousPosition.Y == Position.Y)
                    IsBorn = false;
                else
                    previousPosition.Y = Position.Y;
                return;
            }
            CheckDespawn(gameTime);
            if (IsDead || !player.IsSoulAlive)
                return;
            lifeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            Move();
            ApplyPhysics(gameTime);
            HorizontalMovement = 0.0f;
            isJumping = false;
            if (lifeTime >= maxLifeTime)
            {
                player.IsSoulAlive = false;
                lifeTime = 0;
                return;
            }
            base.Update(gameTime);
        }

        private void CheckDespawn(GameTime gameTime)
        {
            if (IsSplashing && IsOnGround)
            {
                var neighbourTiles = GetNeighbourTiles(BoundingRectangle);
                var neighbourWallsAsTiles = GetNeighbourWallsAsTiles(neighbourTiles);
                if (neighbourWallsAsTiles[6] is TypeOfTile.Button)
                    Level.ButtonPressed(new Vector2(neighbourTiles[1] - 1, neighbourTiles[3]), gameTime);
                player.IsSoulAlive = false;
                IsSplashing = false;
            }
            else if (IsSplashing)
            {
                Position.Y += 14.0f;
                HandleCollisions();
            }
        }

        private void Move()
        {
            var currentKey = Keyboard.GetState();
            if (currentKey.IsKeyDown(Input.Left))
                HorizontalMovement = -1.0f;
            if (currentKey.IsKeyDown(Input.Right))
                HorizontalMovement = 1.0f;
            if (currentKey.IsKeyDown(Input.SplashDown))
            {
                IsSplashing = true;
                return;
            }
            isJumping = currentKey.IsKeyDown(Input.Jump);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            test.Draw(gameTime, spriteBatch, new Vector2(Position.X + 5, Position.Y + 40), SpriteEffects.None);
        }
    }
}
