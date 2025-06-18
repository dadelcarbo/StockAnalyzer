namespace FrozenLake.Agents
{
    internal class LearningAgent : IAgent
    {
        Random rnd = new Random(0);

        public int X { get; set; }
        public int Y { get; set; }


        public double[,][] Policy { get; private set; }
        public double[,] Value { get; private set; }


        private World world;
        public World World { get => world; private set => world = value; }

        public void Initialize(World world)
        {
            this.World = world;
            this.Policy = new double[world.Size, world.Size][];
            this.Value = new double[world.Size, world.Size];

            for (int i = 0; i < world.Size; i++)
            {
                for (int j = 0; j < world.Size; j++)
                {
                    var probabilities = new double[4];
                    double sum = probabilities[0] = rnd.NextDouble();
                    sum += probabilities[1] = rnd.NextDouble();
                    sum += probabilities[2] = rnd.NextDouble();
                    sum += probabilities[3] = rnd.NextDouble();

                    probabilities[0] /= sum;
                    probabilities[1] /= sum;
                    probabilities[2] /= sum;
                    probabilities[3] /= sum;

                    this.Policy[i, j] = probabilities;
                    this.Value[i, j] = rnd.NextDouble();
                }
            }
        }

        public MoveAction Move()
        {
            MoveAction move = MoveAction.None;
            int i = 0;
            while (move == MoveAction.None && i < 4)
            {
                move = (MoveAction)Policy[X, Y]
                    .Select((value, index) => new { value, index })
                    .OrderByDescending(x => x.value).ElementAt(i).index;
                i++;
                switch (move)
                {
                    case MoveAction.Left:
                        if (world.CanMove(X - 1, Y))
                        {
                            X--;
                            world.Tiles[X, Y] = Tile.Visited;
                        }
                        else
                            move = MoveAction.None;
                        break;
                    case MoveAction.Right:
                        if (world.CanMove(X + 1, Y))
                        {
                            X++;
                            world.Tiles[X, Y] = Tile.Visited;
                        }
                        else move = MoveAction.None;
                        break;
                    case MoveAction.Up:
                        if (world.CanMove(X, Y - 1))
                        {
                            Y--;
                            world.Tiles[X, Y] = Tile.Visited;
                        }
                        else move = MoveAction.None;
                        break;
                    case MoveAction.Down:
                        if (world.CanMove(X, Y + 1))
                        {
                            Y++;
                            world.Tiles[X, Y] = Tile.Visited;
                        }
                        else move = MoveAction.None;
                        break;
                    default:
                        break;
                }
            }
            if (move == MoveAction.None)
            {
                throw new InvalidOperationException("No move found");
            }
            return move;
        }
    }
}