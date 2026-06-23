using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using StockAnalyzerApp.StockData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StockAnalyzer.StockData
{
    public class DataSerie
    {
        public StockInstrument Instrument { get; set; }
        public DataSerie(StockInstrument instrument, BarDuration barDuration, StockDailyValue[] values)
        {
            this.Instrument = instrument;
            this.StockName = instrument.DisplayName;
            this.BarDuration = barDuration;
            this.Values = values;

            this.ValueSeries = new FloatSerie[Enum.GetValues(typeof(StockDataType)).Length];
        }
        public StockDailyValue[] Values;
        public StockDailyValue this[DateTime key] => this.Values?.FirstOrDefault(v => v.DATE == key);

        public string StockName { get; set; }

        public BarDuration BarDuration { get; set; }
        public int LastIndex => Values == null ? -1 : Values.Length - 1;
        public StockDailyValue LastValue => Values == null || Values.Length == 0 ? null : Values[Values.Length - 1];

        public int Count => Values == null ? 0 : Values.Length;
        public bool HasVolume { get; private set; }


        public FloatSerie[] ValueSeries { get; set; }
        public FloatSerie GetSerie(StockDataType dataType)
        {
            if (ValueSeries[(int)dataType] == null)
            {
                switch (dataType)
                {
                    case StockDataType.CLOSE:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.CLOSE).ToArray(), "CLOSE");
                        break;
                    case StockDataType.OPEN:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.OPEN).ToArray(), "OPEN");
                        break;
                    case StockDataType.HIGH:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.HIGH).ToArray(), "HIGH");
                        break;
                    case StockDataType.LOW:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.LOW).ToArray(), "LOW");
                        break;
                    case StockDataType.BODYHIGH:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.BodyHigh).ToArray(), "BODYHIGH");
                        break;
                    case StockDataType.BODYLOW:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.BodyLow).ToArray(), "BODYLOW");
                        break;
                    case StockDataType.VARIATION:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.VARIATION).ToArray(), "VARIATION");
                        break;
                    case StockDataType.ATR:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.ATR).ToArray(), "ATR");
                        break;
                    case StockDataType.ADR:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.ADR).ToArray(), "ADR");
                        break;
                    case StockDataType.VOLUME:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.VOLUME * 1.0f).ToArray(), "VOLUME");
                        break;
                    case StockDataType.EXCHANGED:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.EXCHANGED).ToArray(), "EXCHANGED");
                        break;
                }
            }

            return ValueSeries[(int)dataType];
        }



        #region Inficator Management


        public Dictionary<string, IStockIndicator> IndicatorCache { get; set; }
        protected Dictionary<string, IStockCloud> CloudCache { get; set; }
        public IStockTrailStop TrailStopCache { get; set; }
        public IStockPaintBar PaintBarCache { get; set; }
        public IStockAutoDrawing AutoDrawingCache { get; set; }
        protected Dictionary<string, IStockDecorator> DecoratorCache { get; set; }
        protected IStockTrail TrailCache { get; set; }

        public IStockTrailStop GetTrailStop(String trailStopName)
        {
            if (this.TrailStopCache != null && this.TrailStopCache.Name == trailStopName)
            {
                return this.TrailStopCache;
            }
            else
            {
                IStockTrailStop trailStop = StockTrailStopManager.CreateTrailStop(trailStopName);
                if (trailStop != null && (this.HasVolume || !trailStop.RequiresVolumeData))
                {
                    StockLog.Write($"{trailStopName} to {this.StockName} - {this.BarDuration}");
                    trailStop.ApplyTo(this);
                    this.TrailStopCache = trailStop;
                    return trailStop;
                }
                return null;
            }
        }
        public IStockIndicator GetIndicator(String indicatorName)
        {
            if (this.IndicatorCache.ContainsKey(indicatorName))
            {
                return this.IndicatorCache[indicatorName];
            }
            else
            {
                IStockIndicator indicator = StockIndicatorManager.CreateIndicator(indicatorName);
                if (indicator != null && (this.HasVolume || !indicator.RequiresVolumeData))
                {
                    StockLog.Write($"{indicatorName} to {this.StockName} - {this.BarDuration}", false);
                    indicator.ApplyTo(this);
                    AddIndicatorSerie(indicator, indicatorName);
                    return indicator;
                }
                return null;
            }
        }

        public IStockCloud GetCloud(String cloudName)
        {
            if (this.CloudCache.ContainsKey(cloudName))
            {
                return this.CloudCache[cloudName];
            }
            else
            {
                IStockCloud cloud = StockCloudManager.CreateCloud(cloudName);
                if (cloud != null && (this.HasVolume || !cloud.RequiresVolumeData))
                {
                    StockLog.Write($"{cloudName} to {this.StockName} - {this.BarDuration}");
                    cloud.ApplyTo(this);
                    AddCloudSerie(cloud);
                    return cloud;
                }
                return null;
            }
        }
        public IStockPaintBar GetPaintBar(String paintBarName)
        {
            if (this.PaintBarCache != null && this.PaintBarCache.Name == paintBarName)
            {
                return this.PaintBarCache;
            }
            else
            {
                IStockPaintBar paintBar = StockPaintBarManager.CreatePaintBar(paintBarName);
                if (paintBar != null && (this.HasVolume || !paintBar.RequiresVolumeData))
                {
                    StockLog.Write($"{paintBarName} to {this.StockName} - {this.BarDuration}");
                    paintBar.ApplyTo(this);

                    this.PaintBarCache = paintBar;
                    return paintBar;
                }
                return null;
            }
        }
        public IStockAutoDrawing GetAutoDrawing(String autoDrawingName)
        {
            if (this.AutoDrawingCache != null && this.AutoDrawingCache.Name == autoDrawingName)
            {
                return this.AutoDrawingCache;
            }
            else
            {
                IStockAutoDrawing autoDrawing = StockAutoDrawingManager.CreateAutoDrawing(autoDrawingName);
                if (autoDrawing != null && (this.HasVolume || !autoDrawing.RequiresVolumeData))
                {
                    StockLog.Write($"{autoDrawingName} to {this.StockName} - {this.BarDuration}");
                    autoDrawing.ApplyTo(this);

                    this.AutoDrawingCache = autoDrawing;
                    return autoDrawing;
                }
                return null;
            }
        }
        public IStockDecorator GetDecorator(string decoratorName, string decoratedItem)
        {
            string fullDecoratorName = decoratorName + "|" + decoratedItem;
            if (this.DecoratorCache.ContainsKey(fullDecoratorName))
            {
                return this.DecoratorCache[fullDecoratorName];
            }
            else
            {
                IStockDecorator decorator = StockDecoratorManager.CreateDecorator(decoratorName, decoratedItem);
                if (decorator != null && (this.HasVolume || !decorator.RequiresVolumeData))
                {
                    StockLog.Write($"{decoratorName} to {this.StockName} - {this.BarDuration}");
                    decorator.ApplyTo(this);
                    this.DecoratorCache.Add(fullDecoratorName, decorator);
                    return decorator;
                }
                return null;
            }
        }
        public IStockTrail GetTrail(string trailName, string trailedItem)
        {
            if (this.TrailCache != null && this.TrailCache.Name == trailName && this.TrailCache.TrailedItem == trailedItem)
            {
                return this.TrailCache;
            }
            else
            {
                IStockTrail trail = StockTrailManager.CreateTrail(trailName, trailedItem);
                if (trail != null && (this.HasVolume || !trail.RequiresVolumeData))
                {
                    StockLog.Write($"{trailName} to {this.StockName} - {this.BarDuration}");
                    trail.ApplyTo(this);
                    this.TrailCache = trail;
                    return trail;
                }
                return null;
            }
        }

        public IStockViewableSeries GetViewableItem(string name)
        {
            string[] nameFields = name.Split('|');
            switch (nameFields[0])
            {
                case "INDICATOR":
                    return this.GetIndicator(nameFields[1]);
                case "CLOUD":
                    return this.GetCloud(nameFields[1]);
                case "TRAILSTOP":
                    return this.GetTrailStop(nameFields[1]);
                case "TRAIL":
                    return this.GetTrail(nameFields[1], nameFields[2]);
                case "PAINTBAR":
                    return this.GetPaintBar(nameFields[1]);
                case "AUTODRAWING":
                    return this.GetAutoDrawing(nameFields[1]);
                case "DECORATOR":
                    return this.GetDecorator(nameFields[1], nameFields[2]);
            }
            throw new ArgumentException("No viewable item matching " + name + " has been found");
        }

        public void AddIndicatorSerie(IStockIndicator indicator, string name)
        {
            if (this.IndicatorCache.ContainsKey(name))
            {
                this.IndicatorCache[name] = indicator;
            }
            else
            {
                this.IndicatorCache.Add(name, indicator);
            }
        }
        public void AddCloudSerie(IStockCloud indicator)
        {
            if (this.CloudCache.ContainsKey(indicator.Name))
            {
                this.CloudCache[indicator.Name] = indicator;
            }
            else
            {
                this.CloudCache.Add(indicator.Name, indicator);
            }
        }

        #endregion


    }
}
