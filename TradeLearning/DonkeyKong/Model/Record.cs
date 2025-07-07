using DonkeyKong.Model.Agents;

namespace DonkeyKong.Model
{
    public class Record
    {
        public int Level { get; set; }
        public int Step { get; set; }
        public AgentAction Action { get; set; }
        public Tiles[] State { get; set; }
        public float Reward { get; set; }
    }
}
