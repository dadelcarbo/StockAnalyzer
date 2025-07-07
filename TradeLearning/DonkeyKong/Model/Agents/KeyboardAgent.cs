using System.Windows.Input;

namespace DonkeyKong.Model.Agents
{
    public class RandomAgent : IAgent
    {
        Random rnd = new Random();
        public Key LastKey { get; set; } = Key.None;
        public AgentAction Decide()
        {
            return (AgentAction)rnd.Next(0, 6);
        }

        World world = World.Instance;

        public void Initialize(World world)
        {
        }
    }
}
