using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.EntryStops
{
    class StockEntryStop_ATR : StockEntryStopBase
    {
        public override string Description => "Define the stop as a multiple of ATR(10)";

        [StockAgentParam(1f, 4f, 0.5f)]
        public float NbATR { get; set; }

        FloatSerie atrSerie;
        public override float GetStop(int index)
        {
            return this.StockSerie.GetSerie(StockDataType.CLOSE)[index] - NbATR * atrSerie[index];
        }

        protected override bool Init(StockSerie stockSerie)
        {
            this.atrSerie = stockSerie.GetIndicator("ATR(10)").Series[0];
            return true;
        }
    }
}
