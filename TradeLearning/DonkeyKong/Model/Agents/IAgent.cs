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
        public bool IsCapturingData { get; set; }
        string Name { get; }
        void Initialize();
        AgentAction Decide();

        void OnDead();
        void OnWin();
    }
}