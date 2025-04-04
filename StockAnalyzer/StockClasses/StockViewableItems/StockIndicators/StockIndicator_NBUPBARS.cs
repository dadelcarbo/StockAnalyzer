﻿using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_NBUPBARS : StockIndicatorBase
    {
        public StockIndicator_NBUPBARS()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override string Name => "NBUPBARS(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")";

        public override string Definition => "NBUPBARS(int Period, int Smoothing)";
        public override object[] ParameterDefaultValues => new Object[] { 20, 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Period", "Smoothing" };
        public override string[] SerieNames => new string[] { "NBUPBARS(" + this.Parameters[0].ToString() + ")" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Blue) };
                return seriePens;
            }
        }
        static HLine[] lines = null;
        public override HLine[] HorizontalLines
        {
            get
            {
                if (lines == null)
                {
                    lines = new HLine[] { new HLine(0.0f, new Pen(Color.Gray)) };
                    lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                }
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            FloatSerie nbUpDaysSerie = new FloatSerie(stockSerie.Count, "NBUPDAYS");
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            float nbUpDays = 0;
            for (int i = 1; i <= period; i++)
            {
                if (closeSerie[i] >= closeSerie[i - 1])
                {
                    nbUpDays++;
                }
                nbUpDaysSerie[i] = nbUpDays / i;
            }

            for (int i = period + 1; i < closeSerie.Count; i++)
            {
                nbUpDays = 0;
                for (int j = i - period + 1; j <= i; j++)
                {
                    if (closeSerie[j] >= closeSerie[j - 1])
                    {
                        nbUpDays++;
                    }
                }
                nbUpDaysSerie[i] = nbUpDays / period;
            }
            this.Series[0] = nbUpDaysSerie.CalculateMA((int)this.parameters[1]).Sub(0.5f);
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
