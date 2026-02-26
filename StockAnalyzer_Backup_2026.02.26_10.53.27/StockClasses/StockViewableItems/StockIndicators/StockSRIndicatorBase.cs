using StockAnalyzer.StockMath;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public abstract class StockSRIndicatorBase : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.SupportResistance;

        public override string[] SerieNames => new string[] { "Support", "Resistance" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2) };

        protected void GenerateEvents(StockSerie stockSerie, FloatSerie supportSerie, FloatSerie resistanceSerie)
        {
            this.CreateEventSeries(stockSerie.Count);

            if (stockSerie.Count <= 4)
                return;

            int i = 0;
            while (i < stockSerie.Count && float.IsNaN(supportSerie[i]) && float.IsNaN(resistanceSerie[i]))
                i++;
            if (i == stockSerie.Count)
                return;

            var supportEvents = this.GetEvents("Support");
            var resistanceEvents = this.GetEvents("Resistance");

            var higherLowEvents = this.GetEvents("HigherLow");
            var lowerHighEvents = this.GetEvents("LowerHigh");

            var resistanceBrokenEvents = this.GetEvents("ResistanceBroken");
            var supportBrokenEvents = this.GetEvents("SupportBroken");

            var bullishEvents = this.GetEvents("Bullish");
            var bearishEvents = this.GetEvents("Bearish");

            float previousResistance = float.NaN;
            float previousSupport = float.NaN;
            bool isBullish = false;
            bool isBearish = false;

            if (!float.IsNaN(supportSerie[i]))
            {
                previousSupport = supportSerie[i];
                supportEvents[i] = true;
                isBullish = true;
                bullishEvents[i] = true;
            }
            else
            {
                previousResistance = resistanceSerie[i];
                resistanceEvents[i] = true;
                isBearish = true;
                bearishEvents[i] = true;
            }

            for (i++; i < stockSerie.Count; i++)
            {
                if (float.IsNaN(previousResistance))
                {
                    if (!float.IsNaN(resistanceSerie[i]))
                    {
                        resistanceEvents[i] = true;
                    }
                }
                else
                {
                    if (float.IsNaN(resistanceSerie[i])) // Resistance Broken
                    {
                        resistanceBrokenEvents[i] = true;
                    }
                    else
                    {
                        if (resistanceSerie[i] < previousResistance) // Lower High
                        {
                            lowerHighEvents[i] = true;
                            this.stockTexts.Add(new StockText { AbovePrice = true, Index = i, Text = "LH", Price = resistanceSerie[i] });
                        }
                        resistanceEvents[i] = resistanceSerie[i] != previousResistance;
                    }
                }
                previousResistance = resistanceSerie[i];

                if (float.IsNaN(previousSupport))
                {
                    if (!float.IsNaN(supportSerie[i]))
                    {
                        supportEvents[i] = true;
                    }
                }
                else
                {
                    if (float.IsNaN(supportSerie[i])) // Support Broken
                    {
                        supportBrokenEvents[i] = true;
                    }
                    else
                    {
                        if (supportSerie[i] > previousSupport) // Higher Low
                        {
                            higherLowEvents[i] = true;
                            this.stockTexts.Add(new StockText { AbovePrice = false, Index = i, Text = "HL", Price = supportSerie[i] });
                        }
                        supportEvents[i] = supportSerie[i] != previousSupport;
                    }
                }
                previousSupport = supportSerie[i];

                bullishEvents[i] = isBullish;
                bearishEvents[i] = isBearish;
            }
        }


        static readonly string[] eventNames = new string[] {
            "Support", "Resistance",
            "HigherLow", "LowerHigh",
            "ResistanceBroken", "SupportBroken",
            "Bullish", "Bearish" };

        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
