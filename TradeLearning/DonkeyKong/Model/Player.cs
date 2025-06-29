
using System.Diagnostics;
namespace DonkeyKong.Model
{
    enum Action
    {
        Right,
        Left,
        Jump,
        Climb
    }

	[DebuggerDisplay("X:{X} Y:{Y} IsRight:{IsMovingRight} IsJumping:{IsJumping} IsFalling:{IsFalling}")]
	public class Player : IMoveable
    {
        public int X { get; set; }
        public int Y { get; set; }

        public bool IsMovingRight { get; set; }
        public bool IsJumping { get; set; }
        public bool IsFalling { get; set; }

        public bool CanMove(int x, int y)
        {
            if (x >= 0 && x < World.Instance.Width && y >= 0) // Check boundaries
            {
                if (World.Instance.Background[x, y] == Tiles.FloorLeft || World.Instance.Background[x, y] == Tiles.FloorRight)
                {
                    return false;
                }
                return true;
            }
            return false;
        }
        public bool CanMoveUp(int x, int y)
        {
            return y > 0 && World.Instance.Background[x, y] == Tiles.Ladder;
        }
        public bool CanMoveDown(int x, int y)
        {
            return y >= 0 && World.Instance.Background[x, y] == Tiles.Ladder;
        }

        public void Move(int x, int y)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"X:{X} Y:{Y} IsRight:{IsMovingRight} IsJumping:{IsJumping} IsFalling:{IsFalling}";
        }

        string debugString;
        public void Dump(bool force = false)
        {
            var debug = this.ToString();
            if (force || debug != debugString)
            {
                debugString = debug;
                Debug.WriteLine(debugString);
            }
        }
    }
}
