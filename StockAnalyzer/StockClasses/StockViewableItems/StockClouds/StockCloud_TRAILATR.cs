using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_TRAILATRBAND : StockCloudBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string Definition => "Paint a cloud based on TRAIL ATR";

        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "NbUpDev", "NbDownDev", "MAType", "SignalPeriod" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 2.0f, -2.0f, "MA", 3 }; }
        }
        static List<string> emaTypes = new List<string>() { "EMA", "HMA", "MA", "EA" };
        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(0f, 20.0f),
                new ParamRangeFloat(-20.0f, 0.0f),
                new ParamRangeStringList( emaTypes),
                new ParamRangeInt(1, 500)
                };
            }
        }
        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 1), new Pen(Color.DarkRed, 1), new Pen(Color.DarkBlue, 1), new Pen(Color.DarkBlue, 2) };
                }
                return seriePens;
            }
        }
        public override string[] SerieNames { get { return new string[] { "Bull", "Bear", "Mid", "Signal" }; } }
        public override void ApplyTo(StockSerie stockSerie)
        {
            var atrBandIndicator = stockSerie.GetTrailStop($"TRAILATRBAND({(int)this.parameters[0]},{(float)this.parameters[1]},{(float)this.parameters[2]},{this.parameters[3]})");
            var longStop = atrBandIndicator.Series[0];
            var shortStop = atrBandIndicator.Series[1];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var bodyHighSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Max(v.OPEN, v.CLOSE)).ToArray());
            var bodyLowSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Min(v.OPEN, v.CLOSE)).ToArray());

            var maSerie = stockSerie.GetIndicator(this.parameters[3] + "(" + (int)this.parameters[0] + ")").Series[0];
            var signalSerie = closeSerie.CalculateMA((int)this.parameters[4]);

            var bullSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0]);
            var bearSerie = new FloatSerie(stockSerie.Count, this.SerieNames[1]);

            bool isBull = true;
            for (int i = 0; i < stockSerie.Count; i++)
            {
                isBull = float.IsNaN(shortStop[i]);

                if (isBull)
                {
                    bullSerie[i] = bodyLowSerie[i];
                    bearSerie[i] = longStop[i];
                }
                else
                {
                    bullSerie[i] = bodyHighSerie[i];
                    bearSerie[i] = shortStop[i];
                }
            }

            this.Series[0] = bullSerie;
            this.Series[1] = bearSerie;
            this.Series[2] = maSerie;
            this.Series[2].Name = this.SerieNames[2];
            this.Series[3] = signalSerie;
            this.Series[3].Name = this.SerieNames[3];

            // Detecting events
            base.GenerateEvents(stockSerie);
            for (int i = 5; i <= stockSerie.LastCompleteIndex; i++)
            {
                var bullVal = bullSerie[i];
                var bearVal = bearSerie[i];
                var signalVal = maSerie[i];
                var upBand = Math.Max(bullVal, bearVal);
                var lowBand = Math.Min(bullVal, bearVal);

                if (signalVal > upBand)
                {
                    this.Events[7][i] = true;
                    this.Events[10][i] = !this.Events[7][i - 1];
                }
                else if (signalVal < lowBand)
                {
                    this.Events[8][i] = true;
                    this.Events[11][i] = !this.Events[8][i - 1];
                }
                else
                {
                    this.Events[9][i] = true;
                }
                if (this.Events[3][i] == true)
                {
                    if (closeSerie[i - 1] < signalSerie[i - 1] && closeSerie[i] > signalSerie[i])
                    {
                        this.Events[12][i] = true;
                    }
                }
                else
                {
                    if (closeSerie[i - 1] < signalSerie[i - 1] && closeSerie[i] > signalSerie[i])
                    {
                        this.Events[13][i] = true;
                    }
                }
            }
        }
        public override string[] EventNames => eventNames;

        protected static new string[] eventNames = new string[]
          {
                     "AboveCloud", "BelowCloud", "InCloud",         // 0,1,2
                     "BullishCloud", "BearishCloud",                // 3, 4
                     "CloudUp", "CloudDown",                        // 5, 6
                     "SignalUp", "SignalDown" , "SignalInCloud",    // 7,8,9
                     "BrokenUp", "BrokenDown",                      // 10,11
                     "LongEntry", "ShortEntry"                      // 12,13
          };
        public override bool[] IsEvent => isEvent;

        protected static new readonly bool[] isEvent = new bool[] {
            false, false, false,            // 0,1,2
            false, false,                   // 3, 4
            true, true,                     // 5, 6
            false, false, false,            // 7,8,9
            true, true,                     // 10,11
            true, true                      // 12,13
        };

    }
}
