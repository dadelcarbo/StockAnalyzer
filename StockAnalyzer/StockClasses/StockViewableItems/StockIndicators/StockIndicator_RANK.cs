using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_RANK : StockIndicatorBase, IRange
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }
        public override string Definition => "Display the rank in its group based on the specified indicator.";

        public override object[] ParameterDefaultValues => new Object[] { "RSI(20_1)" };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeIndicator() };
        public override string[] ParameterNames => new string[] { "Indicator" };

        public override string[] SerieNames => new string[] { "RANK(" + this.Parameters[0].ToString() + ")" };

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
        public override HLine[] HorizontalLines
        {
            get
            {
                HLine[] lines = new HLine[] { new HLine((Min + Max) / 2.0f, new Pen(Color.LightGray)) };
                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }

        public float Max
        {
            get { return 1.0f; }
        }

        public float Min
        {
            get { return 0.0f; }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            var rankSerie = new FloatSerie(stockSerie.Count);
            var groupsSeries = StockDictionary.StockDictionarySingleton.Values.Where(s => s.BelongsToGroup(stockSerie.StockGroup) && s.Initialise());

            foreach (var serie in groupsSeries)
            {
                serie.BarDuration = stockSerie.BarDuration;
            }

            var indicatorName = this.parameters[0].ToString().Replace("_", ",");
            int count = 0;
            foreach (var dailyValue in stockSerie.Values)
            {
                var rank = new List<Tuple<string, float>>();
                foreach (var serie in groupsSeries)
                {
                    var index = serie.IndexOf(dailyValue.DATE);
                    if (index != -1)
                    {
                        var indicatorValue = serie.GetIndicator(indicatorName).Series[0][index];
                        rank.Add(new Tuple<string, float>(serie.StockName, indicatorValue));
                    }
                }
                var orderedRank = (float)rank.OrderBy(r => r.Item2).Select(r => r.Item1).ToList().IndexOf(stockSerie.StockName);
                rankSerie[count++] = orderedRank / rank.Count;
            }

            this.series[0] = rankSerie;
            this.series[0].Name = this.SerieNames[0];

            //this.series[1] = rankSerie.CalculateHLTrail((int)this.Parameters[1]);
            //this.series[1].Name = this.SerieNames[1];

            //
            float overbought = 0.75f;
            float oversold = 0.25f;
            for (int i = 100; i < stockSerie.Count; i++)
            {
                float rank = this.series[0][i];
                this.eventSeries[0][i] = (rank > overbought);
                this.eventSeries[1][i] = (rank > oversold);
            }
        }

        static string[] eventNames = new string[] { "Overbought", "Oversold", "Bullish", "Bearish" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
