﻿using System;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILFW : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return true; } }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "NbATRStop" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 5f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(0, 500), new ParamRangeFloat(0f, 20f) }; }
        }

        public override string[] SerieNames { get { return new string[] { "TRAILFW.LS", "TRAILFW.SS" }; } }

        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.Parameters[0];
            var nbAtr = (float)this.Parameters[1];
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[1], float.NaN);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);
            FloatSerie volumeSerie = stockSerie.GetSerie(StockDataType.VOLUME);
            FloatSerie highestSerie = stockSerie.GetIndicator($"HIGHEST({period})").Series[0];
            FloatSerie emaSerie = stockSerie.GetIndicator($"EMA({period})").Series[0];
            FloatSerie natrSerie = stockSerie.GetIndicator("NATR(14)").Series[0];

            bool holding = false;
            float trail = float.NaN;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (holding)
                {
                    if (closeSerie[i] < trail)
                    {
                        holding = false;
                        trail = float.NaN;
                    }
                    else
                    {
                        trail = Math.Max(trail, closeSerie[i] * (1.0f - 0.01f * nbAtr * natrSerie[i]));
                    }
                }
                else
                {
                    if (closeSerie[i] < 2.0f)
                        continue;
                    if (highestSerie[i] < period)
                        continue;
                    if (closeSerie[i] * volumeSerie[i] < 1000000)
                        continue;
                    if ((volumeSerie[i] - volumeSerie[i - 1]) / volumeSerie[i - 1] < 0.5f)
                        continue;
                    var variation = (closeSerie[i] - openSerie[i]) / openSerie[i];
                    if (variation < 0.05f || variation > 0.2f)
                        continue;
                    if (closeSerie[i] < emaSerie[i])
                        continue;
                    if (natrSerie[i] > 8.0f)
                        continue;

                    trail = closeSerie[i] * (1.0f - 0.01f * nbAtr * natrSerie[i]);

                    holding = true;
                }
                longStopSerie[i] = trail;
            }

            this.Series[0] = longStopSerie;
            this.Series[0].Name = longStopSerie.Name;
            this.Series[1] = shortStopSerie;
            this.Series[1].Name = shortStopSerie.Name;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}