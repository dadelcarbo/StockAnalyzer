using DonkeyKong.Model.Agents;
using System.IO;
using System.Text.Json;

namespace DonkeyKong.Model
{
    public class GameRecord
    {
        public GameRecord()
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Records");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
        }
        public int Level { get; set; }

        public List<Record> Records { get; set; } = [];

        private static string folderPath;
        public void Serialize()
        {
            string fileName = Path.Combine(folderPath, $"Game_{this.Level}_{DateTime.Now.ToString("yyyyMMdd_hhss_fff")}.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            using FileStream createStream = File.Create(fileName);
            JsonSerializer.Serialize(createStream, this, options);
        }

        public static List<GameRecord> Load()
        {
            List<GameRecord> gameRecords = [];
            foreach (var fileName in Directory.EnumerateFiles(folderPath))
            {
                string json = File.ReadAllText(fileName);

                var gameRecord = JsonSerializer.Deserialize<GameRecord>(json);
                gameRecords.Add(gameRecord);
            }

            return gameRecords;
        }
    }

    public class Record
    {
        public AgentAction Action { get; set; }
        public Tiles[] State { get; set; }
        public float Reward { get; set; }
    }
}
