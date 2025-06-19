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
        public override MoveAction Move()
        {
            MoveAction move = MoveAction.None;
            while (move == MoveAction.None)
            {
                move = (MoveAction)rnd.Next(0, 4);
                switch (move)
                {
                    case MoveAction.Left:
                        if (world.CanMove(X - 1, Y, true))
                        {
                            X--;
                        }
                        else
                            move = MoveAction.None;
                        break;
                    case MoveAction.Right:
                        if (world.CanMove(X + 1, Y, true))
                        {
                            X++;
                        }
                        else move = MoveAction.None;
                        break;
                    case MoveAction.Up:
                        if (world.CanMove(X, Y - 1, true))
                        {
                            Y--;
                        }
                        else move = MoveAction.None;
                        break;
                    case MoveAction.Down:
                        if (world.CanMove(X, Y + 1, true))
                        {
                            Y++;
                        }
                        else move = MoveAction.None;
                        break;
                }
            }
            return move;
        }
    }
}