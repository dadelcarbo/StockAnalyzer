namespace FrozenLake.Agents
{
    public class GreedyAgent : AgentBase
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
                        if (world.CanMove(X - 1, Y, false))
                        {
                            X--;
                        }
                        else
                            move = MoveAction.None;
                        break;
                    case MoveAction.Right:
                        if (world.CanMove(X + 1, Y, false))
                        {
                            X++;
                        }
                        else move = MoveAction.None;
                        break;
                    case MoveAction.Up:
                        if (world.CanMove(X, Y - 1, false))
                        {
                            Y--;
                        }
                        else move = MoveAction.None;
                        break;
                    case MoveAction.Down:
                        if (world.CanMove(X, Y + 1, false))
                        {
                            Y++;
                        }
                        else move = MoveAction.None;
                        break;
                    default:
                        break;
                }
                if (move == MoveAction.None)
                {
                    if (world.CanMove(X - 1, Y, false))
                    {
                        X--;
                        move = MoveAction.Left;
                    }
                    else if (world.CanMove(X + 1, Y, false))
                    {
                        X++;
                        move = MoveAction.Right;
                    }
                    else if (world.CanMove(X, Y - 1, false))
                    {
                        Y--;
                        move = MoveAction.Up;
                    }
                    else if (world.CanMove(X, Y + 1, false))
                    {
                        Y++;
                        move = MoveAction.Down;
                    }
                    else
                    {
                        bool found = false;
                        for (int i = 0; !found && i < world.Size; i++)
                        {
                            for (int j = 0; !found && j < world.Size; j++)
                            {
                                if (world.CanMove(i, j, false))
                                {
                                    found = true;
                                    X = i;
                                    Y = j;
                                    move = MoveAction.Left;
                                    break;
                                }
                            }
                        }
                        if (!found)
                            return MoveAction.None;
                    }
                }
            }
            return move;
        }
    }
}