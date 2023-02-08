using System.Linq;
using System.Windows.Media;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators
{
    public class StockIndicator_EMA : MovingAverageBase
    {
        public override DisplayType DisplayType => DisplayType.Price;

        [IndicatorDisplayAttribute]
        public ValueSerie EMA { get; protected set; } = new ValueSerie() { Brush = new SolidColorBrush(Colors.Red) };

        public override void Initialize(StockSerie stockSerie)
        {
            this.EMA.Dates = stockSerie.DateValues;
            this.EMA.Values = stockSerie.CloseValues.CalculateEMA(this.Period);
        }
    }
}
