using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockAgent.EntryStops
{
    class StockEntryStop_None : StockEntryStopBase
    {
        public override string Description => "No entry stop";

        public override float GetStop(int index)
        {
            return 0f;
        }

        protected override bool Init(StockSerie stockSerie)
        {
            return true;
        }
    }
}
