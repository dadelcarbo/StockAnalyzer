using System.Diagnostics;
using Tensorflow.Keras.Engine;
using static Tensorflow.Binding;


namespace FrozenLake.Agents
{
    public class LearningNNAgent : AgentBase, ILearningAgent
    {
        IModel policyNetwork;
        IModel valueNetwork;

        public override void Initialize(World world, Random random)
        {
            this.world = world;
            this.rnd = random;

            tf.enable_eager_execution();

            policyNetwork = CreatePolicyNetwork();
            valueNetwork = CreateValueNetwork();

        }

        private static IModel CreatePolicyNetwork()
        {

            // Define input shape (e.g., 2 continuous state variables)
            var input = tf.keras.Input(shape: new Tensorflow.Shape(2));

            // Hidden layers
            var dense1 = tf.keras.layers.Dense(64, activation: (Tensorflow.Keras.Activation)tf.nn.relu).Apply(input);
            var dense2 = tf.keras.layers.Dense(64, activation: (Tensorflow.Keras.Activation)tf.nn.relu).Apply(dense1);

            // Output layer for discrete actions (e.g., 4 actions)
            var output = tf.keras.layers.Dense(4, activation: "softmax").Apply(dense2);

            // Build model
            var model = tf.keras.Model(input, output);
            model.summary();

            // Compile model
            model.compile(optimizer: tf.keras.optimizers.Adam(learning_rate: 0.001f),
                          loss: tf.keras.losses.SparseCategoricalCrossentropy(),
                          metrics: new[] { "accuracy" });
            return model;
        }
        private static IModel CreateValueNetwork()
        {

            // Define input shape (e.g., 2 continuous state variables)
            var input = tf.keras.Input(shape: new Tensorflow.Shape(2));

            // Hidden layers
            var dense1 = tf.keras.layers.Dense(64, activation: (Tensorflow.Keras.Activation)tf.nn.relu).Apply(input);
            var dense2 = tf.keras.layers.Dense(64, activation: (Tensorflow.Keras.Activation)tf.nn.relu).Apply(dense1);

            // Output layer for values (e.g., 1 value)
            var output = tf.keras.layers.Dense(1).Apply(dense2);

            // Build model
            var model = tf.keras.Model(input, output);
            model.summary();

            // Compile model
            model.compile(optimizer: tf.keras.optimizers.Adam(learning_rate: 0.001f),
                          loss: tf.keras.losses.SparseCategoricalCrossentropy(),
                          metrics: new[] { "accuracy" });
            return model;
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
                for (int i = 0; i < world.Size; i++)
                {
                    for (int j = 0; j < world.Size; j++)
                    {
                        if (world.Tiles[i, j] != Tile.Empty)
                            continue;

                        this.X = i; this.Y = j;

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

            return 0;
        }

        public static float[] EncodeState(Tile[,] grid, int agentRow, int agentCol)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);
            int tileTypes = Enum.GetValues(typeof(Tile)).Length;

            // Encode the grid layout (one-hot per tile)
            float[] gridEncoding = new float[rows * cols * tileTypes];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int tileIndex = (int)grid[r, c];
                    int flatIndex = (r * cols + c) * tileTypes + tileIndex;
                    gridEncoding[flatIndex] = 1f;
                }
            }

            // Encode the agent's position as a one-hot vector
            float[] agentEncoding = new float[rows * cols];
            int agentIndex = agentRow * cols + agentCol;
            agentEncoding[agentIndex] = 1f;

            // Combine both encodings
            return gridEncoding.Concat(agentEncoding).ToArray();
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

        private float[] EvaluatePolicy(int x, int y)
        {
            Tensorflow.NumPy.NDArray input = Tensorflow.NumPy.np.array(new float[,] { { 0.5f, -0.2f } });

            var output = policyNetwork.predict(input).Single.numpy();

            return output.ElementAt(0).ToArray<float>();
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

            var policy = EvaluatePolicy(X, Y).Select((value, index) => new { value, index }).OrderByDescending(x => x.value).ToArray();
            while (move == MoveAction.None && i < 4)
            {
                // Select next move
                if (useRadomMove) // exploration
                {
                    move = randomMoves[i];
                }
                else // exploitation
                {
                    var bestMove = policy[i];
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