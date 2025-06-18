namespace FrozenLake.Agents
{
    internal class GreedyAgent : IAgent
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
                        if (world.CanMove(X - 1, Y, false))
                        {
                            X--;
                            world.Tiles[X, Y] = Tile.Visited;
                        }
                        else
                            move = MoveAction.None;
                        break;
                    case MoveAction.Right:
                        if (world.CanMove(X + 1, Y, false))
                        {
                            X++;
                            world.Tiles[X, Y] = Tile.Visited;
                        }
                        else move = MoveAction.None;
                        break;
                    case MoveAction.Up:
                        if (world.CanMove(X, Y - 1, false))
                        {
                            Y--;
                            world.Tiles[X, Y] = Tile.Visited;
                        }
                        else move = MoveAction.None;
                        break;
                    case MoveAction.Down:
                        if (world.CanMove(X, Y + 1, false))
                        {
                            Y++;
                            world.Tiles[X, Y] = Tile.Visited;
                        }
                        else move = MoveAction.None;
                        break;
                    default:
                        break;
                }
                if (move == MoveAction.None)
                {
                    if (world.CanMove(X - 1, Y, false))
                    {
                        X--;
                        world.Tiles[X, Y] = Tile.Visited;
                        move = MoveAction.Left;
                    }
                    else if (world.CanMove(X + 1, Y, false))
                    {
                        X++;
                        world.Tiles[X, Y] = Tile.Visited;
                        move = MoveAction.Right;
                    }
                    else if (world.CanMove(X, Y - 1, false))
                    {
                        Y--;
                        world.Tiles[X, Y] = Tile.Visited;
                        move = MoveAction.Down;
                    }
                    else if (world.CanMove(X, Y + 1, false))
                    {
                        Y++;
                        world.Tiles[X, Y] = Tile.Visited;
                        move = MoveAction.Up;
                    }
                    else
                    {
                        bool found = false;
                        for (int i = 0; !found && i < world.Size; i++)
                        {
                            for (int j = 0; !found && j < world.Size; j++)
                            {
                                if (world.CanMove(i, j, false))
                                {
                                    found = true;
                                    X = i;
                                    Y = j;
                                    world.Tiles[X, Y] = Tile.Visited;
                                    move = MoveAction.Left;
                                    break;
                                }
                            }
                        }
                        if (!found)
                            return MoveAction.Completed;
                    }
                }
            }
            return move;
        }
    }
}