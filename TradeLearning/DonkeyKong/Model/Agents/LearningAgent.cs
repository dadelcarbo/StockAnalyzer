using DonkeyKong.Helpers;
using System.Diagnostics;
using Tensorflow;
using Tensorflow.Keras.Engine;
using static Tensorflow.Binding;

namespace DonkeyKong.Model.Agents
{
    public class LearningAgent : IAgent
    {
        AgentAction[] actions = Enum.GetValues<AgentAction>();
        public string Name => "Learning";

        private const int BATCH_SIZE = 5;
        GameRecord gameRecord = new();
        int step = 0;

        /// <summary>
        /// Represent the exploration/exploitation ratio. 0 being 100% random
        /// </summary>
        public double Epsilon { get; set; } = 0.90;
        public bool IsLearning { get; set; } = true;

        Random rnd = new Random();

        IModel policyNetwork;

        private static IModel CreatePolicyNetwork(int inputSize, int nbActions)
        {
            // Define input shape
            var input = tf.keras.Input(shape: new Tensorflow.Shape(inputSize));

            // Hidden layers
            var dense1 = tf.keras.layers.Dense(inputSize, activation: (Tensorflow.Keras.Activation)tf.nn.relu).Apply(input);
            var dense2 = tf.keras.layers.Dense(inputSize, activation: (Tensorflow.Keras.Activation)tf.nn.relu).Apply(dense1);

            // Output layer for discrete actions
            var output = tf.keras.layers.Dense(nbActions, activation: "softmax").Apply(dense2);

            // Build model
            var model = tf.keras.Model(input, output);
            model.summary();

            // Compile model
            model.compile(optimizer: tf.keras.optimizers.Adam(learning_rate: 0.001f),
                          loss: tf.keras.losses.CategoricalCrossentropy(),
                          metrics: new[] { "accuracy" });

            return model;
        }

        private float[] EvaluatePolicy(Tiles[] tiles)
        {
            var state = EncodeOneHot(tiles);
            // Encode one hot
            Tensorflow.NumPy.NDArray input = Tensorflow.NumPy.np.array<float>(state).reshape(new Shape(1, state.Length));

            var output = policyNetwork.predict(input);

            //.Single.numpy();

            return output.ElementAt(0).ToArray<float>();
        }

        public float[] EncodeOneHot(Tiles[] tiles)
        {
            // Encode the grid layout (one-hot per tile)
            float[] gridEncoding = new float[world.StateSize]; // +1 to allocate for the agent

            int arraySize = world.Width * world.Height;
            int index;

            int i = 0;
            for (int y = 0; y < world.Height; y++)
            {
                for (int x = 0; x < world.Width; x++)
                {
                    int tileIndex = y * world.Width + x;
                    var tile = tiles[tileIndex];
                    i = 0;
                    foreach (var t in Enum.GetValues<Tiles>())
                    {
                        if (i == 0)
                        {
                            if (tile == 0)
                            {
                                gridEncoding[tileIndex] = 1f;
                            }
                        }
                        else
                        {
                            if (tile.HasFlag(t))
                            {
                                index = arraySize * i + tileIndex;
                                gridEncoding[index] = 1f;
                            }
                        }
                        i++;
                    }
                }
            }

            return gridEncoding;
        }


        public AgentAction Decide()
        {
            if (++step > 50 && IsLearning)
            {
                var vec = (X: world.Level.GoalPos.X - world.Player.X, Y: world.Level.GoalPos.Y - world.Player.Y);
                var distToGoal = (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
                var maxDist = (float)Math.Sqrt(world.Width * world.Width + world.Height * world.Height);

                var reward = (maxDist - distToGoal) / maxDist;
                reward = reward * reward;

                LevelComplete(reward);

                world.Initialize(1);
                return AgentAction.None;
            }

            // Get state
            var state = world.GetState();

            var action = rnd.NextDouble() > Epsilon ? (AgentAction)rnd.Next(0, actions.Length) : this.Calculate(state);

            if (IsLearning)
            {
                gameRecord.Records.Add(new Record
                {
                    Action = action,
                    Reward = 0,
                    State = state
                });
            }

            return action;
        }

        private AgentAction Calculate(Tiles[] state)
        {
            var output = EvaluatePolicy(state);
            return (AgentAction)output.ArgMax();
        }

        World world = World.Instance;

        public void Initialize()
        {
            policyNetwork = CreatePolicyNetwork(world.StateSize, actions.Length);
        }

        public void OnDead()
        {
            if (IsLearning)
            {
                LevelComplete(-1);
            }
            world.Initialize(1);
        }

        public void OnWin()
        {
            if (IsLearning)
            {
                LevelComplete(1);
            }
            world.Initialize(1);
        }

        public float Decay { get; set; } = 0.98f;
        private void LevelComplete(float reward)
        {
            if (!float.IsNaN(reward))
            {
                var value = reward;
                foreach (var record in gameRecord.Records.AsEnumerable().Reverse())
                {
                    record.Reward = value;
                    value *= Decay;
                }
                // gameRecord.Serialize();
                gameRecords.Add(gameRecord);
                if (gameRecords.Count >= BATCH_SIZE)
                {
                    // Train model
                    TrainModel();

                    gameRecords.Clear();
                    gameRecords.AddRange(GameRecord.Load());
                }
            }

            gameRecord = new GameRecord() { Level = world.Level.Number };

            this.step = 0;
        }

        private void TrainModel()
        {
            Debug.WriteLine($"TrainModel: {nbFit}");
            List<(float[] State, float[] ActionValues)> batch = [];
            foreach (var game in gameRecords)
            {
                foreach (var record in game.Records)
                {
                    float[] actionValues;
                    if (record.Reward > 0)
                    {
                        actionValues = new float[actions.Length];
                        actionValues[(int)record.Action] = record.Reward;
                        actionValues.NormalizeNonZero();
                    }
                    else if (record.Reward < 0)
                    {
                        actionValues = new float[actions.Length];
                        for (int i = 0; i < actions.Length; i++)
                        {
                            actionValues[i] = 1;
                        }
                        actionValues[(int)record.Action] = 1 + record.Reward;
                        actionValues.NormalizeNonZero();
                    }
                    else
                    {
                        continue;
                    }

                    batch.Add(new(EncodeOneHot(record.State), actionValues));
                }
            }


            float[] flat = batch.SelectMany(b => b.State).ToArray();
            var states = Tensorflow.NumPy.np.array(flat).reshape(new Shape(1, batch.Count, world.StateSize));

            flat = batch.SelectMany(b => b.ActionValues).ToArray();
            var targets = Tensorflow.NumPy.np.array(flat).reshape(new Shape(1, batch.Count, actions.Length));

            var callback = policyNetwork.fit(states, targets, batch_size: 10, epochs: 20);

            nbFit++;
        }

        int nbFit = 0;

        List<GameRecord> gameRecords = new List<GameRecord>();
    }
}
