using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace DonkeyKong.Model;

[DebuggerDisplay("({X},{Y})")]
public struct Coord
{
    public int X { get; set; }
    public int Y { get; set; }

    public Coord(int x, int y) : this()
    {
        this.X = x;
        this.Y = y;
    }
}

public class Level
{
    public static ObservableCollection<Level> Levels { get; set; } = new();

    public int Number { get; set; }

    public Coord GoalPos { get; set; }
    public Coord PlayerStartPos { get; set; }
    public Coord EnnemySource { get; set; }
    public int Interval { get; set; } // ms
    public int MaxEnnemys { get; set; }
    public Tiles[][] LevelArray { get; set; }

    public Level() { }
    public Level(Coord goalPos, Coord playerStartPos, Coord ennemySource, int interval, int maxEnnemys)
    {
        Number = Levels.Max(l => l.Number) + 1;
        GoalPos = goalPos;
        PlayerStartPos = playerStartPos;
        EnnemySource = ennemySource;
        Interval = interval;
        MaxEnnemys = maxEnnemys;

        LevelArray = new Tiles[12][];
        for (int i = 0; i < 10; i++)
        {
            LevelArray[i] = new Tiles[10];
        }

        Levels.Add(this);
    }

    static public Level GetLevel(int level)
    {
        return Levels.FirstOrDefault(l => l.Number == level);
    }

    private static readonly string folderPath = @"C:\src\Repos\StockAnalyzer\TradeLearning\DonkeyKong\Levels";
    public void Serialize()
    {
        string fileName = Path.Combine(folderPath, $"level{this.Number}.json");
        var options = new JsonSerializerOptions { WriteIndented = true };
        using FileStream createStream = File.Create(fileName);
        JsonSerializer.Serialize(createStream, this, options);
    }

    public static void Load()
    {
        foreach (var fileName in Directory.EnumerateFiles(folderPath))
        {
            string json = File.ReadAllText(fileName);

            var level = JsonSerializer.Deserialize<Level>(json);
            Levels.Add(level);
        }
    }
}
