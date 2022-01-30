using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_TRAILATR : StockCloudBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string Definition => "Paint a cloud based on TRAIL ATR";

        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "NbUpDev", "NbDownDev", "MAType", "ReentryPeriod" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 30, 2.0f, -2.0f, "EMA", 6 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(0f, 20.0f),
                new ParamRangeFloat(-20.0f, 0.0f),
                new ParamRangeMA(),
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
                    seriePens = new Pen[] { new Pen(Color.Green, 1), new Pen(Color.DarkRed, 1), new Pen(Color.DarkBlue, 1), new Pen(Color.DarkRed, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot } };
                }
                return seriePens;
            }
        }
        public override string[] SerieNames { get { return new string[] { "Bull", "Bear", "Mid", "Resistance" }; } }
        public override void ApplyTo(StockSerie stockSerie)
        {
            var atrBandIndicator = stockSerie.GetTrailStop($"TRAILATR({(int)this.parameters[0]},{((float)this.parameters[1]).ToString(StockAnalyzerApp.Global.EnglishCulture)},{((float)this.parameters[2]).ToString(StockAnalyzerApp.Global.EnglishCulture)},{this.parameters[3]})");
            var longStop = atrBandIndicator.Series[0];
            var shortStop = atrBandIndicator.Series[1];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);

            var maSerie = stockSerie.GetIndicator(this.parameters[3] + "(" + (int)this.parameters[0] + ")").Series[0];

            float alpha = 2.0f / ((int)this.parameters[4] + 1f);
            var resistanceSerie = new FloatSerie(stockSerie.Count, this.SerieNames[3], float.NaN);
            float resistance = float.NaN;
            float previousResistance = float.MinValue;

            var bullSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0]);
            var bearSerie = new FloatSerie(stockSerie.Count, this.SerieNames[1]);
            bullSerie[0] = bearSerie[0] = maSerie[0];

            for (int i = 1; i < stockSerie.Count; i++)
            {
                bullSerie[i] = closeSerie[i];
                if (float.IsNaN(shortStop[i])) // Bullish
                {
                    bearSerie[i] = longStop[i];
                    if (float.IsNaN(resistance))
                    {
                        var previousHigh = highSerie[i - 1];
                        if (previousHigh > highSerie[i] && float.IsNaN(resistanceSerie[i - 1]))
                        {
                            resistance = previousHigh;
                            resistanceSerie[i] = resistance;
                        }
                    }
                    else
                    {
                        if (closeSerie[i] > resistance)
                        {
                            bool higherBreakOut = resistance > previousResistance;
                            this.stockTexts.Add(new StockText
                            {
                                AbovePrice = false,
                                Index = i,
                                Text = resistance > previousResistance ? "HBO" : "LBO"
                            });
                            resistanceSerie[i] = resistance;
                            previousResistance = resistance;
                            resistance = float.NaN;
                        }
                        else
                        {
                            resistance = Math.Min(resistance, resistance + alpha * (highSerie[i] - resistance));
                            resistanceSerie[i] = resistance;
                        }
                    }
                }
                else // Bearish
                {
                    previousResistance = bearSerie[i] = shortStop[i];
                    resistance = float.NaN;
                }
            }

            this.Series[0] = bullSerie;
            this.Series[1] = bearSerie;
            this.Series[2] = maSerie;
            this.Series[2].Name = this.SerieNames[2];
            this.Series[3] = resistanceSerie;
            this.Series[3].Name = this.SerieNames[3];

            // Detecting events
            base.GenerateEvents(stockSerie);
            for (int i = 5; i < stockSerie.Count; i++)
            {
                this.Events[7][i] = !float.IsNaN(resistanceSerie[i]) && closeSerie[i] > resistanceSerie[i];
                this.Events[8][i] = false;// not implemented
            }
            foreach (var text in this.StockTexts)
            {
                if (text.Text == "HBO")
                {
                    this.Events[9][text.Index] = true;
                }
                else
                {
                    this.Events[10][text.Index] = true;
                }
            }
        }
        public override string[] EventNames => eventNames;

        protected static new string[] eventNames = new string[]
          {
                     "AboveCloud", "BelowCloud", "InCloud",         // 0,1,2
                     "BullishCloud", "BearishCloud",                // 3,4
                     "CloudUp", "CloudDown",                        // 5,6
                     "Long Reentry", "Short Reentry",               // 7,8
                     "HigherBreakOut", "LowerBreakOut"              // 9,10
          };
        public override bool[] IsEvent => isEvent;

        protected static new readonly bool[] isEvent = new bool[] {
            false, false, false,            // 0,1,2
            false, false,                   // 3,4
            true, true,                     // 5,6
            true, true,                     // 7,8
            true, true                      // 9,10
        };

    }
}
