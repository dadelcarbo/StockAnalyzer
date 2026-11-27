using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.EntryStops
{
    public class StockEntryStop_Percent : StockEntryStopBase
    {
        public override string Description => "Define the stop as a % from entry price";

        [StockAgentParam(0.05f, 0.1f, 0.1f)]
        public float Percent { get; set; }

        FloatSerie openSerie;
        public override float GetStop(int index)
        {
            return openSerie[index + 1] * (1f - Percent);
        }

        protected override bool Init()
        {
            this.openSerie = DataSerie.GetSerie(StockDataType.OPEN);
            return true;
        }
    }
}
