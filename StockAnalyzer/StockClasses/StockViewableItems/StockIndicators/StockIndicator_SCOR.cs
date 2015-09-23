using System;
using System.Drawing;
using StockAnalyzer.StockMath;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_SCOR : StockIndicatorBase
    {
        public StockIndicator_SCOR()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override string Name
        {
            get { return "SCOR(" + this.Parameters[0].ToString() + ")"; }
        }

        public override string Definition
        {
            get { return "SCOR(int Period)"; }
        }
        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 20 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }
        
        public override string[] SerieNames { get { return new string[] { "SCOR(" + this.Parameters[0].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            StockSerie seasonSerie = stockSerie.CalculateSeasonality();
            FloatSerie seasonCloseSerie = new FloatSerie(stockSerie.Count);

            int i = 0;
            float previousClose = stockSerie.Values.First().CLOSE;
            FloatSerie seasonClose = seasonSerie.GetSerie(StockDataType.CLOSE);
            int indexOfDate;
            int previousIndexOfDate = 0;
            foreach (StockDailyValue dailyValue in stockSerie.Values)
            {
                indexOfDate = seasonSerie.IndexOf(new DateTime(2000, dailyValue.DATE.Month, dailyValue.DATE.Day));
                if (indexOfDate != -1)
                {
                    if (indexOfDate >= previousIndexOfDate)
                    {
                        seasonCloseSerie[i] = previousClose * (1 + (seasonClose[indexOfDate] - seasonClose[previousIndexOfDate]) / seasonClose[previousIndexOfDate]);
                    }
                    previousIndexOfDate = indexOfDate;
                }
                else
                {
                    seasonCloseSerie[i] = previousClose;
                }
                previousClose = seasonCloseSerie[i];
                i++;
            }

            this.series[0] = stockSerie.GetSerie(StockDataType.CLOSE).CalculateCorrelation(seasonCloseSerie, (int)parameters[0]);
            this.Series[0].Name = this.Name;
        }

        static string[] eventNames = new string[] { };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
