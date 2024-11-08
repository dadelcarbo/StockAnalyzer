using System;
using System.Linq;

namespace StockAnalyzer.StockClasses
{
    public class StockAlert
    {
        public StockAlert()
        {
        }

        public DateTime Date { get; set; }
        public StockAlertDef AlertDef { get; set; }
        public StockSerie StockSerie { get; set; }


        public StockAlertValue GetAlertValue()
        {
            var values = StockSerie.ValueArray;
            var lastIndex = AlertDef.CompleteBar ? StockSerie.LastCompleteIndex : StockSerie.LastIndex;
            var dailyValue = values.ElementAt(lastIndex);

            float stop = float.NaN;
            if (!string.IsNullOrEmpty(AlertDef.Stop))
            {
                var trailStopSerie = StockSerie.GetTrailStop(AlertDef.Stop)?.Series[0];
                if (trailStopSerie != null)
                {
                    stop = trailStopSerie[lastIndex];
                }
            }

            var speedIndicatorName = string.IsNullOrEmpty(AlertDef.Speed) ? "ROR(35)" : AlertDef.Speed;
            var speedIndicator = StockSerie.GetIndicator(speedIndicatorName);

            var stokIndicatorName = $"STOK({(AlertDef.Stok == 0 ? 35 : AlertDef.Stok)})";
            var stokIndicator = StockSerie.GetIndicator(stokIndicatorName);

            var highest = StockSerie.GetSerie(StockDataType.CLOSE).GetHighestIn(lastIndex, dailyValue.CLOSE);

            return new StockAlertValue()
            {
                StockSerie = StockSerie,
                AlertDef = AlertDef,
                Date = this.Date,

                Value = dailyValue.CLOSE,
                Variation = dailyValue.VARIATION,
                Exchanged = dailyValue.EXCHANGED,

                Speed = speedIndicator.Series[0][lastIndex],
                SpeedFormat = speedIndicator.SerieFormats[0],

                Stok = speedIndicator.Series[0][lastIndex],

                TrailStop = stop,
                Highest = highest
            };
        }
    }
}