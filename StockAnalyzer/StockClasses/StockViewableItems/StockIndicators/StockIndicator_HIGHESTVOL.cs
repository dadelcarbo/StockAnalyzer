﻿using System;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HIGHESTVOL : StockIndicatorBase
    {
        public override string Definition => "Calculate the number of bars the current bar volume is the highest.\r\nEvent is raised when gap with previous value exceeds the trigger parameter.";
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override bool RequiresVolumeData => true;

        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 20 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Trigger" }; }
        }

        public override string[] SerieNames { get { return new string[] { "HIGHEST(" + this.Parameters[0].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) };
                }
                return seriePens;
            }
        }
        public override HLine[] HorizontalLines
        {
            get { return null; }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);
            FloatSerie indexSerie = new FloatSerie(stockSerie.Count);
            this.series[0] = indexSerie;
            this.Series[0].Name = this.Name;

            int trigger = (int)this.Parameters[0];

            var volumeSerie = stockSerie.GetSerie(StockDataType.VOLUME);

            for (int i = trigger; i < stockSerie.Count; i++)
            {
                int count = 0;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (volumeSerie[i] > volumeSerie[j])
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                indexSerie[i] = count;

                if (indexSerie[i] - indexSerie[i - 1] >= trigger)
                {
                    this.eventSeries[0][i] = true;
                }
            }
        }
        static string[] eventNames = new string[] { "NewHighVolume" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}