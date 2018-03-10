using System;
using System.Drawing;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockDrawing;
using System.Collections.Generic;
using System.Xml.Serialization;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILHL2 : StockTrailStopBase
    {
        public StockTrailStop_TRAILHL2()
        {
        }
        public override string Definition
        {
            get { return "TRAILHL2(int period)"; }
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 2 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(0, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "TRAILHL.LS", "TRAILHL.SS" }; } }

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
            stockSerie.CalculateHighLowTrailStop2((int)this.Parameters[0], out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}