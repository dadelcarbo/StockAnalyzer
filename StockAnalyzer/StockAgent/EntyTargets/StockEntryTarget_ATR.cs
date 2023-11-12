using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.EntryTargets
{
    class StockEntryTarget_ATR : StockEntryTargetBase
    {
        public override string Description => "Define the target as a multiple of ATR(10)";

        [StockAgentParam(1f, 4f, 0.5f)]
        public float NbATR { get; set; } = 2f;

        FloatSerie atrSerie;
        public override float GetTarget(int index)
        {
            return this.StockSerie.GetSerie(StockDataType.CLOSE)[index] + NbATR * atrSerie[index];
        }

        protected override bool Init(StockSerie stockSerie)
        {
            this.atrSerie = stockSerie.GetIndicator("ATR(10)").Series[0];
            return true;
        }
    }
}
