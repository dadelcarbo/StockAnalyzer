using DonkeyKong.Model;
using System;
using System.Windows.Documents;
using System.Windows.Input;

namespace DonkeyKong.Model.Agents
{
    public class KeyboardAgent : IAgent
    {
        public bool IsLearning { get; set; } = true;
        public string Name => "Keyboard";
        public Key LastKey { get; set; } = Key.None;

        GameRecord gameRecord = new();

        public AgentAction Decide()
        {

            var action = LastKey switch
            {
                Key.Left => AgentAction.Left,
                Key.Right => AgentAction.Right,
                Key.Up => AgentAction.Up,
                Key.Down => AgentAction.Down,
                _ => AgentAction.None
            };
            this.LastKey = Key.None;
            if (IsLearning && action != AgentAction.None)
            {
                var state = World.Instance.GetState();
                gameRecord.Records.Add(new Record
                {
                    Action = action,
                    Reward = 0,
                    State = state
                });
            }
            return action;
        }

        public void Initialize()
        {
        }
        public void OnDead()
        {
            if (IsLearning)
            {
                LevelComplete(-1);
            }
            World.Instance.Initialize(1);
        }

        public void OnWin()
        {
            if (IsLearning)
            {
                LevelComplete(1);
            }
            World.Instance.Initialize(1);
        }

        public float Decay { get; set; } = 0.99f;
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

            gameRecord = new GameRecord() { Level = World.Instance.Level.Number };
        }
    }
}
