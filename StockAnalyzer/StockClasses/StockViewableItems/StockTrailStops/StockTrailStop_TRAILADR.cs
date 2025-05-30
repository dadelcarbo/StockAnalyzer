﻿using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILADR : StockTrailStopBase
    {
        public override string Definition => "Draws Trail Stop based ADR Bands";

        public override string[] ParameterNames => new string[] { "Period", "ADRPeriod", "NbUpDev", "NbDownDev", "MAType" };
        public override Object[] ParameterDefaultValues => new Object[] { 30, 10, 2f, -2f, "MA" };
        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-5.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 5.0f),
                new ParamRangeMA()
                };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;

            var bandIndicator = stockSerie.GetIndicator($"ADRBAND({(int)this.parameters[0]},{(int)this.parameters[1]},{(float)this.parameters[2]},{(float)this.parameters[3]},{this.parameters[4]})");
            stockSerie.CalculateBandTrailStop(bandIndicator.Series[1], bandIndicator.Series[0], out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}
