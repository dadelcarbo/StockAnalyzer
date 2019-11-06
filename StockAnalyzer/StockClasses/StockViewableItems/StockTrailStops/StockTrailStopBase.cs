using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public abstract class StockTrailStopBase : Parameterizable, IStockTrailStop
    {
        public StockTrailStopBase()
        {
            this.series = new FloatSerie[this.SeriesCount];
            if (EventCount != 0)
            {
                this.eventSeries = new BoolSerie[this.EventCount];
            }
            this.serieVisibility = new bool[this.SeriesCount];
            for (int i = 0; i < this.SeriesCount; this.serieVisibility[i++] = true) ;
        }

        public abstract IndicatorDisplayTarget DisplayTarget { get; }
        public IndicatorDisplayStyle DisplayStyle
        {
            get { return IndicatorDisplayStyle.TrailStop; }
        }
        public ViewableItemType Type { get { return ViewableItemType.TrailStop; } }
        public virtual bool RequiresVolumeData { get { return false; } }

        public string ToThemeString()
        {
            string themeString = "TRAILSTOP|" + this.Name;
            for (int i = 0; i < this.SeriesCount; i++)
            {
                themeString += "|" + GraphCurveType.PenToString(this.SeriePens[i]) + "|" + this.SerieVisibility[i].ToString();
            }
            return themeString;
        }

        protected FloatSerie[] series;
        public FloatSerie[] Series { get { return series; } }

        public virtual System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2) };
                    seriePens[0].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    seriePens[1].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                }
                return seriePens;
            }
        }
        private bool[] serieVisibility;
        public bool[] SerieVisibility { get { return this.serieVisibility; } }

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

        public BoolSerie[] Events
        {
            get { return eventSeries; }
        }
        virtual protected void CreateEventSeries(int count)
        {
            for (int i = 0; i < this.EventCount; i++)
            {
                this.eventSeries[i] = new BoolSerie(count, this.EventNames[i]);
            }
        }
        #endregion


        private StockSerie.Trend[] upDownState = null;

        public StockSerie.Trend[] UpDownState
        {
            get
            {
                if (upDownState == null)
                {
                    FloatSerie longStop = this.Series[0];
                    FloatSerie shortStop = this.Series[1];
                    BoolSerie boolSerie = this.Events[0];
                    upDownState = new StockSerie.Trend[boolSerie.Count];
                    for (int i = 0; i < boolSerie.Count; i++)
                    {
                        if (!float.IsNaN(longStop[i]))
                            upDownState[i] = StockSerie.Trend.UpTrend;
                        else if (!float.IsNaN(shortStop[i]))
                            upDownState[i] = StockSerie.Trend.DownTrend;
                        else
                            upDownState[i] = StockSerie.Trend.NoTrend;
                    }
                }
                return upDownState;
            }
        }

        public static StockSerie.Trend BoolToTrend(bool? upTrend)
        {
            if (upTrend == null) return StockSerie.Trend.NoTrend;
            if (upTrend.Value) return StockSerie.Trend.UpTrend;
            return StockSerie.Trend.DownTrend;
        }

        private static string[] eventNames = new string[]
          {
             "BrokenUp", "BrokenDown",           // 0,1
             "Pullback", "EndOfTrend",           // 2,3
             "HigherLow", "LowerHigh",           // 4,5
             "Bullish", "Bearish",                // 6,7
             "LH_HL", "HL_LH"                // 8,9
          };

        public string[] EventNames => eventNames;

        private static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, false, false, true, true };
        public bool[] IsEvent => isEvent;

        protected void GenerateEvents(StockSerie stockSerie, FloatSerie longStopSerie, FloatSerie shortStopSerie)
        {
            this.CreateEventSeries(stockSerie.Count);

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
                if (!float.IsNaN(shortStopSerie[i]) && float.IsNaN(shortStopSerie[i - 1]))
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
    }
}
