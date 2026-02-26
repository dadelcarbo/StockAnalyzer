using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockAgent.EntryStops
{
    class StockEntryStop_BarLow : StockEntryStopBase
    {
        public override string Description => "Define the stop a the low of the n previous bar";

        [StockAgentParam(1, 20, 1)]
        public int Period { get; set; }

        StockMath.FloatSerie lowSerie;
        public override float GetStop(int index)
        {
            return lowSerie.GetMin(index - Period, index);
        }

        protected override bool Init(StockSerie stockSerie)
        {
            this.lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            return true;
        }
    }
}
