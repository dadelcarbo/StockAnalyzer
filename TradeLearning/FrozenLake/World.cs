
using FrozenLake.Agents;
using System.Diagnostics;

namespace FrozenLake
{
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
        private int[,] intTiles = new int[10, 10]
        {
            {3,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,1,0,0,0,0,0,0,0,0},
            {0,1,0,0,0,0,0,1,1,1},
            {0,1,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
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

        public Tile[,] Tiles { get; private set; } = new Tile[10, 10];

        public int Size => 10;

        public World()
        {
            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Tiles[j, i] = (Tile)intTiles[i, j];
                }
            }

            Debug.WriteLine("intTiles");
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    Debug.Write($"{intTiles[x, y]} ");
                }
                Debug.WriteLine("");
            }

            Debug.WriteLine("Tile");
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    Debug.Write($"{(int)Tiles[x, y]} ");
                }
                Debug.WriteLine("");
            }
        }

        public bool CanMove(int x, int y, bool allowVisited = true)
        {
            if (x < 0 || y < 0 || x >= Size || y >= Size) return false;
            var tile = Tiles[x, y];
            if (tile == Tile.Wall) return false;
            if (!allowVisited && tile == Tile.Visited) return false;

            return true;
        }


        public float[] EncodeState(IAgent agent)
        {
            int rows = Tiles.GetLength(0);
            int cols = Tiles.GetLength(1);
            int nbTiles = Enum.GetValues(typeof(Tile)).Length;

            // Encode the grid layout (one-hot per tile)
            float[] gridEncoding = new float[rows * cols * (nbTiles + 1)]; // +1 to allocate for the agent

            for (int c = 0; c < cols; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    int tile = (int)Tiles[r, c];
                    int flatIndex = (r * cols + c) + tile * rows * cols;
                    gridEncoding[flatIndex] = 1f;
                }
            }

            // Encode the agent's position as a one-hot vector
            int agentIndex = (agent.X * cols + agent.Y) + rows * cols + nbTiles;
            gridEncoding[agentIndex] = 1f;

            int i = 0;
            foreach (var tile in Enum.GetValues(typeof(Tile)))
            {
                Debug.WriteLine(tile.ToString());
                for (int x = 0; x < rows; x++)
                {
                    for (int y = 0; y < cols; y++)
                    {
                        Debug.Write($"{gridEncoding[i]} ");
                        i++;
                    }

                    Debug.WriteLine("");
                }
            }

            // Combine both encodings
            return gridEncoding;
        }

    }
}
