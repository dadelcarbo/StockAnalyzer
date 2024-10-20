using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public abstract class StockTrailStopBase : Parameterizable, IStockTrailStop
    {
        public StockTrailStopBase()
        {
            this.series = new FloatSerie[this.SeriesCount];
            this.extras = new FloatSerie[this.ExtraNames.Length];
            if (EventCount != 0)
            {
                this.eventSeries = new BoolSerie[this.EventCount];
            }
            this.serieVisibility = new bool[this.SeriesCount];
            for (int i = 0; i < this.SeriesCount; this.serieVisibility[i++] = true) ;
        }

        public abstract IndicatorDisplayTarget DisplayTarget { get; }
        public IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.TrailStop;
        public ViewableItemType Type => ViewableItemType.TrailStop;
        public virtual bool RequiresVolumeData => false;

        public string ToThemeString()
        {
            string themeString = "TRAILSTOP|" + this.Name;
            for (int i = 0; i < this.SeriesCount; i++)
            {
                themeString += "|" + GraphCurveType.PenToString(this.SeriePens[i]) + "|" + this.SerieVisibility[i].ToString();
            }
            return themeString;
        }
        public sealed override string[] SerieNames => new string[] { $"{this.ShortName}.LS", $"{this.ShortName}.SS", "LongReentry", "ShortReentry" };

        protected FloatSerie[] series;
        public FloatSerie[] Series => series;

        public FloatSerie LongStopSerie => series?[0];
        public FloatSerie ShortStopSerie => series?[1];

        public string[] ExtraNames => new string[] { "ReentryRange", "DistToReentry", "BarsInReentry" };

        protected FloatSerie[] extras;
        public FloatSerie[] Extras => extras;

        public virtual Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Green, 2) {DashStyle = System.Drawing.Drawing2D.DashStyle.Dot },
            new Pen(Color.Red, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot },
            new Pen(Color.DarkRed, 2), new Pen(Color.DarkGreen, 2) };

        private readonly bool[] serieVisibility;
        public bool[] SerieVisibility => this.serieVisibility;

        public void Initialise(string[] parameters)
        {
            this.ParseInputParameters(parameters);
        }

        abstract public void ApplyTo(StockSerie stockSerie);

        #region IStockEvent implementation
        protected BoolSerie[] eventSeries;
        public int EventCount
        {
            get
            {
                if (EventNames != null)
                {
                    return EventNames.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        public BoolSerie[] Events => eventSeries;
        public BoolSerie GetEvents(string eventName)
        {
            int index = Array.IndexOf(this.EventNames, eventName);
            return index != -1 ? this.Events[index] : null;
        }
        virtual protected void CreateEventSeries(int count)
        {
            for (int i = 0; i < this.EventCount; i++)
            {
                this.eventSeries[i] = new BoolSerie(count, this.EventNames[i]);
            }
        }
        #endregion

        private static readonly string[] eventNames = new string[]
          {
             "BrokenUp", "BrokenDown",           // 0,1
             "Pullback", "EndOfTrend",           // 2,3
             "HigherLow", "LowerHigh",           // 4,5
             "Bullish", "Bearish",               // 6,7
             "LH_HL", "HL_LH",                   // 8,9
             "Long Reentry", "Consolidation"     // 10,11
          };

        public string[] EventNames => eventNames;

        private static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, false, false, true, true, true, false };
        public bool[] IsEvent => isEvent;

        protected void GenerateEvents(StockSerie stockSerie, FloatSerie longStopSerie, FloatSerie shortStopSerie)
        {
            this.CreateEventSeries(stockSerie.Count);

            var periodIndex = Array.IndexOf(this.ParameterNames, "ReentryPeriod");
            if (periodIndex != -1)
            {
                this.ReentryPeriod = (int)this.parameters[periodIndex];
            }
            this.CalculateLongReentry(stockSerie, longStopSerie, shortStopSerie);

            if (stockSerie.Count <= 4)
                return;

            float previousHigh = stockSerie.GetSerie(StockDataType.HIGH).GetMax(0, 4);
            float previousLow = stockSerie.GetSerie(StockDataType.LOW).GetMin(0, 4);
            float previousHigh2 = previousHigh;
            float previousLow2 = previousLow;
            bool waitingForEndOfTrend = false;
            bool isBullish = false;
            bool isBearish = false;
            bool afterLH = false;
            bool afterHL = false;
            for (int i = 5; i < stockSerie.Count; i++)
            {
                if (!float.IsNaN(longStopSerie[i]) && float.IsNaN(longStopSerie[i - 1]))
                {
                    this.Events[0][i] = true; // SupportDetected

                    if (waitingForEndOfTrend)
                    {
                        this.Events[3][i] = true; // EndOfTrend
                        waitingForEndOfTrend = false;
                    }

                    if (longStopSerie[i] > previousLow)
                    {
                        this.Events[4][i] = true; // HigherLow
                        afterHL = true;
                        if (afterLH)
                            this.Events[8][i] = true; // LH_HL

                        if (longStopSerie[i] > previousHigh2)
                        {
                            this.Events[2][i] = true; // PB
                            waitingForEndOfTrend = true;
                        }
                    }
                    else
                    {
                        afterHL = false;
                    }
                    previousLow2 = previousLow;
                    previousLow = longStopSerie[i];
                }
                if ((float.IsNaN(longStopSerie[i]) && !float.IsNaN(longStopSerie[i - 1])) || (!float.IsNaN(shortStopSerie[i]) && float.IsNaN(shortStopSerie[i - 1])))
                {
                    this.Events[1][i] = true; // ResistanceDetected

                    if (waitingForEndOfTrend)
                    {
                        this.Events[3][i] = true; // EndOfTrend
                        waitingForEndOfTrend = false;
                    }

                    if (shortStopSerie[i] < previousHigh)
                    {
                        this.Events[5][i] = true; // LowerHigh
                        afterLH = true;
                        if (afterHL)
                            this.Events[9][i] = true; // HL_LH
                        if (shortStopSerie[i] < previousLow2)
                        {
                            this.Events[2][i] = true; // PB
                            waitingForEndOfTrend = true;
                        }
                    }
                    else
                    {
                        afterLH = false;
                    }
                    previousHigh2 = previousHigh;
                    previousHigh = shortStopSerie[i];
                }

                bool supportBroken = float.IsNaN(longStopSerie[i]) && !float.IsNaN(longStopSerie[i - 1]);
                bool resistanceBroken = float.IsNaN(shortStopSerie[i]) && !float.IsNaN(shortStopSerie[i - 1]);

                if (isBullish)
                {
                    isBullish = !supportBroken;
                }
                else
                {
                    isBullish = !float.IsNaN(longStopSerie[i]) && float.IsNaN(shortStopSerie[i]);
                }
                if (isBearish)
                {
                    isBearish = !resistanceBroken;
                }
                else
                {
                    isBearish = float.IsNaN(longStopSerie[i]) && !float.IsNaN(shortStopSerie[i]);
                }

                this.Events[6][i] = isBullish;
                this.Events[7][i] = isBearish;
            }
        }

        protected int ReentryPeriod { get; set; } = 6;

        private void CalculateLongReentry(StockSerie stockSerie, FloatSerie longStop, FloatSerie shortStop)
        {
            float alpha = 2.0f / (ReentryPeriod + 1f);
            var longReentrySerie = new FloatSerie(stockSerie.Count, this.SerieNames[2], float.NaN);
            this.Series[2] = longReentrySerie;
            var reentryRangeSerie = new FloatSerie(stockSerie.Count, this.ExtraNames[0], float.NaN);
            this.Extras[0] = reentryRangeSerie;
            var distToReentrySerie = new FloatSerie(stockSerie.Count, this.ExtraNames[1], float.NaN);
            this.Extras[1] = distToReentrySerie;
            var barsInReentrySerie = new FloatSerie(stockSerie.Count, this.ExtraNames[2], float.NaN);
            this.Extras[2] = barsInReentrySerie;

            if (this.ReentryPeriod == 0)
                return;

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            float longReentry = float.NaN;
            float previousResistance = float.MinValue;

            int barsInReentry = 0;
            float longReentryLow = float.NaN;
            float previousHigh = float.NaN;
            for (int i = ReentryPeriod; i < stockSerie.Count; i++)
            {
                if (!float.IsNaN(longStop[i])) // Bullish
                {
                    if (float.IsNaN(longReentry))
                    {
                        previousHigh = highSerie[i - 1];
                        if (previousHigh > highSerie[i] && float.IsNaN(longReentrySerie[i - 1])) // Starting LongReentry resistance.
                        {
                            longReentry = previousHigh;
                            longReentrySerie[i] = longReentry;

                            longReentryLow = lowSerie[i];
                            reentryRangeSerie[i] = (previousHigh - longReentryLow) / previousHigh;
                            distToReentrySerie[i] = (longReentry - closeSerie[i]) / longReentry;
                            barsInReentrySerie[i] = barsInReentry = 1;
                        }
                    }
                    else
                    {
                        if (closeSerie[i] > longReentry)
                        {
                            this.stockTexts.Add(new StockText
                            {
                                AbovePrice = false,
                                Index = i,
                                Text = longReentry > previousResistance ? "HBO" : "LBO"
                            });
                            longReentrySerie[i] = longReentry;
                            previousResistance = longReentry;
                            longReentry = float.NaN;
                            this.Events[10][i] = true;

                            reentryRangeSerie[i] = (previousHigh - longReentryLow) / previousHigh;
                            barsInReentrySerie[i] = ++barsInReentry;
                        }
                        else
                        {
                            longReentry = Math.Min(longReentry, longReentry + alpha * (highSerie[i] - longReentry));
                            longReentrySerie[i] = longReentry;

                            longReentryLow = Math.Min(lowSerie[i], longReentryLow);
                            reentryRangeSerie[i] = (previousHigh - longReentryLow) / previousHigh;
                            distToReentrySerie[i] = (longReentry - closeSerie[i]) / longReentry;
                            barsInReentrySerie[i] = ++barsInReentry;
                            this.Events[11][i] = true;
                        }
                    }
                }
                else // Bearish
                {
                    previousResistance = longReentry;
                    longReentry = float.NaN;
                }
            }
        }
        #region IStockText implementation
        protected List<StockText> stockTexts = new List<StockText>();
        public List<StockText> StockTexts => stockTexts;
        #endregion
    }
}