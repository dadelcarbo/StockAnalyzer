

using System.Diagnostics;

namespace DonkeyKong.Model
{
    public enum Tiles
    {
        Empty = 0,
        FloorLeft = 1, // Barrel go Left
        FloorRight = 2, // Barrel go down
        Ladder = 3,
        Barrel = 4,
        Goal = 5,
        Fire = 6
    }

    public enum LevelStatus
    {
        Running,
        Completed,
        Lost
    }

    public class World
    {
        Random rnd = new Random();
        public int Width => level.GetLength(0);
        public int Height => level.GetLength(1);

        private int[,] level = new int[,] {
            {0,0,0,5,0,0,0,0,0,0 },
            {0,0,0,3,0,0,0,0,0,0 },
            {0,0,0,3,0,0,0,0,0,0 },
            {2,2,2,2,2,2,3,2,2,0 },
            {0,0,0,0,0,0,3,0,0,0 },
            {0,0,0,0,0,0,3,0,0,0 },
            {0,1,3,1,1,1,1,1,1,1 },
            {0,0,3,0,0,0,0,0,0,0 },
            {0,0,3,0,0,0,0,0,0,0 },
            {2,2,2,2,2,2,2,2,2,6 },
        };
        public Tiles[,] Background;

        public (int X, int Y) PlayerStartPos = new(7, 8);
        public (int X, int Y) GoalPos = new(3, 0);
        public (int X, int Y) BarrelSource = new(1, 0);

        private static World world = new World();
        public static World Instance => world;

        public LevelStatus Status;
        public Player Player { get; set; }
        public Goal Goal { get; set; }
        public List<Barrel> Barrels { get; set; } = new List<Barrel>();
        int NB_BARRELS = 6;

        public void Initialize(int level)
        {
            this.Player = new Player() { X = PlayerStartPos.X, Y = PlayerStartPos.Y };
            this.Goal = new Goal() { X = GoalPos.X, Y = GoalPos.Y };
            Barrels.Clear();

            this.Background = new Tiles[world.Width, world.Height];
            for (int i = 0; i < world.Height; i++)
            {
                for (int j = 0; j < world.Width; j++)
                {
                    this.Background[j, i] = (Tiles)this.level[i, j];
                }
            }

            this.Status = LevelStatus.Running;
        }

        bool skipCreation = false;
        public void Step()
        {
            if (Barrels.Count < NB_BARRELS)
            {
                if (skipCreation)
                {
                    skipCreation = false;
                }
                else if (rnd.NextDouble() > 0.5)
                {
                    this.Barrels.Add(new Barrel() { X = BarrelSource.X, Y = BarrelSource.Y });
                    skipCreation = true;
                }
            }
            Tiles tileBelow;
            foreach (Barrel barrel in Barrels)
            {
                if (barrel.Y + 1 == Height) // Barrel dead
                {
                    barrel.IsDead = true;
                    break;
                }

                tileBelow = Background[barrel.X, barrel.Y + 1];
                switch (tileBelow)
                {
                    case Tiles.Empty:
                        barrel.Y++;
                        break;
                    case Tiles.FloorLeft:
                        barrel.X--;
                        barrel.IsMovingRight = false;
                        break;
                    case Tiles.FloorRight:
                        barrel.X++;
                        barrel.IsMovingRight = true;
                        break;
                    case Tiles.Ladder:
                        barrel.X = barrel.IsMovingRight ? barrel.X + 1 : barrel.X - 1;
                        break;
                    case Tiles.Fire:
                        barrel.Y++;
                        barrel.IsDead = true;
                        break;
                }
            }
            this.Barrels.RemoveAll(b => b.IsDead);

            // Check if player is falling
            tileBelow = Background[Player.X, Player.Y + 1];
            if (tileBelow == Tiles.Fire) // Player dead
            {
                this.Status = LevelStatus.Lost;
                return;
            }
            else if (tileBelow == Tiles.Empty)
            {
                if (!this.Player.IsJumping)
                {
                    this.Player.Y++;
                }
                this.Player.IsFalling = Background[Player.X, Player.Y + 1] == Tiles.Empty;
                this.Player.IsJumping = false;
            }
            else
            {
                this.Player.IsFalling = false;
                this.Player.IsJumping = false;
            }

            Player.Dump();

            // Check collision with barrels
            if (Barrels.Any(b => b.X == Player.X && b.Y == Player.Y))
            {
                this.Status = LevelStatus.Lost;
                return;
            }

            // Check if goal is reached
            if (Goal.X == Player.X && Goal.Y == Player.Y)
            {
                this.Status = LevelStatus.Completed;
                return;
            }
        }
    }
}
