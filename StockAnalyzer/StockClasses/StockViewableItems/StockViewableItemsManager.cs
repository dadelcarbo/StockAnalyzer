﻿using StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems
{
    public class StockViewableItemsManager
    {
        public static List<string> IndicatorTypes = new List<string>() { "Indicator", "TrailStop", "Cloud", "AutoDrawing" };

        static public IStockViewableSeries GetViewableItem(string fullString)
        {
            return GetViewableItem(fullString, null);
        }

        static public bool Supports(string fullString)
        {
            if (string.IsNullOrEmpty(fullString))
                return false;
            string[] fields = fullString.Split('|');
            if (fields.Length < 2 || string.IsNullOrEmpty(fields[1]))
                return false;

            switch (fields[0].ToUpper())
            {
                case "INDICATOR":
                    return StockIndicatorManager.Supports(fields[1]);
                case "CLOUD":
                    return StockCloudManager.Supports(fields[1]);
                case "PAINTBAR":
                    return StockPaintBarManager.Supports(fields[1]);
                case "AUTODRAWING":
                    return StockAutoDrawingManager.Supports(fields[1]);
                case "TRAILSTOP":
                    return StockTrailStopManager.Supports(fields[1]);
                default:
                    return false;
            }
        }

        static public IStockViewableSeries GetViewableItem(string fullString, StockSerie stockSerie)
        {
            if (string.IsNullOrEmpty(fullString))
                return null;
            string[] fields = fullString.Split('|');
            if (fields.Length < 2 || string.IsNullOrEmpty(fields[1]))
                return null;
            int offset = 2;
            IStockViewableSeries viewableSerie = null;
            switch (fields[0].ToUpper())
            {
                case "INDICATOR":
                    if (stockSerie == null)
                    {
                        viewableSerie = StockIndicatorManager.CreateIndicator(fields[1]);
                    }
                    else
                    {
                        viewableSerie = stockSerie.GetIndicator(fields[1]);
                    }
                    offset = 2;
                    break;
                case "CLOUD":
                    if (stockSerie == null)
                    {
                        viewableSerie = StockCloudManager.CreateCloud(fields[1]);
                    }
                    else
                    {
                        viewableSerie = stockSerie.GetCloud(fields[1]);
                    }
                    offset = 2;
                    break;
                case "PAINTBAR":
                    if (stockSerie == null)
                    {
                        viewableSerie = StockPaintBarManager.CreatePaintBar(fields[1]);
                    }
                    else
                    {
                        viewableSerie = stockSerie.GetPaintBar(fields[1]);
                    }
                    offset = 2;
                    break;
                case "AUTODRAWING":
                    if (stockSerie == null)
                    {
                        viewableSerie = StockAutoDrawingManager.CreateAutoDrawing(fields[1]);
                    }
                    else
                    {
                        viewableSerie = stockSerie.GetAutoDrawing(fields[1]);
                    }
                    offset = 2;
                    break;
                case "TRAILSTOP":
                    if (stockSerie == null)
                    {
                        viewableSerie = StockTrailStopManager.CreateTrailStop(fields[1]);
                    }
                    else
                    {
                        viewableSerie = stockSerie.GetTrailStop(fields[1]);
                    }
                    offset = 2;
                    break;
                case "DECORATOR":
                    if (stockSerie == null)
                    {
                        viewableSerie = StockDecoratorManager.CreateDecorator(fields[1], fields[2]);
                    }
                    else
                    {
                        viewableSerie = stockSerie.GetDecorator(fields[1], fields[2]);
                    }
                    offset = 3;
                    break;
                case "TRAIL":
                    if (stockSerie == null)
                    {
                        viewableSerie = StockTrailManager.CreateTrail(fields[1], fields[2]);
                    }
                    else
                    {
                        viewableSerie = stockSerie.GetTrail(fields[1], fields[2]);
                    }
                    offset = 3;
                    break;
                default:
                    return null;
            }

            if (viewableSerie != null)
            {
                int index = 0;
                for (int i = 0; i < viewableSerie.SeriesCount; i++)
                {
                    index = 2 * i + offset;
                    if (index < fields.Length)
                    {
                        viewableSerie.SeriePens[i] = GraphCurveType.PenFromString(fields[index]);
                        viewableSerie.SerieVisibility[i] = bool.Parse(fields[index + 1]);
                    }
                    else
                    {
                        viewableSerie.SerieVisibility[i] = true;
                    }
                }
                if (viewableSerie is IStockIndicator)
                {
                    var indicator = (IStockIndicator)viewableSerie;
                    if (indicator?.Areas != null)
                    {
                        foreach (var area in indicator.Areas)
                        {
                            index += 2;
                            if (index < fields.Length)
                            {
                                area.Color = GraphCurveType.ColorFromString(fields[index]);
                                area.Visibility = bool.Parse(fields[index + 1]);
                            }
                        }
                    }
                }
                if (fields[0].ToUpper() == "DECORATOR")
                {
                    offset += viewableSerie.SeriesCount * 2;
                    IStockDecorator decorator = viewableSerie as IStockDecorator;
                    for (int i = 0; i < decorator.EventCount; i++)
                    {
                        index = 2 * i + offset;
                        if (index < fields.Length)
                        {
                            decorator.EventPens[i] = GraphCurveType.PenFromString(fields[index]);
                            decorator.EventVisibility[i] = bool.Parse(fields[index + 1]);
                        }
                        else
                        {
                            decorator.EventVisibility[i] = true;
                        }
                    }
                }
            }
            return viewableSerie;
        }
        static public IStockViewableSeries CreateInitialisedFrom(IStockViewableSeries aViewableSerie, StockSerie stockSerie)
        {
            if (!stockSerie.Initialise()) return null;

            IStockViewableSeries viewableSerie = null;
            switch (aViewableSerie.Type)
            {
                case ViewableItemType.Indicator:
                    viewableSerie = stockSerie.GetIndicator(aViewableSerie.Name);
                    break;
                case ViewableItemType.Decorator:
                    viewableSerie = stockSerie.GetDecorator(aViewableSerie.Name, ((IStockDecorator)aViewableSerie).DecoratedItem);
                    break;
                case ViewableItemType.PaintBar:
                    viewableSerie = stockSerie.GetPaintBar(aViewableSerie.Name);
                    break;
                case ViewableItemType.TrailStop:
                    viewableSerie = stockSerie.GetTrailStop(aViewableSerie.Name);
                    break;
                case ViewableItemType.Trail:
                    viewableSerie = stockSerie.GetTrail(aViewableSerie.Name, ((IStockTrail)aViewableSerie).TrailedItem);
                    break;
                case ViewableItemType.Cloud:
                    viewableSerie = stockSerie.GetCloud(aViewableSerie.Name);
                    break;
                case ViewableItemType.AutoDrawing:
                    viewableSerie = stockSerie.GetAutoDrawing(aViewableSerie.Name);
                    break;
                default:
                    throw new NotImplementedException($"ItemType not Implemented {aViewableSerie.Type}");
            }
            return viewableSerie;
        }

        static public string GetTheme(string fullString)
        {
            string theme = ThemeTemplate;

            IStockViewableSeries indicator = StockViewableItemsManager.GetViewableItem(fullString);
            if (indicator != null)
            {
                theme = AppendThemeLine(indicator, theme);
            }
            return theme;
        }

        const string ThemeTemplate = @"#ScrollGraph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|Line
DATA|CLOSE|1:255:0:0:0:Solid|True
#CloseGraph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|CandleStick|1:255:184:134:11:Solid
DATA|CLOSE|1:255:0:0:0:Solid|True
@PriceIndicator
SECONDARY|NONE
#Indicator1Graph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart
@Indicator1
#Indicator2Graph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart
@Indicator2
#Indicator3Graph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart
@Indicator3
#VolumeGraph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart";


        private static int indCount = 1;

        private static string AppendThemeLine(IStockViewableSeries indicator, string theme)
        {
            int index = -1;
            if (indicator.DisplayTarget == IndicatorDisplayTarget.PriceIndicator)
            {
                index = theme.IndexOf("@PriceIndicator");
                theme = theme.Insert(index, indicator.ToThemeString() + System.Environment.NewLine);
            }
            else
            {
                if (indicator is IStockIndicator)
                {
                    IStockIndicator stockIndicator = indicator as IStockIndicator;
                    if (stockIndicator.HorizontalLines != null && stockIndicator.HorizontalLines.Count() > 0)
                    {
                        foreach (HLine hline in stockIndicator.HorizontalLines)
                        {
                            theme = theme.Replace("@Indicator" + indCount,
                               "LINE|" + hline.Level.ToString() + "|" + GraphCurveType.PenToString(hline.LinePen) +
                               System.Environment.NewLine + "@Indicator" + indCount);
                        }
                    }
                }
                if (indicator is IStockDecorator)
                {
                    IStockDecorator decorator = indicator as IStockDecorator;
                    IStockIndicator decoratedIndicator = StockIndicatorManager.CreateIndicator(decorator.DecoratedItem);
                    theme = theme.Replace("@Indicator" + indCount,
                       decoratedIndicator.ToThemeString() + System.Environment.NewLine + "@Indicator" + indCount);
                    if (decoratedIndicator.HorizontalLines != null && decoratedIndicator.HorizontalLines.Count() > 0)
                    {
                        foreach (HLine hline in decoratedIndicator.HorizontalLines)
                        {
                            theme = theme.Replace("@Indicator" + indCount,
                               "LINE|" + hline.Level.ToString() + "|" + GraphCurveType.PenToString(hline.LinePen) +
                               System.Environment.NewLine + "@Indicator" + indCount);
                        }
                    }
                }
                if (indicator is IStockTrail)
                {
                    IStockTrail trail = indicator as IStockTrail;
                    IStockIndicator decoratedIndicator = StockIndicatorManager.CreateIndicator(trail.TrailedItem);
                    theme = theme.Replace("@Indicator" + indCount,
                       decoratedIndicator.ToThemeString() + System.Environment.NewLine + "@Indicator" + indCount);
                    if (decoratedIndicator.HorizontalLines != null && decoratedIndicator.HorizontalLines.Count() > 0)
                    {
                        foreach (HLine hline in decoratedIndicator.HorizontalLines)
                        {
                            theme = theme.Replace("@Indicator" + indCount,
                               "LINE|" + hline.Level.ToString() + "|" + GraphCurveType.PenToString(hline.LinePen) +
                               System.Environment.NewLine + "@Indicator" + indCount);
                        }
                    }
                }
                theme = theme.Replace("@Indicator" + indCount, indicator.ToThemeString());

                indCount++;
            }
            return theme;
        }
    }
}
