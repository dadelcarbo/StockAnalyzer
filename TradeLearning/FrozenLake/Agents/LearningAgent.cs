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
    public class LearningAgent : AgentBase, ILearningAgent
    {
        public double[,][] Policy { get; private set; }
        public double[,] Value { get; private set; }

        public override void Initialize(World world, Random random)
        {
            this.world = world;
            this.rnd = random;

            this.Policy = new double[world.Size.Width, world.Size.Height][];
            this.Value = new double[world.Size.Width, world.Size.Height];

            for (int i = 0; i < world.Size.Width; i++)
            {
                for (int j = 0; j < world.Size.Height; j++)
                {
                    var probabilities = new double[4];
                    //probabilities[0] = rnd.NextDouble();
                    //probabilities[1] = rnd.NextDouble();
                    //probabilities[2] = rnd.NextDouble();
                    //probabilities[3] = rnd.NextDouble();
                    probabilities[0] = 1;
                    probabilities[1] = 1;
                    probabilities[2] = 1;
                    probabilities[3] = 1;
                    probabilities.Normalize();

                    this.Policy[i, j] = probabilities;

                    //this.Value[i, j] = 1 - rnd.NextDouble() * 2;
                }
            }

            // Update policy to prevent invalid moves
            for (int x = 0; x < world.Size.Width; x++)
            {
                for (int y = 0; y < world.Size.Height; y++)
                {
                    if (world.Tiles(x, y) == Tile.Wall)
                        continue;

                    double sum = 0;
                    foreach (var move in new[] { MoveAction.Left, MoveAction.Right, MoveAction.Up, MoveAction.Down })
                    {
                        switch (move)
                        {
                            case MoveAction.Left:
                                if (!world.CanMove(x - 1, y))
                                {
                                    Policy[x, y][(int)move] = 0;
                                }
                                break;
                            case MoveAction.Right:
                                if (!world.CanMove(x + 1, y))
                                {
                                    Policy[x, y][(int)move] = 0;
                                }
                                break;
                            case MoveAction.Up:
                                if (!world.CanMove(x, y - 1))
                                {
                                    Policy[x, y][(int)move] = 0;
                                }
                                break;
                            case MoveAction.Down:
                                if (!world.CanMove(x, y + 1))
                                {
                                    Policy[x, y][(int)move] = 0;
                                }
                                break;
                        }

                        sum += Policy[x, y][(int)move];
                    }
                    // Normalize policy
                    Policy[x, y].NormalizeNonZero();
                }
            }

        }

        List<PathItem> path = [];
        /// <summary>
        /// 
        /// </summary>
        /// <param name="epsilon">
        /// 0 Full exploration (random)
        /// 1 Full exploitation (100% policy)
        /// </param>
        public void Train(int nbEpisodes, double learningRate, double epsilon, double discountFactor, bool allowVisited)
        {
            int iteration = 0;
            double error;
            do
            {
                error = 0;
                for (int x = 0; x < world.Size.Width; x++)
                {
                    for (int y = 0; y < world.Size.Height; y++)
                    {
                        if (world.Tiles(x, y) != Tile.Empty)
                            continue;

                        this.X = x; this.Y = y;

                        error += TrainingIteration(learningRate, epsilon, discountFactor, allowVisited);
                    }
                }
                Debug.WriteLine($"Iteration: {iteration} Error: {error}");
            }
            while (++iteration < nbEpisodes && error > 0.001);
        }

        public double TrainingIteration(double learningRate, double epsilon, double discountFactor, bool allowVisited)
        {
            path.Clear();
            world.Reset();

            bool pathComplete = false;
            double actualValue = 0;
            path.Clear();
            path.Add(new PathItem { X = this.X, Y = this.Y, OutgoingMove = MoveAction.None });
            while (!pathComplete)
            {
                //Debug.Write($"X:{X}, Y:{Y}");
                var move = EpsilonMove(epsilon, allowVisited);
                //Debug.WriteLine($" {move} => X:{X}, Y:{Y}");
                if (move == MoveAction.None) // Stuck
                {
                    pathComplete = true;
                    actualValue = -2;
                    Debug.WriteLine($"Stuck");
                }
                else
                {
                    switch (world.Tiles(X, Y))
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
                        case Tile.Empty:
                            AddMove(X, Y, move);
                            world.SetVisited(X, Y);
                            break;
                        case Tile.Visited:
                            // Debug.WriteLine($"Visited");
                            AddMove(X, Y, move);
                            break;
                        default:
                            MessageBox.Show($"Agent on unsupported tile Type ${world.Tiles(X, Y)}");
                            break;
                    }
                }

            }

            // Squared error
            double totalError = 0;
            double error = 0;
            double decay = 1;
            foreach (var pos in path)
            {
                var discountedValue = (actualValue * decay);
                decay *= discountFactor;
                // Update value
                error = discountedValue - Value[pos.X, pos.Y];
                totalError += error * error;

                Value[pos.X, pos.Y] += error * learningRate;

                // Update Policy
                if (pos.OutgoingMove != MoveAction.None)
                {
                    var newPoliciyVal = Math.Max(0, Policy[pos.X, pos.Y][(int)pos.OutgoingMove] + error * learningRate);
                    Policy[pos.X, pos.Y][(int)pos.OutgoingMove] = newPoliciyVal;
                    Policy[pos.X, pos.Y].NormalizeNonZero();
                }
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


        public override MoveAction Move(bool allowVisited)
        {
            return EpsilonMove(1, allowVisited);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="epsilon"></param>
        /// 0 Full exploration (random)
        /// 1 Full exploitation (100% policy)
        /// <returns></returns>
        public MoveAction EpsilonMove(double epsilon, bool allowVisited)
        {
            MoveAction move = MoveAction.None;
            int i = 0;
            bool useRadomMove = rnd.NextDouble() > epsilon;
            MoveAction[] randomMoves = null;
            if (useRadomMove)
            {
                randomMoves = GetRandomMoves();
            }

            while (move == MoveAction.None && i < 4)
            {
                // Select next move
                if (useRadomMove) // exploration
                {
                    move = randomMoves[i];
                }
                else // exploitation
                {
                    var bestMove = Policy[X, Y].Select((value, index) => new { value, index }).OrderByDescending(x => x.value).ElementAt(i);
                    if (bestMove.value == 0)
                        break;

                    move = (MoveAction)bestMove.index;
                }
                i++;
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