using game.MVCElements;
using game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace game.Entities
{
    public class Soul : ControlableEntity
    {
        #region fields
        private float lifeTime;
        private const float maxLifeTime = 3.0f;
        public bool CloneLifeEnded;
        private bool IsBorn;
        private bool IsSplashing;
        private Player player;
        private Vector2 previousPosition;
        #endregion fields

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
                Left = Keys.Left,
                Right = Keys.Right,
                Jump = Keys.Space,
                SplashDown = Keys.Down
            };
            animations = parent.animations;
            View.CallAnimation(animations["idle"]);
        }

        public override void Update(GameTime gameTime)
        {
            if (IsBorn)
            {
                Position = new Vector2(Position.X, Position.Y - 10);
                HandleCollisions();
                if (previousPosition.Y == Position.Y)
                {
                    IsBorn = false;
                    lifeTime = 0;
                }
                else
                    previousPosition.Y = Position.Y;
                return;
            }
            CheckDespawn(gameTime);
            if (IsDead)
                View.CallAnimation(animations["die"]);
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
                Position = new Vector2(Position.X, Position.Y + 14);
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

        public void FollowPlayer(Vector2 playerPos) => this.Position = playerPos;
    }
}
