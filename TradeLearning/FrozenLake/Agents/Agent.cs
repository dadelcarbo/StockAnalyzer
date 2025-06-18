namespace FrozenLake.Agents
{
    public enum MoveAction
    {
        Left = 0,
        Right = 1,
        Up = 2,
        Down = 3,
        None,
        Completed
    };

    internal class Agent : IAgent
    {
        Random rnd = new Random(0);

        public int X { get; set; }
        public int Y { get; set; }

        private World world;
        public World World { get => world; private set => world = value; }

        public void Initialize(World world)
        {
            this.World = world;
        }

        public MoveAction Move()
        {
            MoveAction move = MoveAction.None;
            while (move == MoveAction.None)
            {
                move = (MoveAction)rnd.Next(1, 5);
                switch (move)
                {
                    case MoveAction.Left:
                        if (world.CanMove(X - 1, Y, true))
                        {
                            X--;
                            world.Tiles[X, Y] = Tile.Visited;
                        }
                        else
                            move = MoveAction.None;
                        break;
                    case MoveAction.Right:
                        if (world.CanMove(X + 1, Y, true))
                        {
                            X++;
                            world.Tiles[X, Y] = Tile.Visited;
                        }
                        else move = MoveAction.None;
                        break;
                    case MoveAction.Up:
                        if (world.CanMove(X, Y - 1, true))
                        {
                            Y--;
                            world.Tiles[X, Y] = Tile.Visited;
                        }
                        else move = MoveAction.None;
                        break;
                    case MoveAction.Down:
                        if (world.CanMove(X, Y + 1, true))
                        {
                            Y++;
                            world.Tiles[X, Y] = Tile.Visited;
                        }
                        else move = MoveAction.None;
                        break;
                }
            }
            return move;
        }
    }
}