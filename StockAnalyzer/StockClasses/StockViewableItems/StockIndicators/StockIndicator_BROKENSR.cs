using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockData;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_BROKENSR : StockIndicatorBase
    {
        public StockIndicator_BROKENSR()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override string Name => "BROKENSR()";

        public override string Definition => "BROKENSR()";
        public override object[] ParameterDefaultValues => new Object[] { };
        public override ParamRange[] ParameterRanges => new ParamRange[] { };
        public override string[] ParameterNames => new string[] { };


        public override string[] SerieNames => new string[] { };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { };
                return seriePens;
            }
        }

        public override void ApplyTo(DataSerie dataSerie)
        {
            this.CreateEventSeries(dataSerie.Count);

            FloatSerie closeSerie = dataSerie.GetSerie(StockDataType.CLOSE);
            for (int i = 0; i < closeSerie.Count; i++)
            {
                this.eventSeries[2][i] = true;
            }
            this.eventSeries[2][closeSerie.Count - 1] = dataSerie.LastValue.IsComplete;

            if (!dataSerie.Instrument.StockAnalysis.DrawingItems.ContainsKey(dataSerie.BarDuration))
                return;

            // Detecting events
            for (int i = 2; i < closeSerie.Count; i++)
            {
                foreach (DrawingItem di in dataSerie.Instrument.StockAnalysis.DrawingItems[dataSerie.BarDuration])
                {
                    Line2DBase line = di as Line2DBase;

                    if (line != null)
                    {
                        bool yesterAbove = line.IsAbovePoint(new PointF(i - 1, closeSerie[i - 1]));
                        bool todayAbove = line.IsAbovePoint(new PointF(i, closeSerie[i]));
                        if (yesterAbove && !todayAbove)
                        {
                            this.eventSeries[1][i] |= true;
                            break;
                        }
                        else if (!yesterAbove && todayAbove)
                        {
                            this.eventSeries[0][i] |= true;
                            break;
                        }
                    }
                }
            }
        }

        static readonly string[] eventNames = new string[] { "BrokenSupport", "BrokenResistance", "BarComplete" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, false };
        public override bool[] IsEvent => isEvent;
    }
}


