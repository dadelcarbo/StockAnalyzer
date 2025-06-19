using System.Diagnostics;
using System.Windows;

namespace FrozenLake.Agents
{
    class PathItem
    {
        public int X;
        public int Y;
        public MoveAction OutgoingMove;
    }
    public class LearningAgent : AgentBase
    {
        public double[,][] Policy { get; private set; }
        public double[,] Value { get; private set; }

        public override void Initialize(World world, Random random)
        {
            this.world = world;
            this.rnd = random;

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
                    //this.Value[i, j] = rnd.NextDouble();
                }
            }

        }

        List<PathItem> path = [];
        private void TrainPPO()
        {
            // Update policy to prevent invalid moves
            for (int i = 0; i < world.Size; i++)
            {
                for (int j = 0; j < world.Size; j++)
                {
                    double sum = 0;
                    int invalidMoves = 0;
                    foreach (var move in new[] { MoveAction.Left, MoveAction.Right, MoveAction.Up, MoveAction.Down })
                    {
                        switch (move)
                        {
                            case MoveAction.Left:
                                if (!world.CanMove(i - 1, j))
                                {
                                    Policy[i, j][(int)move] = 0;
                                    invalidMoves++;
                                }
                                break;
                            case MoveAction.Right:
                                if (!world.CanMove(i + 1, j))
                                {
                                    Policy[i, j][(int)move] = 0;
                                    invalidMoves++;
                                }
                                break;
                            case MoveAction.Up:
                                if (!world.CanMove(i, j - 1))
                                {
                                    Policy[i, j][(int)move] = 0;
                                    invalidMoves++;
                                }
                                break;
                            case MoveAction.Down:
                                if (!world.CanMove(i - 1, j + 1))
                                {
                                    Policy[i, j][(int)move] = 0;
                                    invalidMoves++;
                                }
                                break;
                        }

                        sum += Policy[i, j][(int)move];
                    }
                    // Normalize policy

                    for (int k = 0; k < 4; k++)
                    {
                        Policy[i, j][k] /= sum;
                    }
                }
            }

            SetRandomLocation();

            int iteration = 0;
            double error;
            do
            {
                error = TrainingIteration();
                Debug.WriteLine($"Iteration: {iteration} Error: {error}");
            }
            while (++iteration < 1000 && error > 0.001);
        }

        public double TrainingIteration()
        {
            path.Clear();
            world.Reset();

            bool pathComplete = false;
            double actualValue = 0;
            path.Clear();
            path.Add(new PathItem { X = this.X, Y = this.Y, OutgoingMove = MoveAction.None });
            while (!pathComplete)
            {
                var move = Move();
                if (move == MoveAction.None) // Stuck
                {
                    pathComplete = true;
                    actualValue = -1;
                }
                else
                {
                    switch (world.Tiles[X, Y])
                    {
                        case Tile.Wall:
                            MessageBox.Show("Agent stepped into a wall");
                            break;
                        case Tile.Reward:
                            Debug.WriteLine($"Reward");
                            actualValue = 10;
                            AddMove(X, Y, move);
                            pathComplete = true;
                            break;
                        case Tile.Punish:
                            Debug.WriteLine($"Punish");
                            actualValue = -10;
                            AddMove(X, Y, move);
                            pathComplete = true;
                            break;
                        case Tile.Agent:
                            MessageBox.Show("Agent on itself, that's a bug");
                            break;
                        case Tile.Empty:
                            AddMove(X, Y, move);
                            world.Tiles[X, Y] = Tile.Visited;
                            break;
                        case Tile.Visited:
                            Debug.WriteLine($"Visited");
                            AddMove(X, Y, move);
                            break;
                        default:
                            MessageBox.Show($"Agent on unsupported tile Type ${world.Tiles[X, Y]}");
                            break;
                    }
                }

            }

            // Squared error
            double totalError = 0;
            double error = 0;
            double decay = 0.1;
            int count = 0;
            double learningRate = 0.1;
            foreach (var pos in path)
            {
                var discountedValue = (actualValue * (1 - decay * count));
                // Update value
                error = discountedValue - Value[pos.X, pos.Y];
                totalError += error * error;

                Value[pos.X, pos.Y] += error * learningRate;

                // Update Policy
                if (pos.OutgoingMove != MoveAction.None)
                {
                    Policy[pos.X, pos.Y][(int)pos.OutgoingMove] += error * learningRate;
                    Policy[pos.X, pos.Y].NormalizeNonZero();
                }
                count++;
            }

            totalError /= path.Count;
            return totalError;
        }


        private void AddMove(int x, int y, MoveAction move)
        {
            if (path.Count != 0)
            {
                var pathItem = path.First();
                pathItem.OutgoingMove = move;
            }
            path.Insert(0, new PathItem { X = x, Y = y, OutgoingMove = MoveAction.None });
        }

        public override MoveAction Move()
        {
            MoveAction move = MoveAction.None;
            int i = 0;
            bool allowVisited = false;
            while (move == MoveAction.None && i < 4)
            {
                var bestMove = Policy[X, Y]
                    .Select((value, index) => new { value, index })
                    .OrderByDescending(x => x.value).ElementAt(i);
                if (bestMove.value == 0)
                    break;

                i++;
                move = (MoveAction)bestMove.index;
                switch (move)
                {
                    case MoveAction.Left:
                        if (world.CanMove(X - 1, Y, allowVisited))
                        {
                            X--;
                        }
                        else
                            move = MoveAction.None;
                        break;
                    case MoveAction.Right:
                        if (world.CanMove(X + 1, Y, allowVisited))
                        {
                            X++;
                        }
                        else move = MoveAction.None;
                        break;
                    case MoveAction.Up:
                        if (world.CanMove(X, Y - 1, allowVisited))
                        {
                            Y--;
                        }
                        else move = MoveAction.None;
                        break;
                    case MoveAction.Down:
                        if (world.CanMove(X, Y + 1, allowVisited))
                        {
                            Y++;
                        }
                        else move = MoveAction.None;
                        break;
                    default:
                        break;
                }
            }
            return move;
        }
    }
}