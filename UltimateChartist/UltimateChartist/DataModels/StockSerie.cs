using System;
using System.Collections.Generic;
using System.Linq;

namespace UltimateChartist.DataModels
{
    public class StockSerie
    {
        public StockSerie(Instrument instrument, BarDuration barDuration, List<StockBar> bars)
        {
            Instrument = instrument;
            BarDuration = barDuration;
            Bars = bars;
        }
        public Instrument Instrument { get; }
        public BarDuration BarDuration { get; } = BarDuration.Daily;

        public BarDuration[] SupportedBarDurations { get; } = { BarDuration.M_5, BarDuration.H_1, BarDuration.Daily, BarDuration.Weekly };

        public List<StockBar> Bars { get; }

        private DateTime[] dateValues;
        public DateTime[] DateValues => dateValues ??= this.Bars.Select(b => b.Date).ToArray();

        private double[] closeValues;
        public double[] CloseValues => closeValues ??= this.Bars.Select(b => b.Close).ToArray();

        private double[] highValues;
        public double[] HighValues => highValues ??= this.Bars.Select(b => b.High).ToArray();

        private double[] bodyHighValues;
        public double[] BodyHighValues => bodyHighValues ??= this.Bars.Select(b => b.BodyHigh).ToArray();

        private double[] lowValues;
        public double[] LowValues => lowValues ??= this.Bars.Select(b => b.Low).ToArray();

        private double[] bodyLowValues;
        public double[] BodyLowValues => bodyLowValues ??= this.Bars.Select(b => b.BodyLow).ToArray();

        private double[] openValues;
        public double[] OpenValues => openValues ??= this.Bars.Select(b => b.Open).ToArray();

        private long[] volumeValues;
        public long[] VolumeValues => volumeValues ??= this.Bars.Select(b => b.Volume).ToArray();
    }
}
