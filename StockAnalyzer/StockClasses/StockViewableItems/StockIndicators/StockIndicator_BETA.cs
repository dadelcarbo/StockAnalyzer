using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_BETA : StockIndicatorBase
    {
        public StockIndicator_BETA()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Reference", "Period", "Smoothing" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { "CAC40", 20, 1 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeStockName(), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 500) }; }
        }
        public override string[] SerieNames { get { return new string[] { "BETA(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }


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
        static HLine[] lines = null;
        public override HLine[] HorizontalLines
        {
            get
            {
                if (lines == null)
                {
                    lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
                }
                return lines;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[1];
            int smoothing = (int)this.parameters[2];

            var serie = StockDictionary.StockDictionarySingleton[this.parameters[0].ToString()];
            if (!serie.Initialise()) return;

            FloatSerie refSerie = serie.GetIndicator("OSC(" + smoothing + "," + period + ",True)").Series[0];
            FloatSerie currentSerie = stockSerie.GetIndicator("OSC(" + smoothing + "," + period + ",True)").Series[0];

            //FloatSerie refSerie = stockSerie.GenerateSecondarySerieFromOtherSerie(StockDictionary.StockDictionarySingleton[this.parameters[0].ToString()], StockDataType.CLOSE).CalculateEMA((int)this.parameters[2]);
            //FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA((int)this.parameters[2]);

            FloatSerie betaSerie = currentSerie - refSerie;

            //for (int i = period; i < stockSerie.Count; i++)
            //{
            //    float refVar = (refSerie[i] - refSerie[i - period]) / refSerie[i - period];
            //    float closeVar = (currentSerie[i] - currentSerie[i - period]) / currentSerie[i - period];

            //    betaSerie[i] = closeVar / refVar;
            //}

            this.series[0] = betaSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = period; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = (betaSerie[i - 2] < betaSerie[i - 1] && betaSerie[i - 1] > betaSerie[i]);
                this.eventSeries[1][i] = (betaSerie[i - 2] > betaSerie[i - 1] && betaSerie[i - 1] < betaSerie[i]);
                this.eventSeries[2][i] = (betaSerie[i - 1] < 0 && betaSerie[i] >= 0);
                this.eventSeries[3][i] = (betaSerie[i - 1] > 0 && betaSerie[i] <= 0);
            }
        }

        static string[] eventNames = new string[] { "Top", "Bottom", "TurnedPositive", "TurnedNegative" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
