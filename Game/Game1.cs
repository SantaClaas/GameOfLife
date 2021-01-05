using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGaymy
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch? _spriteBatch;
        private Texture2D _textureAlive;
        private Texture2D _textureDead;
        private bool[,] _grid;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(3);
        private DateTimeOffset _timeStampLastUpdate = DateTimeOffset.Now;
        /// <summary>
        /// Cell size in pixel
        /// </summary>
        private const int CellSize = 30;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _textureAlive = new Texture2D(GraphicsDevice, CellSize, CellSize);
            var color = Color.White;
            var colorData = Enumerable.Repeat(color, CellSize * CellSize).ToArray();
            _textureAlive.SetData(colorData);

            _textureDead = new Texture2D(GraphicsDevice, CellSize, CellSize);
            color = Color.Black;
            colorData = Enumerable.Repeat(color, CellSize * CellSize).ToArray();
            _textureDead.SetData(colorData);

            // grid has to be as big as the window but 
            // horizontal
            //TODO overlapping / cell size does not fit since we have a remainder
            var xCellCount = GraphicsDevice.Viewport.Width / CellSize;
            // vertical
            var yCellCount = GraphicsDevice.Viewport.Height / CellSize;

            _grid = new bool[xCellCount, yCellCount];

            Random rng = new();
            for (var x = 0; x < _grid.GetLength(0); x++)
                for (var y = 0; y < _grid.GetLength(1); y++)
                {
                    _grid[x, y] = rng.Next(0, 2) == 1;
                }



            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var now = DateTimeOffset.Now;
            if (now - _timeStampLastUpdate > _updateInterval)
            {
                // TODO: Add your update logic here
                var updatedGrid = (bool[,])_grid.Clone();
                for (var x = 0; x < _grid.GetLength(0); x++)
                    for (var y = 0; y < _grid.GetLength(1); y++)
                    {
                        var countNeighbors = _grid.GetCountNeighbors((x, y), cell => cell);
                        var isAlive = _grid[x, y];
                        updatedGrid[x, y] = countNeighbors is 3 || countNeighbors is 2 && isAlive;
                    }

                _grid = updatedGrid;
                _timeStampLastUpdate = now;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            // TODO: Add your drawing code here
            _spriteBatch!.Begin();
            for (var x = 0; x < _grid.GetLength(0); x++)
                for (var y = 0; y < _grid.GetLength(1); y++)
                {
                    var texture = _grid[x, y] ? _textureAlive : _textureDead;
                    // calculate draw position
                    Vector2 drawPosition = new(x * CellSize, y * CellSize);
                    _spriteBatch.Draw(texture, drawPosition, Color.White);
                }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

    }

    // kinda my grid extensions
    // I swear I didn't intend to write functional code with C# (functional as in paradigm an adjective 🤭)
    internal static class MultiDimensionalArrayExtensions
    {
        /// <summary>
        /// Gets the value at <paramref name="direction"/> relative from the <paramref name="origin"/>
        /// </summary>
        internal static T GetRelative<T>(this T[,] grid, (int X, int Y) origin, Direction direction) =>
            CanReach(grid, origin, direction)
                ? direction switch
                {
                    Direction.North => grid[origin.X, origin.Y - 1],
                    Direction.West => grid[origin.X - 1, origin.Y],
                    Direction.South => grid[origin.X, origin.Y + 1],
                    Direction.East => grid[origin.X + 1, origin.Y],
                    Direction.NorthWest => grid[origin.X - 1, origin.Y - 1],
                    Direction.SouthWest => grid[origin.X - 1, origin.Y + 1],
                    Direction.SouthEast => grid[origin.X + 1, origin.Y + 1],
                    Direction.NorthEast => grid[origin.X + 1, origin.Y - 1],
                    _ => throw new NotSupportedException()
                }
                : throw new ArgumentOutOfRangeException(nameof(direction));


        private static bool CanReach<T>(this T[,] grid, (int X, int Y) origin, Direction direction)
            => grid.CanReach((origin, direction));
        private static bool CanReach<T>(this T[,] grid, ((int X, int Y) Origin, Direction Direction) tuple)
            => tuple is not (
        {
            Origin: { X: <= 0 },
            Direction: Direction.West
                    or Direction.NorthWest
                    or Direction.SouthWest
        }
                or
        {
            Origin: { Y: <= 0 },
            Direction: Direction.North
                    or Direction.NorthWest
                    or Direction.NorthEast
        }) && (tuple.Origin.X + 1 < grid.GetLength(0) || tuple.Direction is not (Direction.East or Direction.SouthEast or Direction.NorthEast)) && (tuple.Origin.Y + 1 < grid.GetLength(1) || tuple.Direction is not (Direction.South or Direction.SouthEast or Direction.SouthWest));

        internal static int GetCountNeighbors<T>(this T[,] grid, (int X, int Y) origin, Func<T, bool> isNeighbor)
            => Enum.GetValues<Direction>()
                .Where(d => grid.CanReach(origin, d))
                .Select(direction => grid.GetRelative(origin, direction))
                .Count(isNeighbor);
    }

    internal enum Direction
    {
        North,
        East,
        South,
        West,
        NorthEast,
        SouthEast,
        NorthWest,
        SouthWest

    }
}