using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.EntryTargets
{
    public class StockEntryTarget_Percent : StockEntryTargetBase
    {
        public override string Description => "Define the target as a % from entry price";

        [StockAgentParam(0.05f, 0.1f, 0.1f)]
        public float Percent { get; set; }


        FloatSerie closeSerie;
        public override float GetTarget(int index)
        {
            return closeSerie[index] * (1f + Percent);
        }

        protected override bool Init()
        {
            this.closeSerie = DataSerie.GetSerie(StockDataType.CLOSE);
            return true;
        }
    }
}
