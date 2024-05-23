using game.LevelInfo;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using SOUL;
using System.Text;
using System;

namespace game
{
    public class Game1 : Game
    {
        #region fields
        private int deathInCurrRun;
        private int cameraZoom;
        private float timer = 0;
        private int deathInPrevRun;
        private int screenWidth;
        private int screenHeight;
        private float timeToSkip = 0;
        private float timeToClose = 0;
        private float saveTimer;
        private int loopCounter;
        private int deathCounter;
        private bool nextLevelLoaded;
        private bool IsEndingShow;
        private Texture2D background;
        private Color dimColor = new Color(0, 0, 0, 100);
        private Color finalLevelDim = new Color(0, 0, 0, 152);
        private SoundEffectInstance deathSound;
        private Texture2D blackRectangle;
        private SoundEffectInstance bgm;
        private GraphicsDeviceManager graphics;
        private SoundEffect finalSound;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private const int levelsCount = 13;
        private int levelNumber = -1;
        public Level level { get; private set; }
        Camera camera;
        #endregion
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }


        protected override void Initialize()
        {
            graphics.IsFullScreen = false;
            screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();
            var counterInfo = File.ReadAllText("Content/counter.txt").Split(' ');
            loopCounter = int.Parse(counterInfo[0]);
            deathCounter = int.Parse(counterInfo[1]);
            deathInPrevRun = int.Parse(counterInfo[2]);
            var oldCameraZoom = int.Parse(counterInfo[3]);
            camera = new Camera(screenWidth);
            if (deathInPrevRun == 0 && oldCameraZoom < 5)
                camera.Zoom = oldCameraZoom + 1;
            else if (oldCameraZoom > 1 && oldCameraZoom < 5)
                camera.Zoom = oldCameraZoom;
            else if (oldCameraZoom > 4)
                camera.Zoom = oldCameraZoom + 5;
            cameraZoom = (int)camera.Zoom;
            deathInCurrRun = 1;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.Content.RootDirectory = "Content";
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font");
            background = Content.Load<Texture2D>("background");
            deathSound = Content.Load<SoundEffect>("Sounds/DeathSound").CreateInstance();
            finalSound = Content.Load<SoundEffect>("Sounds/endSound");
            var bgmusic = Content.Load<SoundEffect>("Sounds/Bgm");
            bgm = bgmusic.CreateInstance();
            bgm.Volume = 0.3f;
            bgm.IsLooped = true;
            bgm.Play();
            blackRectangle = new Texture2D(GraphicsDevice, 1, 1);
            blackRectangle.SetData(new[] { Color.White });
            LoadNextLevel();
        }

