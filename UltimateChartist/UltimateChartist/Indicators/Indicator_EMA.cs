using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators
{
    public class StockIndicator_EMA : MovingAverageBase
    {
        public override DisplayType DisplayType => DisplayType.Price;

        public override void Initialize(StockSerie bars)
        {
            this.Values = new ValueSerie
            {
                Values = bars.CloseValues.CalculateEMA((int)this.parameters[0]),
                Name = this.DisplayName,
                Brush = this.DefaultBrush
            };
        }
    }
}
