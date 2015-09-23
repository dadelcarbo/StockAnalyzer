using System;
using System.Drawing;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILBB : StockTrailStopBase
    {
        public StockTrailStop_TRAILBB()
        {
        }
        public override string Definition
        {
            get { return "TRAILBB(int Period, float NbUpDev, float NbDownDev)"; }
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "NbUpDev", "NbDownDev" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 12, 2.0f, -2.0f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 20.0f), new ParamRangeFloat(-20.0f, 0.0f) }; }
        }

        public override string[] SerieNames { get { return new string[] { "TRAILBB.S", "TRAILBB.R" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;

            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie HighSerie = stockSerie.GetSerie(StockDataType.HIGH);

            IStockIndicator bbIndicator = stockSerie.GetIndicator(this.Name.Replace("TRAIL", ""));
            stockSerie.CalculateBBTrailStop(bbIndicator.Series[1], bbIndicator.Series[0], out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 5; i < stockSerie.Count; i++)
            {
                bool upTrend;
                this.Events[0][i] = upTrend = float.IsNaN(shortStopSerie[i]);
                this.Events[1][i] = !upTrend;
                this.Events[2][i] = upTrend && !this.Events[0][i - 1];
                this.Events[3][i] = !upTrend && !this.Events[1][i - 1];
                this.Events[4][i] = upTrend && this.Events[0][i - 1] && lowSerie[i - 1] <= longStopSerie[i - 1] && lowSerie[i] > longStopSerie[i];
                this.Events[5][i] = !upTrend && this.Events[1][i - 1] && HighSerie[i - 1] >= shortStopSerie[i - 1] && HighSerie[i] < shortStopSerie[i]; ;
            }
        }

        static string[] eventNames = new string[] { "UpTrend", "DownTrend", "BrokenUp", "BrokenDown", "TouchedDown", "TouchedUp" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false, true, true, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
