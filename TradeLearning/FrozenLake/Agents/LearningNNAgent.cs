using System.Diagnostics;
using Tensorflow;
using Tensorflow.Keras.Engine;
using static Tensorflow.Binding;


namespace FrozenLake.Agents
{

    public class MyCallback : ICallback
    {
        public Dictionary<string, List<float>> history { get; set; }

        public void on_epoch_begin(int epoch)
        {
            Debug.WriteLine($"on_epoch_begin {epoch}");
        }

        public void on_epoch_end(int epoch, Dictionary<string, float> epoch_logs)
        {
            Debug.WriteLine($"on_epoch_end {epoch}");
        }

        public void on_predict_batch_begin(long step)
        {
            Debug.WriteLine($"on_predict_batch_begin {step}");
        }

        public void on_predict_batch_end(long end_step, Dictionary<string, Tensors> logs)
        {
            Debug.WriteLine($"on_predict_batch_end {end_step}");
        }

        public void on_predict_begin()
        {
            Debug.WriteLine($"on_predict_begin");
        }

        public void on_predict_end()
        {
            Debug.WriteLine($"on_predict_end");
        }

        public void on_test_batch_begin(long step)
        {
            Debug.WriteLine($"on_test_batch_begin {step}");
        }

        public void on_test_batch_end(long end_step, Dictionary<string, float> logs)
        {
            Debug.WriteLine($"on_test_batch_end {end_step}");
        }

        public void on_test_begin()
        {
            Debug.WriteLine($"on_test_begin");
        }

        public void on_test_end(Dictionary<string, float> logs)
        {
            Debug.WriteLine($"on_test_end");
        }

        public void on_train_batch_begin(long step)
        {
            Debug.WriteLine($"on_train_batch_begin {step}");
        }

        public void on_train_batch_end(long end_step, Dictionary<string, float> logs)
        {
            Debug.WriteLine($"on_train_batch_end {end_step}");
        }

        public void on_train_begin()
        {
            Debug.WriteLine($"on_train_begin");
        }

        public void on_train_end()
        {
            Debug.WriteLine($"on_train_end");
        }
    }

    public class LearningNNAgent : AgentBase, ILearningAgent
    {
        IModel policyNetwork;
        IModel valueNetwork;

        public (float Value, float[] Policy) GetValuePolicy(int x, int y)
        {
            var state = world.EncodeState(x, y);
            var value = EvaluateValue(state);
            var policy = EvaluatePolicy(state);

            return (value, policy);
        }

        public override void Initialize(World world, Random random)
        {
            this.world = world;
            this.rnd = random;

            tf.enable_eager_execution();

            policyNetwork = CreatePolicyNetwork(world.StateSize);
            valueNetwork = CreateValueNetwork(world.StateSize);

        }

        private static IModel CreatePolicyNetwork(int inputSize)
        {
            // Define input shape
            var input = tf.keras.Input(shape: new Tensorflow.Shape(inputSize));

            // Hidden layers
            var dense1 = tf.keras.layers.Dense(inputSize, activation: (Tensorflow.Keras.Activation)tf.nn.relu).Apply(input);
            var dense2 = tf.keras.layers.Dense(inputSize, activation: (Tensorflow.Keras.Activation)tf.nn.relu).Apply(dense1);

            // Output layer for discrete actions (e.g., 4 actions)
            var output = tf.keras.layers.Dense(4, activation: "softmax").Apply(dense2);

            // Build model
            var model = tf.keras.Model(input, output);
            model.summary();

            // Compile model
            model.compile(optimizer: tf.keras.optimizers.Adam(learning_rate: 0.001f),
                          loss: tf.keras.losses.CategoricalCrossentropy(),
                          metrics: new[] { "accuracy" });



            return model;
        }
        private static IModel CreateValueNetwork(int inputSize)
        {
            var input = tf.keras.Input(shape: new Tensorflow.Shape(inputSize));

            // Hidden layers
            var dense1 = tf.keras.layers.Dense(inputSize, activation: (Tensorflow.Keras.Activation)tf.nn.relu).Apply(input);
            var dense2 = tf.keras.layers.Dense(inputSize, activation: (Tensorflow.Keras.Activation)tf.nn.relu).Apply(dense1);

            // Output layer for values (e.g., 1 value)
            var output = tf.keras.layers.Dense(1).Apply(dense2);

            // Build model
            var model = tf.keras.Model(input, output);
            model.summary();

            // Compile model
            model.compile(optimizer: tf.keras.optimizers.Adam(learning_rate: 0.001f),
                          loss: tf.keras.losses.CategoricalCrossentropy(),
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
                List<(float[] State, float[] ActionValues)> batch = [];
                error = 0;
                for (int y = 0; y < world.Size.Height; y++)
                {
                    for (int x = 0; x < world.Size.Width; x++)
                    {
                        if (world.Tiles(x, y) != Tile.Empty)
                            continue;

                        var actionValues = new double[4];
                        foreach (var action in new[] { MoveAction.Left, MoveAction.Right, MoveAction.Up, MoveAction.Down })
                        {
                            actionValues[(int)action] = world.CanMove(x, y, action, true) ? 1 : 0;
                        }
                        actionValues.NormalizeNonZero();

                        batch.Add(new(world.EncodeState(x, y), actionValues.ToFloatArray()));
                    }
                }
                float[] flat = batch.SelectMany(b => b.State).ToArray();
                var states = Tensorflow.NumPy.np.array(flat).reshape(new Shape(1, batch.Count, world.StateSize));

                flat = batch.SelectMany(b => b.ActionValues).ToArray();
                var targets = Tensorflow.NumPy.np.array(flat).reshape(new Shape(1, batch.Count, 4));

                var callback = policyNetwork.fit(states, targets, batch_size: 10, epochs: 100);



                Debug.WriteLine($"Iteration: {iteration} Error: {error}");
            }
            while (++iteration < nbEpisodes && error > 0.001);
        }

        public double TrainingIteration(double learningRate, double epsilon, double discountFactor, bool allowVisited)
        {
            path.Clear();
            world.Reset();

            var state = world.EncodeState(this.X, this.Y);

            return 0;
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

        private float[] EvaluatePolicy(float[] state)
        {
            Tensorflow.NumPy.NDArray input = Tensorflow.NumPy.np.array<float>(state).reshape(new Shape(1, state.Length));

            var output = policyNetwork.predict(input).Single.numpy();

            return [.. output.ElementAt(0)];
        }

        private float EvaluateValue(float[] state)
        {
            Tensorflow.NumPy.NDArray input = Tensorflow.NumPy.np.array<float>(state).reshape(new Shape(1, state.Length));

            var output = policyNetwork.predict(input).Single.numpy();

            return output.ElementAt(0).ToArray<float>()[0];
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

            var state = world.EncodeState(this.X, this.Y);
            var value = EvaluateValue(state);
            var policy = EvaluatePolicy(state).Select((value, index) => new { value, index }).OrderByDescending(x => x.value).ToArray();
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