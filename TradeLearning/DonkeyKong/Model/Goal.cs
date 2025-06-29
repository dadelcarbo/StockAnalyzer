using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonkeyKong.Model
{
    [DebuggerDisplay("X:{X} Y:{Y}")]
    public class Goal : IDisplayItem
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
