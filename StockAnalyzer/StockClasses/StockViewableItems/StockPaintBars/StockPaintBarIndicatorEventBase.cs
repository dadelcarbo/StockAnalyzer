﻿using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public abstract class StockPaintBarIndicatorEventBase : Parameterizable, IStockPaintBar, IStockText
    {
        protected IStockIndicator baseIndicator;

        public StockPaintBarIndicatorEventBase()
        {
            baseIndicator = StockIndicatorManager.CreateIndicator(this.ShortName);
            if (baseIndicator == null)
            {
                throw new ApplicationException("Unable to create " + this.ShortName + " indicator");
            }
            this.parameters = baseIndicator.ParameterDefaultValues;

            this.serieVisibility = new bool[this.SeriesCount];
            for (int i = 0; i < this.SeriesCount; this.serieVisibility[i++] = false) ;
        }

        public override string Name => ((IStockViewableSeries)baseIndicator).Name;

        public IndicatorDisplayTarget DisplayTarget => baseIndicator.DisplayTarget;
        public IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.PaintBar;
        public ViewableItemType Type => ViewableItemType.PaintBar;

        public virtual bool RequiresVolumeData => baseIndicator.RequiresVolumeData;

        public string ToThemeString()
        {
            string themeString = "PAINTBAR|" + this.Name;
            for (int i = 0; i < this.SeriesCount; i++)
            {
                themeString += "|" + GraphCurveType.PenToString(this.SeriePens[i]) + "|" + this.SerieVisibility[i].ToString();
            }
            return themeString;
        }

        public override string Definition => this.baseIndicator.Definition;

        public override string[] ParameterNames => this.baseIndicator.ParameterNames;
        public override object[] ParameterDefaultValues
        {
            get
            {
                if (this.baseIndicator == null) return null;
                else return this.baseIndicator.ParameterDefaultValues;
            }
        }
        public override object[] Parameters
        {
            get
            {
                if (this.baseIndicator == null) return null;
                else return this.baseIndicator.Parameters;
            }
        }

        public override ParamRange[] ParameterRanges => this.baseIndicator.ParameterRanges;

        public override string[] SerieNames => EventNames;

        abstract public Pen[] SeriePens { get; }

        protected bool[] serieVisibility;
        public bool[] SerieVisibility => this.serieVisibility;

        public void Initialise(string[] parameters)
        {
            ((Parameterizable)baseIndicator).ParseInputParameters(parameters);
        }

        public virtual void ApplyTo(StockSerie stockSerie)
        {
            this.baseIndicator = stockSerie.GetIndicator(this.Name);
        }

        #region IStockEvent implementation

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
        public string[] EventNames
        {
            get
            {
                if (baseIndicator == null)
                {
                    return null;
                }
                else
                { return baseIndicator.EventNames; }
            }
        }

        public BoolSerie[] Events
        {
            get
            {
                if (baseIndicator == null)
                {
                    return null;
                }
                else
                { return baseIndicator.Events; }
            }
        }
        public BoolSerie GetEvents(string eventName)
        {
            int index = Array.IndexOf(this.EventNames, eventName);
            return index != -1 ? this.Events[index] : null;
        }

        public bool[] IsEvent { get; set; }
        #endregion

        #region IStockText Implementation
        public List<StockText> StockTexts { get; set; }
        #endregion
    }
}
