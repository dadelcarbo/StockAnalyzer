using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FastBars
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct StockBar
    {
        public float open;
        public float high;
        public float low;
        public float close;
        public long volume;
        public long dateTicks;   // Date stored as ticks (keeps struct blittable)


        // ------------------------------------------------------------
        // FASTEST .NET FRAMEWORK SERIALIZE: raw binary dump via pinned buffer
        // ------------------------------------------------------------
        public static void Serialize(string path, StockBar[] bars)
        {
            int size = sizeof(StockBar) * bars.Length;
            byte[] buffer = new byte[size];

            unsafe
            {
                fixed (StockBar* src = bars)
                fixed (byte* dst = buffer)
                {
                    Buffer.MemoryCopy(src, dst, size, size);
                }
            }

            File.WriteAllBytes(path, buffer);
        }

        // ------------------------------------------------------------
        // FASTEST .NET FRAMEWORK DESERIALIZE: raw binary load via pinned buffer
        // ------------------------------------------------------------
        public static StockBar[] Deserialize(string path)
        {
            byte[] buffer = File.ReadAllBytes(path);
            int count = buffer.Length / sizeof(StockBar);
            StockBar[] bars = new StockBar[count];

            unsafe
            {
                fixed (StockBar* dst = bars)
                fixed (byte* src = buffer)
                {
                    Buffer.MemoryCopy(src, dst, buffer.Length, buffer.Length);
                }
            }

            return bars;
        }
    }
}
