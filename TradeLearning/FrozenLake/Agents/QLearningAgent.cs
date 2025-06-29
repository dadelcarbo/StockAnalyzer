using System.Diagnostics;
using System.Windows;

namespace FrozenLake.Agents
{
    /// <summary>
    /// <para>Basic QLearning class which looks only one step ahead.</para>
    /// <see href="https://deeplizard.com/learn/video/mo96Nqlo1L8"/>
    /// </summary>
    public class QLearningAgent : AgentBase, ILearningAgent
    {
        public double[,][] Q { get; private set; }

        public override void Initialize(World world, Random random)
        {
            this.world = world;
            this.rnd = random;

            this.Q = new double[world.Size, world.Size][];

            for (int i = 0; i < world.Size; i++)
            {
                for (int j = 0; j < world.Size; j++)
                {
                    this.Q[i, j] = new double[4];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="epsilon">
        /// 0 Full exploration (random)
        /// 1 Full exploitation (100% policy)
        /// </param>
        public void Train(int nbEpisodes, double learningRate, double epsilon, double discountFactor, bool allowVisited)
        {
            for (int i = 0; i < nbEpisodes; i++)
            {
                var reward = TrainingIteration(learningRate, epsilon, discountFactor, allowVisited);
            }
        }

        public double TrainingIteration(double learningRate, double epsilon, double discountFactor, bool allowVisited)
        {
            world.Reset();

            bool done = false;
            int step = 0;
            int maxSteps = 100;
            double totalReward = 0;
            while (!done && ++step < maxSteps)
            {
                var state = new { x = X, y = Y };
                var move = EpsilonMove(epsilon, true);

                var reward = 0.0;
                if (move == MoveAction.None) // Stuck
                {
                    throw new Exception("Should never be stuck");
                }
                else
                {
                    switch (move)
                    {
                        case MoveAction.Left:
                            break;
                        case MoveAction.Right:
                            break;
                        case MoveAction.Up:
                            break;
                        case MoveAction.Down:
                            break;
                        case MoveAction.None:
                            break;
                        default:
                            break;
                    }
                    switch (world.Tiles[X, Y])
                    {
                        case Tile.Wall:
                            MessageBox.Show("Agent stepped into a wall");
                            break;
                        case Tile.Reward:
                            Debug.WriteLine($"Reward");
                            reward = 10;
                            done = true;
                            break;
                        case Tile.Punish:
                            Debug.WriteLine($"Punish");
                            reward = -10;
                            done = true;
                            break;
                        case Tile.Agent:
                            MessageBox.Show("Agent on itself, that's a bug");
                            break;
                        case Tile.Empty:
                            world.Tiles[X, Y] = Tile.Visited;
                            break;
                        case Tile.Visited:
                            // Debug.WriteLine($"Visited");
                            break;
                        default:
                            MessageBox.Show($"Agent on unsupported tile Type ${world.Tiles[X, Y]}");
                            break;
                    }
                }

                // Update QTable
                var action = (int)move;
                Q[state.x, state.y][action] = (1 - learningRate) * Q[state.x, state.y][action] + learningRate * (reward + discountFactor * Q[X, Y].Max());

                totalReward += reward;
            }

            return totalReward;
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
            bool useRadomMove = epsilon < 1 && rnd.NextDouble() > epsilon;
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
                    var bestMove = Q[X, Y].Select((value, index) => new { value, index }).OrderByDescending(x => x.value).ElementAt(i);

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