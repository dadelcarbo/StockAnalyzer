using StockAnalyzer.StockMath;
using StockAnalyzerSettings;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_RANK : StockIndicatorBase, IRange
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.RangedIndicator;
        public override string Definition => "Display the rank in its group based on the specified indicator.";

        public override object[] ParameterDefaultValues => new Object[] { "ROR(100)" };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeIndicator() };
        public override string[] ParameterNames => new string[] { "Indicator" };

        public override string[] SerieNames => new string[] { "RANK(" + this.Parameters[0].ToString() + ")" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black) };
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

        public float Max => 1.0f;

        public float Min => 0.0f;

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            var rankSerie = new FloatSerie(stockSerie.Count);
            this.series[0] = rankSerie;
            this.series[0].Name = this.SerieNames[0];

            var indicatorName = this.parameters[0].ToString().Replace("_", ",");

            var destinationFolder = Path.Combine(Folders.DataFolder, "Rank");

            string fileName = Path.Combine(destinationFolder, $"{stockSerie.StockGroup}_{indicatorName}_{stockSerie.BarDuration}.txt");
            if (!File.Exists(fileName))
                return;

            using (var sr = new StreamReader(fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith(stockSerie.StockName))
                    {
                        rankSerie.Values = line.Split('|').Skip(1).Select(r => float.Parse(r) / 100.0f).ToArray();
                        break;
                    }
                }
            }

            ////
            //float overbought = 0.75f;
            //float oversold = 0.25f;
            //for (int i = 100; i < stockSerie.Count; i++)
            //{
            //    float rank = this.series[0][i];
            //    this.eventSeries[0][i] = (rank > overbought);
            //    this.eventSeries[1][i] = (rank > oversold);
            //}
        }

        static string[] eventNames = new string[] { "Overbought", "Oversold", "Bullish", "Bearish" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { false, false, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
