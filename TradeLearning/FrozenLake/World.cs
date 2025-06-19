
namespace FrozenLake
{
    public class World
    {
        private int[,] intTiles = new int[10, 10]
        {
            {0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,1,0,0,1,0,0},
            {0,0,0,0,1,0,0,1,1,1},
            {0,1,1,1,1,0,0,0,0,0},
            {0,0,0,0,1,1,1,1,0,0},
            {1,1,1,0,1,0,0,0,0,0},
            {0,0,0,0,1,0,1,1,1,1},
            {0,1,1,1,1,0,0,0,0,3},
            {0,0,0,3,1,0,0,1,0,0},
            {0,0,0,0,1,0,0,1,2,0}
        };

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
        }

        public bool CanMove(int x, int y, bool allowVisited = true)
        {
            if (x < 0 || y < 0 || x >= Size || y >= Size) return false;
            var tile = Tiles[x, y];
            if (tile == Tile.Wall) return false;
            if (!allowVisited && tile == Tile.Visited) return false;

            return true;
        }
    }
}
