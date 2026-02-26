using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockAgent.EntryTargets
{
    class StockEntryTarget_None : StockEntryTargetBase
    {
        public override string Description => "No entry stop";

        public override float GetTarget(int index)
        {
            return float.MaxValue;
        }

        protected override bool Init(StockSerie stockSerie)
        {
            return true;
        }
    }
}
