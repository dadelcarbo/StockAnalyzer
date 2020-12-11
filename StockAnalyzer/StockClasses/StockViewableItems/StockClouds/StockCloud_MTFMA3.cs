using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_MTFMA3 : StockCloudBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }
        public override string Definition => "Paint cloud based MA on multitime frames";

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 3 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }
        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 1), new Pen(Color.DarkRed, 1), new Pen(Color.DarkBlue, 2) };
                }
                return seriePens;
            }
        }
        public override string[] SerieNames { get { return new string[] { "Bull", "Bear", "Signal" }; } }
        public override void ApplyTo(StockSerie stockSerie)
        {
            if (stockSerie.BarDuration.Duration != BarDuration.Daily)
                throw new InvalidOperationException("Can only calculate this indicator on daily time frame");

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var weeklySerie = new FloatSerie(stockSerie.Count);
            var monthlySerie = new FloatSerie(stockSerie.Count);
            weeklySerie[0] = weeklySerie[1] = weeklySerie[2] = closeSerie[2];
            monthlySerie[0] = monthlySerie[1] = monthlySerie[2] = closeSerie[2];

            float weekly0;
            float weekly1 = closeSerie[2];
            float weekly2 = closeSerie[2];
            float monthly0;
            float monthly1 = closeSerie[2];
            float monthly2 = closeSerie[2];
            var previousValue = stockSerie.Values.ElementAt(2);

            int i = 3;
            foreach (var dailyValue in stockSerie.Values.Skip(3))
            {
                if (dailyValue.DATE.DayOfWeek < previousValue.DATE.DayOfWeek)
                { // New week starting
                    weekly0 = weekly1;
                    weekly1 = weekly2;
                    weekly2 = previousValue.CLOSE;
                    weeklySerie[i] = (weekly0 + weekly1 + weekly2) / 3;
                }
                else
                {
                    weeklySerie[i] = weeklySerie[i - 1];
                }
                if (dailyValue.DATE.Month != previousValue.DATE.Month)
                { // New month starting
                    monthly0 = monthly1;
                    monthly1 = monthly2;
                    monthly2 = previousValue.CLOSE;
                    monthlySerie[i] = (monthly0 + monthly1 + monthly2) / 3;
                }
                else
                {
                    monthlySerie[i] = monthlySerie[i - 1];
                }

                i++;
                previousValue = dailyValue;
            }

            this.Series[0] = weeklySerie;
            this.Series[1] = monthlySerie;
            this.Series[2] = closeSerie.CalculateMA(3);

            // Detecting events
            this.GenerateEvents(stockSerie);
        }
    }
}
