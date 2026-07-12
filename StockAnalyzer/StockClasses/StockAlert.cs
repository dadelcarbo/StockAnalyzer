using StockAnalyzer.StockData;
using System;
using System.Linq;

namespace StockAnalyzer.StockClasses
{
    public class StockAlert
    {
        public DateTime Date { get; set; }
        public StockAlertDef AlertDef { get; set; }
        public StockInstrument Instrument { get; set; }


        public StockAlertValue GetAlertValue()
        {
            var dataSerie = Instrument.GetDataSerie(AlertDef.BarDuration);
            if (dataSerie == null)
                throw new InvalidOperationException($"No data serie found for {AlertDef.BarDuration} on {Instrument.DisplayName}");

            var lastIndex = AlertDef.CompleteBar ? dataSerie.LastCompleteIndex : dataSerie.LastIndex;
            var dailyValue = dataSerie.Values[lastIndex];

            float stop = float.NaN;
            if (!string.IsNullOrEmpty(AlertDef.Stop))
            {
                var trailStopSerie = dataSerie.GetTrailStop(AlertDef.Stop)?.Series[0];
                if (trailStopSerie != null)
                {
                    stop = trailStopSerie[lastIndex];
                }
            }

            var speedIndicatorName = string.IsNullOrEmpty(AlertDef.Speed) ? "ROR(35)" : AlertDef.Speed;
            var speedIndicator = dataSerie.GetIndicator(speedIndicatorName);

            var stokPeriod = AlertDef.Stok == 0 ? 35 : AlertDef.Stok;

            var closeSerie = dataSerie.GetSerie(StockDataType.CLOSE);
            var highest = closeSerie.GetHighestIn(lastIndex);
            var stok = dataSerie.CalculateLastFastOscillator(stokPeriod, StockViewableItems.InputType.Close);

            return new StockAlertValue()
            {
                Instrument = Instrument,
                AlertDef = AlertDef,
                Date = this.Date,

                Value = dailyValue.CLOSE,
                Variation = dailyValue.VARIATION,
                Exchanged = dailyValue.EXCHANGED / 1000000,

                Speed = speedIndicator.Series[0][lastIndex],
                SpeedFormat = speedIndicator.SerieFormats?[0],

                Stop = stop,
                Highest = highest,
                Stok = stok
            };

        }
    }
}