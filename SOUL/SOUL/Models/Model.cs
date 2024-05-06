using game.Extensions;
using game.LevelInfo;
using game.MVCElements;
using game.MVCElements.Animations;
using game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace game.Entities
{
    public class Model
    {
        public Vector2 Position;
        public View View;
        public Vector2 Velocity;
        public Level Level;
        public float VerticalMovement;
        public float HorizontalMovement;
        public bool IsOnGround;
        public bool IsDead;
        public float previousBottom;
        public Rectangle BoundingRectangle
        {
            get => new Rectangle((int)Position.X, (int)Position.Y, View.Texture.Width, View.Texture.Height);
        }

        public virtual void Update(GameTime gameTime)
        { }

        public virtual void Draw(SpriteBatch spriteBatch)
        { }

        #region Collision
        public void HandleCollisions()
        {
            var bounds = BoundingRectangle;
            var neighbourTiles = GetNeighbourTiles(bounds);
            var leftTile = neighbourTiles[0];
            var rightTile = neighbourTiles[1];
            var topTile = neighbourTiles[2];
            var bottomTile = neighbourTiles[3];

            IsOnGround = false;

            for (var y = topTile; y <= bottomTile; ++y)
            {
                for (var x = leftTile; x <= rightTile; ++x)
                {
                    var collision = Level.HoldInBounds(x, y);
                    if (collision != TypeOfTile.Passable)
                    {
                        var tileBounds = Level.GetBounds(x, y);
                        var depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            var absDepthX = Math.Abs(depth.X);
                            var absDepthY = Math.Abs(depth.Y);
                            if ((this is Player || this is Soul) && collision is TypeOfTile.Spike
                                && absDepthX > 10 && absDepthY > 35)
                            {
                                this.IsDead = true;
                                return;
                            }

                            if (absDepthY < absDepthX || collision is TypeOfTile.Platform)
                            {
                                if (previousBottom <= tileBounds.Top && collision is not TypeOfTile.Spike)
                                    IsOnGround = true;

                                if (collision is TypeOfTile.Impassable || collision is TypeOfTile.Button
                                    || collision is TypeOfTile.Door || IsOnGround)
                                {
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision is TypeOfTile.Impassable || collision is TypeOfTile.Button
                                || collision is TypeOfTile.Door)
                            {
                                Position = new Vector2(Position.X + depth.X, Position.Y);
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }
            previousBottom = bounds.Bottom;
        }

        public int[] GetNeighbourTiles(Rectangle bounds)
        {
            var leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width) - 1;
            var rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width));
            var topTile = (int)Math.Floor((float)bounds.Top / Tile.Height) - 1;
            var bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height));
            return new int[]
            {
                leftTile,
                rightTile,
                topTile,
                bottomTile
            };
        }
        #endregion
    }
}
