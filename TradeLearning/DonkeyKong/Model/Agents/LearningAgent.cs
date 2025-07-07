using DonkeyKong.Helpers;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Tensorflow;
using Tensorflow.Keras.Engine;
using static Tensorflow.Binding;

namespace DonkeyKong.Model.Agents
{
    public class LearningAgent : IAgent
    {
        public string Name => "Learning";

        GameRecord gameRecord = new();
        int step = 0;

        /// <summary>
        /// Represent the exploration/exploitation ratio. 0 being 100% random
        /// </summary>
        public double Epsilon { get; set; } = 0.75;
        public bool IsCapturingData { get; set; } = true;

        Random rnd = new Random();
        public Key LastKey { get; set; } = Key.None;



        IModel policyNetwork;

        private static IModel CreatePolicyNetwork(int inputSize, int nbActions)
        {
            // Define input shape
            var input = tf.keras.Input(shape: new Tensorflow.Shape(inputSize));

            // Hidden layers
            var dense1 = tf.keras.layers.Dense(inputSize, activation: (Tensorflow.Keras.Activation)tf.nn.relu).Apply(input);
            var dense2 = tf.keras.layers.Dense(inputSize, activation: (Tensorflow.Keras.Activation)tf.nn.relu).Apply(dense1);

            // Output layer for discrete actions (e.g., 4 actions)
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

            var output = policyNetwork.predict(input).Single.numpy();

            return output.ElementAt(0).ToArray<float>();
        }

        public float[] EncodeOneHot(Tiles[] tiles)
        {
            int nbTiles = Enum.GetValues(typeof(Tiles)).Length;

            // Encode the grid layout (one-hot per tile)
            float[] gridEncoding = new float[world.StateSize]; // +1 to allocate for the agent

            int arraySize = world.Width * world.Height;
            int index;

            for (int y = 0; y < world.Height; y++)
            {
                for (int x = 0; x < world.Width; x++)
                {
                    int tileIndex = y * world.Width + x;
                    var tile = tiles[tileIndex];
                    foreach (var t in Enum.GetValues<Tiles>())
                    {
                        if (tile & t != Tiles.Empty)
                        index = arraySize * tile + tileIndex;
                        gridEncoding[index] = 1f;
                    }
                }
            }

            int i = 0;
            foreach (var tile in Enum.GetValues(typeof(Tiles)))
            {
                Debug.WriteLine(tile.ToString());
                for (int y = 0; y < world.Height; y++)
                {
                    for (int x = 0; x < world.Width; x++)
                    {
                        Debug.Write($"{gridEncoding[i]} ");
                        i++;
                    }
                    Debug.WriteLine("");
                }
            }

            return gridEncoding;
        }


        public AgentAction Decide()
        {
            if (++step > 50 && IsCapturingData)
            {
                if (world.Player.Y < 10)
                {
                    LevelComplete((world.Height - world.Player.Y) / (float)world.Height);
                }
                else
                {
                    LevelComplete(float.NaN);
                }
                world.Initialize(1);
                return AgentAction.None;
            }
            // Get state
            var state = world.GetState();

            var action = rnd.NextDouble() > Epsilon ? (AgentAction)rnd.Next(0, 6) : this.Calculate(state);

            if (IsCapturingData)
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
            policyNetwork = CreatePolicyNetwork(world.StateSize, Enum.GetValues<AgentAction>().Length);
        }

        public void OnDead()
        {
            if (IsCapturingData)
            {
                LevelComplete(-1);
            }
            world.Initialize(1);
        }

        public void OnWin()
        {
            if (IsCapturingData)
            {
                LevelComplete(1);
            }
            world.Initialize(1);
        }

        public float Decay { get; set; } = 0.9f;
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
                gameRecord.Serialize();
            }

            gameRecord = new GameRecord() { Level = world.Level.Number };

            this.step = 0;
        }
    }
}
