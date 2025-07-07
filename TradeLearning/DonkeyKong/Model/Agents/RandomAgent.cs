using System.Windows.Input;

namespace DonkeyKong.Model.Agents
{
    public class KeyboardAgent : IAgent
    {
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

        World world = World.Instance;
        public void Initialize(World world)
        {
        }
    }
}
