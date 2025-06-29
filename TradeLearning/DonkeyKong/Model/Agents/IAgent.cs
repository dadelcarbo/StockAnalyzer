namespace DonkeyKong.Model.Agents
{
    public enum AgentAction
    {
        None = 0,
        Left,
        Right,
        Up,
        Down
    }

    public interface IAgent
    {
        void Initialize(World world);
        AgentAction Decide();
    }
}