using System.Windows.Input;

namespace DonkeyKong.Model.Agents
{
    public class KeyboardAgent : IAgent
    {
        public bool IsCapturingData { get; set; } = true;
        public string Name => "Keyboard";
        public Key LastKey { get; set; } = Key.None;
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
            return action;
        }

        public void Initialize()
        {
        }
        public void OnDead()
        {
        }

        public void OnWin()
        {
        }
    }
}
