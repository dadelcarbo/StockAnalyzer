namespace FrozenLake.Agents
{
    public enum MoveAction
    {
        Left = 0,
        Right = 1,
        Up = 2,
        Down = 3,
        None
    };

    public class Agent : AgentBase
    {
        public override MoveAction Move(bool allowVisited)
        {
            MoveAction[] randomMoves = GetRandomMoves();

            foreach (var move in randomMoves)
            {
                switch (move)
                {
                    case MoveAction.Left:
                        if (world.CanMove(X - 1, Y, allowVisited))
                        {
                            X--;
                            return move;
                        }
                        break;
                    case MoveAction.Right:
                        if (world.CanMove(X + 1, Y, allowVisited))
                        {
                            X++;
                            return move;
                        }
                        break;
                    case MoveAction.Up:
                        if (world.CanMove(X, Y - 1, allowVisited))
                        {
                            Y--;
                            return move;
                        }
                        break;
                    case MoveAction.Down:
                        if (world.CanMove(X, Y + 1, allowVisited))
                        {
                            Y++;
                            return move;
                        }
                        break;
                }
            }
            return MoveAction.None;
        }
    }
}