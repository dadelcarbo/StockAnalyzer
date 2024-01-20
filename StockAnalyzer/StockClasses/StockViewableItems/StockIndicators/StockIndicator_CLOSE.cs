using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_CLOSE : StockIndicatorBase
    {
        public StockIndicator_CLOSE()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override object[] ParameterDefaultValues => new Object[] { };
        public override ParamRange[] ParameterRanges => new ParamRange[] { };
        public override string[] ParameterNames => new string[] { };
        public override string[] SerieNames => new string[] { "ACTUALCLOSE()" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Blue) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie closeSerie = null;
            if (stockSerie.BarDuration == StockBarDuration.Daily)
            {
                closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            }
            else
            {
                closeSerie = new FloatSerie(stockSerie.Count);
                var dailySerie = stockSerie.GetValues(StockBarDuration.Daily).ToList();
                var valueSerie = stockSerie.Values;

                int i = 0;
                foreach (var dailyValue in valueSerie)
                {
                    closeSerie[i++] = dailySerie.First(d => d.DATE == dailyValue.DATE).CLOSE;
                    dailySerie.RemoveAll(d => d.DATE <= dailyValue.DATE);
                }
            }
            int period = (int)this.parameters[0];
            this.series[0] = closeSerie.CalculateEMA(period);
            this.series[0].Name = "ActualClose";

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
