using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using game.Tiles;
using game.Entities;
using game.Extensions;


namespace game.LevelInfo
{
    public class Level
    {
        private Tile[,] tiles;
        private Texture2D[] layers;
        private const int EntityLayer = 2;
        private Vector2 start;
        private Point exit;
        Player player;
        bool reachedExit;
        ContentManager content;
        private Dictionary<Vector2, Button> buttons = new Dictionary<Vector2, Button>();
        public List<Button> ButtonsToUpdate = new List<Button>();
        public List<Door> DoorsToUpdate = new List<Door>();
        private List<Vector2> buttonsToLoad = new List<Vector2>();
        private HashSet<Vector2> doorsPositions = new HashSet<Vector2>();
        public Player Player { get => player; }

        public int Width { get => tiles.GetLength(0); }

        public int ScreenHeight { get => tiles.GetLength(1) - 2; }

        public bool ReachedExit { get => reachedExit; }

        public ContentManager Content { get => content; }

        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            content = new ContentManager(serviceProvider, "Content");
            //layers = new Texture2D[2];
            //for (var i = 0; i < layers.Length; i++)
            //{
            //    layers[i] = content.Load<Texture2D>("Tiles")
            //}
            LoadTiles(fileStream);
        }

        private void LoadTiles(Stream fileStream)
        {
            var width = 0;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                var line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new ArgumentException("This map isn't correct: previous line has another length.");
                    line = reader.ReadLine();
                }
            }
            tiles = new Tile[width, lines.Count];
            for (var y = 0; y < ScreenHeight + 2; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }
            ConnectButtonsWithDoors();
        }

        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                case 'S':
                    return LoadTile("Spike", TypeOfTile.Spike);
                case 'D':
                    return new Tile(null, TypeOfTile.Door);
                case 'B':
                    return LoadButtonTile(x, y);
                case '.':
                    return new Tile(null, TypeOfTile.Passable);
                case 'E':
                    return LoadExit(x, y);
                case 'P':
                    return LoadStart(x, y);
                case '-':
                    return LoadTile("Platform", TypeOfTile.Platform);
                case '#':
                    return LoadTile("Wall", TypeOfTile.Impassable);
                default:
                    throw new ArgumentException("Unknown tile.");
            }
        }

        private Tile LoadButtonTile(int x, int y)
        {
            var pos = new Vector2(x, y);
            buttonsToLoad.Add(pos);
            return new Tile(null, TypeOfTile.Button);
        }

        private void ConnectButtonsWithDoors()
        {
            var prevPos = new Vector2(0, 0);
            foreach (var pos in buttonsToLoad)
            {
                Door door;
                var doorPos = FindDoor(pos);
                if (!doorsPositions.Contains(doorPos))
                {
                    door = new Door(doorPos, Content.Load<Texture2D>("Tiles/Door"), this);
                    doorsPositions.Add(doorPos);
                }
                else
                    door = buttons[prevPos].DependentDoor;
                var texture = Content.Load<Texture2D>("Tiles/Button");
                var currButton = new Button(pos, this, texture, door);
                door.ParentButtons.Add(currButton);
                buttons.Add(pos, currButton);
                prevPos = pos;
            }
        }

        private Vector2 FindDoor(Vector2 button)
        {
            var door = new Vector2(-1, -1);
            var visited = new HashSet<Vector2>();
            var queue = new Queue<Vector2>();
            queue.Enqueue(button);
            visited.Add(button);
            while (queue.Count > 0)
            {
                var currPoint = queue.Dequeue();
                visited.Add(currPoint);
                if (HoldInBounds((int)currPoint.X, (int)currPoint.Y) is TypeOfTile.Door)
                {
                    door = new Vector2((int)currPoint.X, (int)currPoint.Y);
                    break;
                }
                foreach (var cell in GetNeighbours(currPoint))
                    if (!visited.Contains(cell))
                        queue.Enqueue(cell);
            }
            if (door.X == door.Y && door.Y == -1)
                throw new ArgumentException("There's a button but there aren't doors!");
            return door;
        }

        private IEnumerable<Vector2> GetNeighbours(Vector2 pos)
        {
            var directions = new Vector2[]
            {
                new Vector2(-1, 0),
                new Vector2(1, 0),
                new Vector2(0, -1),
                new Vector2(0, 1)
            };
            foreach (var dir in directions)
            {
                var newPos = new Vector2(pos.X + dir.X, pos.Y + dir.Y);
                if (CanMove(newPos))
                    yield return newPos;
            }
        }

        private bool CanMove(Vector2 pos)
        {
            return pos.X > 0 && pos.X < Width && pos.Y > 0 && pos.Y < ScreenHeight;
        }

        private Tile LoadTile(string name, TypeOfTile type)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), type);
        }

        private Tile LoadStart(int x, int y)
        {
            var playerTexture = Content.Load<Texture2D>("Tiles/Character");
            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            start.X -= 30;
            player = new Player(this, start, playerTexture);
            return new Tile(null, TypeOfTile.Passable);
        }

        private Tile LoadExit(int x, int y)
        {
            exit = GetBounds(x, y).Center;
            return new Tile(null, TypeOfTile.Passable);
        }

        public void Dispose()
        {
            Content.Unload();
        }

        public void RemoveCollision(int x, int y)
        {
            tiles[x, y] = new Tile(null, TypeOfTile.Passable);
        }

        public void SetCollision(int x, int y)
        {
            tiles[x, y] = new Tile(tiles[x, y].Texture, TypeOfTile.Impassable);
        }

        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        public TypeOfTile HoldInBounds(int x, int y)
        {
            if (x < 0 || x >= Width || y <= 0)
                return TypeOfTile.Impassable;
            if (y >= ScreenHeight)
                return TypeOfTile.Passable;
            return tiles[x, y].Collision;
        }

        public void Update(GameTime gameTime)
        {
            if (player.IsSoulAlive)
            {
                player.Soul.Update(gameTime);
                if (Player.Soul.BoundingRectangle.Top >= ScreenHeight * Tile.Height || player.Soul.IsDead)
                    Player.IsDead = true;
                return;
            }
            else
            {
                player.Soul.Position = player.Position;
                if (Player.IsOnGround)
                    Player.Stamina = 60;
                Player.Update(gameTime);
                if (Player.BoundingRectangle.Top >= ScreenHeight * Tile.Height)
                    Player.IsDead = true;
            }
            for (var i = 0; i < ButtonsToUpdate.Count; i++)
            {
                var oldLength = ButtonsToUpdate.Count;
                ButtonsToUpdate[i].Update(gameTime);
                if (oldLength != ButtonsToUpdate.Count)
                    i--;
            }
            for (var i = 0; i < DoorsToUpdate.Count; i++)
            {
                var oldLength = DoorsToUpdate.Count;
                DoorsToUpdate[i].Update(gameTime);
                if (oldLength != DoorsToUpdate.Count)
                    i--;
            }
            if (!Player.IsDead && Player.IsOnGround && Player.BoundingRectangle.Contains(exit))
                OnExitReached();
        }

        public void ButtonPressed(Vector2 buttonPlace, GameTime gameTime)
        {
            if (buttons[buttonPlace].IsPressed == false)
            {
                buttons[buttonPlace].PressButton(gameTime);
                ButtonsToUpdate.Add(buttons[buttonPlace]);
                var door = buttons[buttonPlace].DependentDoor;
                door.ParentButtons.Remove(buttons[buttonPlace]);
                if (door.ParentButtons.Count == 0)
                    DoorsToUpdate.Add(buttons[buttonPlace].DependentDoor);
            }
        }

        private void OnExitReached()
        {
            reachedExit = true;
        }

        private void Restart() => Player.Reset(start);

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //for (var i = 0; i <= EntityLayer; i++)
            //    spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);
            foreach (var button in buttons.Values)
                button.Draw(spriteBatch);
            foreach (var key in buttons.Keys)
                buttons[key].DependentDoor.Draw(spriteBatch);
            DrawTiles(spriteBatch);
            player.Draw(gameTime, spriteBatch);
            if (player.IsSoulAlive)
                player.Soul.Draw(gameTime, spriteBatch);
            //for (int i = EntityLayer + 1; i < layers.Length; ++i)
            //    spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);
        }

        private void DrawTiles(SpriteBatch spriteBatch)
        {
            for (var y = 0; y < ScreenHeight; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }
    }
}
