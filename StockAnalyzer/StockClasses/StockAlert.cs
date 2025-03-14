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

            var closeSerie = StockSerie.GetSerie(StockDataType.CLOSE);
            var highest = closeSerie.GetHighestIn(lastIndex, dailyValue.CLOSE);

            var cupHandle = highest > 5 ? closeSerie.DetectCupHandle(lastIndex, 5, false) : null;
            int step = 0;
            if (cupHandle != null)
            {
                step = (int)cupHandle.Point2.X - (int)cupHandle.Point1.X;
            }

            return new StockAlertValue()
            {
                StockSerie = StockSerie,
                AlertDef = AlertDef,
                Date = this.Date,

                Value = dailyValue.CLOSE,
                Variation = dailyValue.VARIATION,
                Exchanged = dailyValue.EXCHANGED / 1000000,

                Speed = speedIndicator.Series[0][lastIndex],
                SpeedFormat = speedIndicator.SerieFormats[0],

                Stok = stokIndicator.Series[0][lastIndex],

                Stop = stop,
                Highest = highest,
                Step = step
            };
        }
    }
}