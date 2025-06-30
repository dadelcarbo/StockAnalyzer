namespace DonkeyKong.Model;

public class Level
{
    private static readonly List<Level> levels = [
        new Level {
            Number=1,
            LevelTable = new int[,] {
                { 0,0,0,5,0,0,0,0,0,0 },
                { 0,0,0,3,0,0,0,0,0,0 },
                { 0,0,0,3,0,0,0,0,0,0 },
                { 2,2,2,2,2,2,3,2,2,0 },
                { 0,0,0,0,0,0,3,0,0,0 },
                { 0,0,0,0,0,0,3,0,0,0 },
                { 0,1,3,1,1,1,1,1,1,1 },
                { 0,0,3,0,0,0,0,0,0,0 },
                { 0,0,3,0,0,0,0,0,0,0 },
                { 2,2,2,2,2,2,2,2,2,6 }
            },
            GoalPos = new (3, 0),
            PlayerStartPos = new(7, 8),
            BarrelSource = new(1, 0),
            Interval = 500,
            MaxBarrels = 6
        }
    ];

    public int[,] LevelTable { get; private set; }
    public int Number { get; private set; }

    public (int X, int Y) GoalPos { get; private set; }
    public (int X, int Y) PlayerStartPos { get; private set; }
    public (int X, int Y) BarrelSource { get; private set; }
    public int Interval { get; private set; } // ms
    public int MaxBarrels { get; private set; }

    static public Level GetLevel(int level)
    {
        return levels[level - 1];
    }
}
