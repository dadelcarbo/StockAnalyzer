
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
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Tiles[,] Background;

        private static World world = new World();
        public static World Instance => world;

        public LevelStatus Status;
        public Player Player { get; private set; }
        public Goal Goal { get; private set; }
        public List<Barrel> Barrels { get; private set; } = new List<Barrel>();

        Level level;

        public void Initialize(int levelNumber)
        {
            this.level = Level.GetLevel(levelNumber);

            Width = level.LevelTable.GetLength(0);
            Height = level.LevelTable.GetLength(1);

            this.Player = new Player() { X = level.PlayerStartPos.X, Y = level.PlayerStartPos.Y };
            this.Goal = new Goal() { X = level.GoalPos.X, Y = level.GoalPos.Y };
            Barrels.Clear();

            this.Background = new Tiles[world.Width, world.Height];
            for (int i = 0; i < world.Height; i++)
            {
                for (int j = 0; j < world.Width; j++)
                {
                    this.Background[j, i] = (Tiles)this.level.LevelTable[i, j];
                }
            }

            this.Status = LevelStatus.Running;
        }

        bool skipCreation = false;
        public void Step()
        {
            if (Barrels.Count < level.MaxBarrels)
            {
                if (skipCreation)
                {
                    skipCreation = false;
                }
                else if (rnd.NextDouble() > 0.5)
                {
                    this.Barrels.Add(new Barrel() { X = level.BarrelSource.X, Y = level.BarrelSource.Y });
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
