using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonkeyKong.Model
{
    [DebuggerDisplay("X:{X} Y:{Y}")]
    public class Barrel : IMoveable
    {
        public int X { get; set; }
        public int Y { get; set; }

        public bool IsMovingRight { get; set; }

        public bool IsDead { get; set; } = false;

        public bool CanMove(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void Move(int x, int y)
        {
            throw new NotImplementedException();
        }
    }
}
