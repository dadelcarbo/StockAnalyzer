using System.Windows.Input;

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
            return (AgentAction)rnd.Next(1, 6);
        }

        World world = World.Instance;

        public void Initialize()
        {
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
