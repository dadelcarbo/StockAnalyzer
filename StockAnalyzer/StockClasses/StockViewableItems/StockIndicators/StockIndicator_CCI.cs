﻿using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_CCI : StockIndicatorBase
    {
        public StockIndicator_CCI()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override string Definition => "CCI(int Period, int SmoothPeriod1, int SmoothPeriod2)";
        public override string[] ParameterNames => new string[] { "Period", "SmoothPeriod1", "SmoothPeriod2" };
        public override Object[] ParameterDefaultValues => new Object[] { 14, 1, 10 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "CCI(" + this.Parameters[0].ToString() + ")", "Signal" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
                return seriePens;
            }
        }
        public override HLine[] HorizontalLines
        {
            get
            {
                HLine[] lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie cciSerie = stockSerie.CalculateCCI((int)this.parameters[0]);

            int period1 = ((int)this.parameters[1]);
            int period2 = ((int)this.parameters[2]);

            cciSerie = period1 <= 1 ?
            cciSerie :
            cciSerie.CalculateEMA(period1);

            this.series[0] = cciSerie;
            this.series[0].Name = this.Name;

            FloatSerie signalSerie = cciSerie.CalculateEMA(period2);
            this.series[1] = signalSerie;
            this.series[1].Name = this.series[0].Name + "_SIGNAL";

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = 2; i < cciSerie.Count; i++)
            {
                this.eventSeries[0][i] = (cciSerie[i] > signalSerie[i]);
                this.eventSeries[1][i] = (cciSerie[i] < signalSerie[i]);
                this.eventSeries[2][i] = eventSeries[0][i] & !eventSeries[0][i - 1];
                this.eventSeries[3][i] = eventSeries[1][i] & !eventSeries[1][i - 1];
            }
        }

        static readonly string[] eventNames = new string[] { "UpTrend", "DownTrend", "BullishCrossing", "BearishCrossing" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { false, false, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
