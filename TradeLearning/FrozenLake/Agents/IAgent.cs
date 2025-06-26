namespace FrozenLake.Agents
{
    public interface IAgent
    {
        int X { get; set; }
        int Y { get; set; }
        string Name { get; }

        World World { get; }

        void Initialize(World world, Random random);
        MoveAction Move(bool allowVisited);
        void SetRandomLocation();
    }

    public interface ILearningAgent : IAgent
    {
        public void Train(int nbIteration, double learningRate, double epsilon, double discountFactor, bool allowVisited);

    }
}