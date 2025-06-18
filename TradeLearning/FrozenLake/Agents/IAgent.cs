namespace FrozenLake.Agents
{
    internal interface IAgent
    {
        int X { get; set; }
        int Y { get; set; }

        World World { get; }

        void Initialize(World world);
        MoveAction Move();
    }
}