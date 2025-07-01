
namespace DonkeyKong.Model
{
    public enum Tiles
    {
        Empty = 0,
        FloorLeft = 1, // Ennemy go Left
        FloorRight = 2, // Ennemy go down
        Ladder = 3,
        Ennemy = 4,
        Goal = 5,
        Fire = 6,
        Player = 7
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
        public Coord Goal { get; private set; }
        public Coord EnnemySource { get; private set; }
        public List<Ennemy> Ennemies { get; private set; } = new List<Ennemy>();
        public Level Level { get; set; }

        public bool NextLevel()
        {
            if (Level == null)
            {
                Initialize(1);
                return true;
            }
            else
            {
                Initialize(Level.Number + 1);
                return Level != null;
            }
        }

        public void Initialize(int levelNumber)
        {
            this.Level = Level.GetLevel(levelNumber);
            if (Level == null)
                return;

            Width = Level.LevelArray.GetLength(0);
            Height = Level.LevelArray[0].GetLength(0);

            this.Player = new Player() { X = Level.PlayerStartPos.X, Y = Level.PlayerStartPos.Y };
            this.Goal = Level.GoalPos;
            this.EnnemySource = Level.EnnemySource;

            Ennemies.Clear();

            this.Background = new Tiles[world.Width, world.Height];
            for (int i = 0; i < world.Height; i++)
            {
                for (int j = 0; j < world.Width; j++)
                {
                    this.Background[j, i] = (Tiles)this.Level.LevelArray[i][j];
                }
            }

            this.Status = LevelStatus.Running;
        }

        bool skipCreation = false;
        public void Step()
        {
            if (Ennemies.Count < Level.MaxEnnemys)
            {
                if (skipCreation)
                {
                    skipCreation = false;
                }
                else if (rnd.NextDouble() > 0.8)
                {
                    this.Ennemies.Add(new Ennemy() { X = Level.EnnemySource.X, Y = Level.EnnemySource.Y });
                    skipCreation = true;
                }
            }
            Tiles tileBelow;
            foreach (Ennemy ennemy in Ennemies)
            {
                if (ennemy.Y + 1 == Height) // Ennemy dead
                {
                    ennemy.IsDead = true;
                    break;
                }

                tileBelow = Background[ennemy.X, ennemy.Y + 1];
                switch (tileBelow)
                {
                    case Tiles.Empty:
                        ennemy.Y++;
                        break;
                    case Tiles.FloorLeft:
                        ennemy.X--;
                        ennemy.IsMovingRight = false;
                        break;
                    case Tiles.FloorRight:
                        ennemy.X++;
                        ennemy.IsMovingRight = true;
                        break;
                    case Tiles.Ladder:
                        ennemy.X = ennemy.IsMovingRight ? ennemy.X + 1 : ennemy.X - 1;
                        break;
                    case Tiles.Fire:
                        ennemy.Y++;
                        ennemy.IsDead = true;
                        break;
                }
            }
            this.Ennemies.RemoveAll(b => b.IsDead);

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

            // Check collision with ennemys
            if (Ennemies.Any(b => b.X == Player.X && b.Y == Player.Y))
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
