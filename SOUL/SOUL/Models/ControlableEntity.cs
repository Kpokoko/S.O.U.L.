using game.Extensions;
using game.LevelInfo;
using game.Tiles;
using game.MVCElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using game.MVCElements.Animations;
using Microsoft.Xna.Framework.Graphics;

namespace game.Entities
{
    public class ControlableEntity : Model
    {
        public float Stamina = 100f;
        public Controller Input;
        public bool isJumping;
        public bool InAir;
        public bool IsClimbing;
        public bool IsLastTile;
        public float AirTime;
        public const float MaxAirTime = 0.525f;
        public float StartJumpVelocity = -2200.0f;
        public const float JumpControlPower = 0.14f;
        public bool NearWall;
        float MoveAcceleration = 13000f;
        float GravityAcceleration = 3400f;
        const float MaxFallSpeed = 550f;
        const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;
        float MaxMovementSpeed = 1750f;

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public float Jump(float velocityY, GameTime gameTime)
        {
            var time = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (isJumping)
            {
                if ((!InAir && IsOnGround) || AirTime > 0.0f)
                {
                    AirTime += time;
                    //test.PlayAnimation(new Animation(Level.Content.Load<Texture2D>("Player/Jump"), 0.1f, false));
                }
                if (AirTime > 0.0f && AirTime <= MaxAirTime)
                    velocityY =
                        StartJumpVelocity * (1 - (float)Math.Pow(AirTime / MaxAirTime, JumpControlPower));
                else
                    AirTime = 0;
            }
            else
                AirTime = 0;
            InAir = isJumping;
            return velocityY;
        }

        public Vector2 Climb(Vector2 velocity, GameTime gameTime)
        {
            Stamina -= 0.5f;
            var currentKey = Keyboard.GetState();
            if (!currentKey.IsKeyDown(Input.Up) || !currentKey.IsKeyDown(Input.Down))
                velocity.X = 0;
            var bounds = this.BoundingRectangle;
            var neighbourTiles = GetNeighbourTiles(bounds);
            var neighbourWalls = GetNeighbourWalls(neighbourTiles);
            var neighbourWallsAsTiles = GetNeighbourWallsAsTiles(neighbourTiles);
            return MovementInClimbing(neighbourWalls, neighbourWallsAsTiles, bounds, currentKey, velocity);
        }

        private Vector2 MovementInClimbing(Rectangle[] neighbourWalls, TypeOfTile[] neighbourWallsAsTiles,
            Rectangle bounds, KeyboardState currentKey, Vector2 velocity)
        {
            var leftWall = neighbourWalls[1];
            var leftTile = neighbourWallsAsTiles[1];
            var rightWall = neighbourWalls[4];
            var rightTile = neighbourWallsAsTiles[4];
            float[] distances = new float[0];
            if ((leftTile is TypeOfTile.Impassable || leftTile is TypeOfTile.Button) ||
                (rightTile is TypeOfTile.Impassable || rightTile is TypeOfTile.Button))
            {
                var isLeftSide = true;
                var distanceToLeft = RectangleExtensions.GetDistancesBetween(bounds, leftWall)[0];
                var distanceToRight = RectangleExtensions.GetDistancesBetween(bounds, rightWall)[0];
                if ((leftTile is TypeOfTile.Impassable || leftTile is TypeOfTile.Button)
                    && distanceToLeft < Math.Abs(distanceToRight))
                    distances = RectangleExtensions.GetDistancesBetween(bounds, leftWall);
                else if ((rightTile is TypeOfTile.Impassable || rightTile is TypeOfTile.Button)
                    && distanceToRight < distanceToLeft)
                {
                    isLeftSide = false;
                    distances = RectangleExtensions.GetDistancesBetween(bounds, rightWall);
                }
                else
                    return Vector2.Zero;
                if (Math.Abs(distances[0]) <= 50)
                {
                    ClimbingMove(isLeftSide, distances, currentKey);
                    IsClimbing = true;
                    return new Vector2(velocity.X, VerticalMovement);
                }
            }
            IsClimbing = false;
            return new Vector2(velocity.X, VerticalMovement);
        }

        private void ClimbingMove(bool isLeftSide, float[] distances,
            KeyboardState currentKey)
        {
            if (isLeftSide)
                Position.X -= Math.Abs(distances[0]) - distances[2];
            else
                Position.X += Math.Abs(distances[0]) - distances[2];
            if (currentKey.IsKeyDown(Input.Up))
            {
                VerticalMovement = -100;
                Stamina -= 0.5f;
            }
            else if (currentKey.IsKeyDown(Input.Down))
            {
                VerticalMovement = 100;
                Stamina -= 0.5f;
            }
        }

        public TypeOfTile[] GetNeighbourWallsAsTiles(int[] neighbourTiles)
        {
            var upperLeft = Level.HoldInBounds(neighbourTiles[0], neighbourTiles[2]);
            var left = Level.HoldInBounds(neighbourTiles[0], neighbourTiles[2] + 1);
            var downLeft = Level.HoldInBounds(neighbourTiles[0], neighbourTiles[3]);
            var upperRight = Level.HoldInBounds(neighbourTiles[1], neighbourTiles[2]);
            var right = Level.HoldInBounds(neighbourTiles[1], neighbourTiles[2] + 1);
            var downRight = Level.HoldInBounds(neighbourTiles[1], neighbourTiles[3]);
            var down = Level.HoldInBounds(neighbourTiles[1] - 1, neighbourTiles[3]);
            return new TypeOfTile[]
            {
                upperLeft,
                left,
                downLeft,
                upperRight,
                right,
                downRight,
                down
            };
        }

        public Rectangle[] GetNeighbourWalls(int[] neighbourTiles)
        {
            var upperLeft = Level.GetBounds(neighbourTiles[0], neighbourTiles[2]);
            var left = Level.GetBounds(neighbourTiles[0], neighbourTiles[2] + 1);
            var downLeft = Level.GetBounds(neighbourTiles[0], neighbourTiles[3]);
            var upperRight = Level.GetBounds(neighbourTiles[1], neighbourTiles[2]);
            var right = Level.GetBounds(neighbourTiles[1], neighbourTiles[2] + 1);
            var downRight = Level.GetBounds(neighbourTiles[1], neighbourTiles[3]);
            return new Rectangle[]
            {
                upperLeft,
                left,
                downLeft,
                upperRight,
                right,
                downRight
            };
        }

        #region Physics
        public void ApplyPhysics(GameTime gameTime)
        {
            var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 previousPosition = Position;
            Velocity.X += HorizontalMovement * MoveAcceleration * elapsed;
            Velocity.Y = MathHelper.Clamp(Velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
            Velocity.Y = Jump(Velocity.Y, gameTime);
            if (Keyboard.GetState().IsKeyDown(Input.Grab) && Stamina > 0)
                Velocity = Climb(Velocity, gameTime);
            if (!isJumping)
                Velocity.X *= GroundDragFactor;
            else
                Velocity.X *= AirDragFactor;
            Velocity.X = MathHelper.Clamp(Velocity.X, -MaxMovementSpeed, MaxMovementSpeed);
            Position += Velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));
            HandleCollisions();
            if (Position.X == previousPosition.X)
                Velocity.X = 0;
            if (Position.Y == previousPosition.Y && InAir)
            {
                AirTime = MaxAirTime;
                Velocity.Y = 0;
            }
            else if (Position.Y == previousPosition.Y)
                Velocity.Y = 0;
        }
        #endregion

        public void OnExitReached()
        {
            Console.WriteLine("END");   
        }
    }
}