        private void LoadNextLevel()
        {
            levelNumber = (levelNumber + 1) % levelsCount;
            if (nextLevelLoaded)
                timeToSkip = 0;
            nextLevelLoaded = true;
            if (level != null)
                level.Dispose();
            if (levelNumber == 12)
            {
                finalSound.Play();
                camera.Zoom = 1f;
                loopCounter++;
                if (deathInCurrRun == 1)
                    deathInCurrRun = 0;
                File.WriteAllText("Content/counter.txt",
                    loopCounter.ToString() + ' ' + deathCounter.ToString() +
                    ' ' + deathInCurrRun.ToString() + ' ' + ((int)cameraZoom).ToString(), Encoding.UTF8);
                IsEndingShow = true;
                return;
            }
            var levelPath = string.Format("Content/Levels/{0}.txt", levelNumber);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelNumber);
            if (levelNumber == 11)
                camera.Zoom = 3f;
        }

        private void ReloadLevel()
        {
            --levelNumber;
            nextLevelLoaded = false;
            LoadNextLevel();
        }

        public void ResetGame()
        {
            levelNumber = -1;
            LoadNextLevel();
        }

        protected override void Update(GameTime gameTime)
        {
            if (IsEndingShow)
            {
                DrawName();
                timeToClose += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (bgm.Volume <= 0.6)
                    bgm.Volume += (float)0.05;
                if (timeToClose >= 7)
                    Exit();
                return;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            saveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (saveTimer >= 10)
            {
                File.WriteAllText("Content/counter.txt",
                    loopCounter.ToString() + ' ' + deathCounter.ToString()
                    + ' ' + deathInCurrRun.ToString() + ' ' + ((int)cameraZoom).ToString(), Encoding.UTF8);
                saveTimer = 0;
            }
            if (levelNumber == 11 || cameraZoom > 1)
                camera.Position = level.Player.Position;
            else
                camera.Position = new Vector2(600, 330);
            camera.UpdateMatrix(GraphicsDevice.Viewport);
            timeToSkip += (float)gameTime.ElapsedGameTime.TotalMinutes;
            if (timeToSkip >= 5)
                level.Player.AllowSkip();
            if (level.Player.IsSkipped)
                LoadNextLevel();
            level.Update(gameTime);
            if (level.Player.IsSoulAlive && bgm.State is SoundState.Playing)
                bgm.Pause();
            else if (!level.Player.IsSoulAlive && bgm.State is SoundState.Paused)
                bgm.Resume();
            if (level.ReachedExit)
                LoadNextLevel();
            if (level.Player.IsDead)
            {
                if (timer == 0)
                {
                    deathSound.Play();
                    deathCounter++;
                    deathInCurrRun++;
                }
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timer >= 3.5f)
                {
                    deathSound.Stop();
                    timer = 0;
                    ReloadLevel();
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (IsEndingShow)
                return;
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, camera.Transform);

            if (levelNumber != 11)
                spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
            level.Draw(gameTime, spriteBatch);
            if (levelNumber == 0)
            {
                spriteBatch.DrawString(font, "Press \"Q\" to throw clone", new Vector2(120, 70), Color.White);
                spriteBatch.DrawString(font, "Press \"R\" to restart level", new Vector2(120, 100), Color.White);
                spriteBatch.DrawString(font, "Press \"Down\" as clone to splash down (needs to press buttons)", new Vector2(100, 130), Color.White);
                spriteBatch.DrawString(font, "Press \"E\" to start climbing", new Vector2(720, 70), Color.White);
                spriteBatch.DrawString(font, "Press \"Space\" while holding \"E\" to climb up", new Vector2(720, 100), Color.White);
                spriteBatch.DrawString(font, "Press arrow keys to move", new Vector2(740, 130), Color.White);
                spriteBatch.DrawString(font, "Press \"Enter\" to skip hard level (avialable after 5 minutes of attempts to pass)", new Vector2(300, 160), Color.White);
            }
            if (levelNumber == 7)
                spriteBatch.DrawString(font, "The spikes are smaller than they seem...", new Vector2(420, 70), Color.White);
            if (levelNumber != 11)
                spriteBatch.Draw(blackRectangle,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), dimColor);
            else
                DrawFinalLevel();
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawFinalLevel()
        {
            spriteBatch.DrawString(font, "Oh, you got here.", new Vector2(60, 570), Color.White);
            spriteBatch.DrawString(font, "Are you surprised to be here?", new Vector2(660, 570), Color.White);
            spriteBatch.DrawString(font, "Now you wanna know, what happens, right?", new Vector2(1200, 420), Color.White);
            spriteBatch.DrawString(font, string.Format("So, you died already {0} times.", deathCounter), new Vector2(600, 420), Color.White);
            spriteBatch.DrawString(font, "Honestly, that's all just for my fun.", new Vector2(60, 300), Color.White);
            spriteBatch.DrawString(font, "And what about you?", new Vector2(660, 300), Color.White);
            spriteBatch.DrawString(font, "You're just my doll, and you'll be here forewer, beating this again and again", new Vector2(80, 150), Color.White);
            if ((loopCounter > 1 && loopCounter < 5) && deathInCurrRun > 1)
                spriteBatch.DrawString(font, "Just say me, why you do it again and again? Do you like to suffer so much, endlessly overcoming the same thing?", new Vector2(60, 80), Color.White);
            else if ((loopCounter > 4) && deathInCurrRun > 1)
                spriteBatch.DrawString(font, "Why you beat this so much time?", new Vector2(60, 80), Color.White);
            else if (deathInCurrRun == 1 && cameraZoom < 5)
                spriteBatch.DrawString(font, "0 death in this loop? Hah, let's do it some more interesting...", new Vector2(60, 80), Color.White);
            else if (deathInCurrRun == 1 && cameraZoom > 4)
                spriteBatch.DrawString(font, "Are you okay? So, if you're so crazy, let's see, how deep you can go", new Vector2(60, 80), Color.White);
            spriteBatch.DrawString(font, string.Format("So let's do our {0} loop. See you at the next end:)", loopCounter + 1), new Vector2(1200, 80), Color.White);
            spriteBatch.Draw(blackRectangle,
                new Rectangle(0, 0, 1800, GraphicsDevice.Viewport.Height), finalLevelDim);
        }

        private void DrawName()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(blackRectangle,
                new Rectangle(0, 0, 1800, GraphicsDevice.Viewport.Height), new Color(0, 0, 0, 256));
            var textSize = font.MeasureString("Sorrow Outside Unending Loop") * 1.5f;
            var position = new Vector2((GraphicsDevice.Viewport.Width - textSize.X) / 2, (GraphicsDevice.Viewport.Height - textSize.Y) / 2);
            spriteBatch.DrawString(font, "Sorrow Outside Unending Loop", position, Color.White, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
            spriteBatch.End();
        }
    }
}
