
using FrozenLake.Agents;
using System.Diagnostics;

namespace FrozenLake
{
    [DebuggerDisplay("({X},{Y})")]
    public struct Size : IEquatable
    {
        public Size() { }
        public Size(int width, int height) { Width = width; Height = height; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
    public enum Tile
    {
        Empty = 0,
        Wall = 1,
        Reward = 2,
        Punish = 3,
        Visited = 4
    };
    public class World
    {
        private int[,] intTiles = new int[6, 10]
        {
            {3,1,1,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,1,0,0,0,0,0,0,0,0},
            {0,1,0,0,0,0,0,1,1,1},
            {0,0,0,0,0,0,0,0,0,2}
        };
        // private int[,] intTiles = new int[10, 10]
        //{
        //     {0,0,0,0,0,0,0,0,0,0},
        //     {0,0,0,0,1,0,0,1,0,0},
        //     {0,0,0,0,1,0,0,1,1,1},
        //     {0,1,1,1,1,0,0,0,0,0},
        //     {0,0,0,0,1,1,1,1,0,0},
        //     {1,1,1,0,1,0,0,0,0,0},
        //     {0,0,0,0,1,0,1,1,1,1},
        //     {0,1,1,1,1,0,0,0,0,0},
        //     {0,0,0,3,1,0,0,1,0,0},
        //     {0,0,0,0,1,0,0,1,2,0}
        //};

        private Tile[,] tiles;

        public Tile Tiles(int x, int y)
        {
            return tiles[y, x];
        }

        public void SetVisited(int x, int y)
        {
            tiles[y, x] = Tile.Visited;
        }

        public Size Size => new Size(intTiles.GetLength(1), intTiles.GetLength(0));

        public int StateSize => Size.Width * Size.Height * (Enum.GetValues(typeof(Tile)).Length + 1);

        public World()
        {
            tiles = new Tile[Size.Height, Size.Width];
            Reset();
        }

        public void Reset()
        {
            for (int x = 0; x < Size.Width; x++)
            {
                for (int y = 0; y < Size.Height; y++)
                {
                    tiles[y, x] = (Tile)intTiles[y, x];
                }
            }

            Debug.WriteLine("intTiles");
            for (int x = 0; x < Size.Width; x++)
            {
                for (int y = 0; y < Size.Height; y++)
                {
                    Debug.Write($"{intTiles[y, x]} ");
                }
                Debug.WriteLine("");
            }

            Debug.WriteLine("Tile");
            for (int x = 0; x < Size.Width; x++)
            {
                for (int y = 0; y < Size.Height; y++)
                {
                    Debug.Write($"{Tiles(x, y)} ");
                }
                Debug.WriteLine("");
            }
        }

        public bool CanMove(int x, int y, bool allowVisited = true)
        {
            if (x < 0 || y < 0 || x >= Size.Width || y >= Size.Height) return false;
            var tile = Tiles(x, y);
            if (tile == Tile.Wall) return false;
            if (!allowVisited && tile == Tile.Visited) return false;

            return true;
        }

        public float[] EncodeState(IAgent agent)
        {
            int nbTiles = Enum.GetValues(typeof(Tile)).Length;

            // Encode the grid layout (one-hot per tile)
            float[] gridEncoding = new float[StateSize]; // +1 to allocate for the agent

            int arraySize = Size.Width * Size.Height;

            for (int y = 0; y < Size.Height; y++)
            {
                for (int x = 0; x < Size.Width; x++)
                {
                    int tile = (int)Tiles(x, y);
                    int index = arraySize * tile + y * Size.Width + x;
                    gridEncoding[index] = 1f;
                }
            }

            // Encode the agent's position as a one-hot vector
            int agentIndex = arraySize * nbTiles + agent.Y * Size.Width + agent.X;
            gridEncoding[agentIndex] = 1f;

            //int i = 0;
            //foreach (var tile in Enum.GetValues(typeof(Tile)))
            //{
            //    Debug.WriteLine(tile.ToString());
            //    for (int y = 0; y < Size.Height; y++)
            //    {
            //        for (int x = 0; x < Size.Width; x++)
            //        {
            //            Debug.Write($"{gridEncoding[i]} ");
            //            i++;
            //        }
            //        Debug.WriteLine("");
            //    }
            //}
            //Debug.WriteLine($"Agent: ({agent.X},{agent.Y})");
            //for (int y = 0; y < Size.Height; y++)
            //{
            //    for (int x = 0; x < Size.Width; x++)
            //    {
            //        Debug.Write($"{gridEncoding[i]} ");
            //        i++;
            //    }

            //    Debug.WriteLine("");
            //}

            // Combine both encodings
            return gridEncoding;
        }

    }
}
