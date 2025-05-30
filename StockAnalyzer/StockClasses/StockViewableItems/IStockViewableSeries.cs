﻿using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems
{
    public enum IndicatorDisplayTarget
    {
        PriceIndicator,             /// Indicator to be displayed as a curve in the price graph such as EMA...
        RangedIndicator,            /// Indicator to be displayed in a ranged indicator
        NonRangedIndicator          /// Indicator to be displayed with own scaling
    }
    public enum IndicatorDisplayStyle
    {
        SimpleCurve,                /// Draw as a curve like EMA
        SupportResistance,          /// Draw as support/resistance dots
        TrailStop,                  /// Draw as a trailing stop
        PaintBar,                   /// Draw as a paint bar
        DecoratorPlot,              /// Draw as a plot
        DecoratorLine,              /// Draw a a vertical line
        TrailCurve,
        AutoDrawing
    }
    public enum ViewableItemType
    {
        Indicator,
        Decorator,
        PaintBar,
        TrailStop,
        Trail,
        Cloud,
        AutoDrawing
    }
    public enum InputType
    {
        HighLow,
        Body,
        Close,
        CloseEMA
    }

    public interface IStockViewableSeries : IStockVisibility
    {
        string Name { get; }
        string ShortName { get; }
        string Definition { get; }
        bool RequiresVolumeData { get; }
        ViewableItemType Type { get; }
        IndicatorDisplayTarget DisplayTarget { get; }
        IndicatorDisplayStyle DisplayStyle { get; }

        bool HasTrendLine { get; }

        string ToThemeString();

        int ParameterCount { get; }
        string[] ParameterNames { get; }
        Type[] ParameterTypes { get; }
        Object[] Parameters { get; }
        Object[] ParameterDefaultValues { get; }
        ParamRange[] ParameterRanges { get; }

        int SeriesCount { get; }
        string[] SerieNames { get; }
        Pen[] SeriePens { get; }

        void Initialise(string[] parameters);
        void ApplyTo(StockSerie stockSerie);
    }
}
