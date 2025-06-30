using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            LevelArray = new int[] [] {
                [ 0,0,0,5,0,0,0,0,0,0 ],
                [ 0,0,0,3,0,0,0,0,0,0 ],
                [ 0,0,0,3,0,0,0,0,0,0 ],
                [ 2,2,2,2,2,2,3,2,2,0 ],
                [ 0,0,0,0,0,0,3,0,0,0 ],
                [ 0,0,0,0,0,0,3,0,0,0 ],
                [ 0,1,3,1,1,1,1,1,1,1 ],
                [ 0,0,3,0,0,0,0,0,0,0 ],
                [ 0,0,3,0,0,0,0,0,0,0 ],
                [ 2,2,2,2,2,2,2,2,2,6 ]
            },
            GoalPos = new (3, 0),
            PlayerStartPos = new(7, 8),
            EnnemySource = new(1, 0),
            Interval = 500,
            MaxEnnemys = 6
        }
    ];

    [JsonIgnore]
    public int[,] LevelTable { get; private set; }
    public int[][] LevelArray { get; private set; }
    public int Number { get; private set; }

    public (int X, int Y) GoalPos { get; private set; }
    public (int X, int Y) PlayerStartPos { get; private set; }
    public (int X, int Y) EnnemySource { get; private set; }
    public int Interval { get; private set; } // ms
    public int MaxEnnemys { get; private set; }

    static public Level GetLevel(int level)
    {
        string filePath = @"C:\src\Repos\StockAnalyzer\TradeLearning\DonkeyKong\Levels\level1.json";

        var options = new JsonSerializerOptions { WriteIndented = true };
        using FileStream createStream = File.Create(filePath);
        JsonSerializer.Serialize(createStream, levels[level - 1], options);

        return levels[level - 1];
    }
}
