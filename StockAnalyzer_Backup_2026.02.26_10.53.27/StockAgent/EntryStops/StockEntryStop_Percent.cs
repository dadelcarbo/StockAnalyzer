using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockAgent.EntryStops
{
    class StockEntryStop_Percent : StockEntryStopBase
    {
        public override string Description => "Define the stop as a % from entry price";

        [StockAgentParam(0.05f, 0.1f, 0.1f)]
        public float Percent { get; set; }

        StockMath.FloatSerie closeSerie;
        public override float GetStop(int index)
        {
            return closeSerie[index] * (1f - Percent);
        }

        protected override bool Init(StockSerie stockSerie)
        {
            this.closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            return true;
        }
    }
}
