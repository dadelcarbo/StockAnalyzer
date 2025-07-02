namespace FrozenLake.Agents
{
    public abstract class AgentBase : IAgent
    {
        public int X { get; set; }
        public int Y { get; set; }

        protected World world;
        public World World { get => world; private set => world = value; }

        public string Name => this.GetType().Name;

        protected Random rnd;
        public virtual void Initialize(World world, Random random)
        {
            this.World = world;
            this.rnd = random;
        }
        public abstract MoveAction Move(bool allowVisited);

        public void SetRandomLocation()
        {
            // Set agent to random valid location
            bool found = false;
            while (!found)
            {
                int x = rnd.Next(0, world.Size.Width);
                int y = rnd.Next(0, world.Size.Height);
                if (world.Tiles(x, y) == Tile.Empty)
                {
                    found = true;
                    this.X = x;
                    this.Y = y;
                }
            }
        }

        protected static MoveAction[] GetRandomMoves()
        {
            MoveAction[] randomMoves = [MoveAction.Left, MoveAction.Right, MoveAction.Up, MoveAction.Down];
            randomMoves.Shuffle();
            return randomMoves;
        }

    }
}
