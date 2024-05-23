using game.LevelInfo;
using game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using game.MVCElements;
using game.MVCElements.Animations;
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Audio;

namespace game.Entities
{
    public class Player : ControlableEntity
    {
        public Soul Soul { get; private set; }
        public bool IsSoulAlive;
        public bool IsSkipped { get; private set; }
        public Player(Level level, Vector2 Position, Texture2D texture)
        {
            View = new View(texture);
            Level = level;
            animations = new Dictionary<string, Animation>
            {
                { "walk", new Animation(Level.Content.Load<Texture2D>("Player/Walk"), 0.1f, true) },
                { "idle", new Animation(Level.Content.Load<Texture2D>("Player/Idle"), 0.1f, true) },
                { "die", new Animation(Level.Content.Load<Texture2D>("Player/Die"), 0.3f, false) }
            };
            Reset(Position);
            View.CallAnimation(animations["idle"]);
        }

        public void Reset(Vector2 position)
        {
            Position = position;
            IsDead = false;
            Input = new Controller
            {
                Left = Keys.Left,
                Right = Keys.Right,
                Jump = Keys.Space,
                Grab = Keys.E,
                Up = Keys.Space,
                Down = Keys.Down,
                ThrowClone = Keys.Q,
                Restart = Keys.R
            };
            Soul = new Soul(this);
            IsSkipped = false;
        }

        #region utilities
        public void KillPlayer() => IsDead = true;
        public void RefillStamina() => Stamina = MaxStamina;
        public bool CheckStanding() => IsOnGround;

        public void AllowSkip()
        {
            Input.Skip = Keys.Enter;
        }
        #endregion utilities

        public override void Update(GameTime gameTime)
        {
            if (IsDead)
            {
                View.UnpauseAnimation();
                View.CallAnimation(animations["die"]);
                return;
            }
            Move(gameTime);
            ApplyPhysics(gameTime);
            if (!IsDead && IsOnGround)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                    View.CallAnimation(animations["walk"]);
                else
                    View.CallAnimation(animations["idle"]);
            }
            HorizontalMovement = 0;
            VerticalMovement = 0;
            isJumping = false;
            if (!IsSoulAlive)
                View.UnpauseAnimation();
        }

        private void Move(GameTime gameTime)
        {
            var currentKey = Keyboard.GetState();
            if (currentKey.IsKeyDown(Input.Restart))
            {
                IsDead = true;
                return;
            }
            if (currentKey.IsKeyDown(Input.Skip))
            {
                IsSkipped = true;
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
            var isBornInfo = Soul.GetType().GetField("IsBorn", BindingFlags.NonPublic | BindingFlags.Instance);
            isBornInfo.SetValue(Soul, true);
            View.PauseAnimation();
        }
    }
}
