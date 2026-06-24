using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using StockAnalyzerApp.StockData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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
            this.StockAnalysis = new StockAnalysis();

            ResetAllCache();
        }

        public void ResetAllCache()
        {
            this.ValueSeries = new FloatSerie[Enum.GetValues(typeof(StockDataType)).Length];
            this.IndicatorCache = new Dictionary<string, IStockIndicator>();
            this.DecoratorCache = new Dictionary<string, IStockDecorator>();
            this.CloudCache = new Dictionary<string, IStockCloud>();
            this.AutoDrawingCache = null;
            this.TrailStopCache = null;
            this.TrailCache = null;
        }

        public StockDailyValue[] Values;
        public StockDailyValue this[DateTime key] => this.Values?.FirstOrDefault(v => v.DATE == key);

        public string StockName { get; set; }

        public BarDuration BarDuration { get; set; }

        public int LastIndex => Values == null ? -1 : Values.Length - 1;
        public int LastCompleteIndex => this.LastIndex > 0 ? this.Values[LastIndex].IsComplete ? this.LastIndex : this.LastIndex : -1;
        public StockDailyValue LastValue => Values == null || Values.Length == 0 ? null : Values[Values.Length - 1];

        public int Count => Values == null ? 0 : Values.Length;

        public bool HasVolume { get; private set; }

        public StockAnalysis StockAnalysis { get; set; }

        public StockDividend Dividend => null; // TODO: Implement dividend retrieval logic


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

        public void GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie highSerie, InputType inputType, int smoothingPeriod = -1)
        {
            switch (inputType)
            {
                case InputType.HighLow:
                    lowSerie = this.GetSerie(StockDataType.LOW);
                    highSerie = this.GetSerie(StockDataType.HIGH);
                    break;
                case InputType.Body:
                    lowSerie = this.GetSerie(StockDataType.BODYLOW);
                    highSerie = this.GetSerie(StockDataType.BODYHIGH);
                    break;
                case InputType.Close:
                    highSerie = lowSerie = this.GetSerie(StockDataType.CLOSE);
                    break;
                case InputType.CloseEMA:
                    if (smoothingPeriod > 1)
                    {
                        highSerie = lowSerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(smoothingPeriod);
                    }
                    else
                        throw new ArgumentOutOfRangeException(nameof(smoothingPeriod), "smoothingPeriod shall be greater than 1");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(inputType), inputType, "Unexpected enum value in GetHighLowSeries");
            }
        }


        #region Inficator Management


        public Dictionary<string, IStockIndicator> IndicatorCache { get; set; }
        protected Dictionary<string, IStockCloud> CloudCache { get; set; }
        public IStockTrailStop TrailStopCache { get; set; }
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

        /// <summary>
        /// 
        ///  %K = 100*(Close - lowest(14))/(highest(14)-lowest(14))
        ///  %D = MA3(%K)
        /// </summary>
        /// <param name="period"></param>
        /// <param name="inputType"></param>
        /// <returns></returns>
        public FloatSerie CalculateFastOscillator(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
        {
            FloatSerie fastOscillatorSerie = new FloatSerie(this.Count);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie highSerie, inputType, smoothingPeriod);


            for (int i = 0; i < Math.Min(period, this.Count); i++)
            {
                fastOscillatorSerie[i] = 50.0f;
            }
            for (int i = period; i < this.Count; i++)
            {
                var lowestLow = lowSerie.GetMin(i - period, i);
                var highestHigh = highSerie.GetMax(i - period, i);
                if (highestHigh == lowestLow)
                {
                    fastOscillatorSerie[i] = 50.0f;
                }
                else
                {
                    var close = closeSerie.Values[i];
                    if (close == highestHigh)
                        fastOscillatorSerie[i] = 100.0f;
                    else if (close == lowestLow)
                        fastOscillatorSerie[i] = 0.0f;
                    else
                        fastOscillatorSerie[i] = 100.0f * (close - lowestLow) / (highestHigh - lowestLow);
                }
            }
            fastOscillatorSerie.Name = $"FastK({period})";
            return fastOscillatorSerie;
        }

        /// <summary>
        /// 
        ///  %K = 100*(Close - lowest(14))/(highest(14)-lowest(14))
        ///  %D = MA3(%K)
        /// </summary>
        /// <param name="period"></param>
        /// <param name="inputType"></param>
        /// <returns></returns>
        public float CalculateLastFastOscillator(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            float fastOscillator = 50.0f;

            GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie highSerie, inputType, smoothingPeriod);

            int lastIndex = LastIndex;

            var lowestLow = lowSerie.GetMin(lastIndex - period, lastIndex);
            var highestHigh = highSerie.GetMax(lastIndex - period, lastIndex);
            if (highestHigh == lowestLow)
            {
                fastOscillator = 50.0f;
            }
            else
            {
                var close = closeSerie.Last;
                if (close == highestHigh)
                    fastOscillator = 100.0f;
                else if (close == lowestLow)
                    fastOscillator = 0.0f;
                else
                    fastOscillator = 100.0f * (close - lowestLow) / (highestHigh - lowestLow);
            }
            return fastOscillator;
        }


        #region Indicators calculation


        public float CalculateLastROx(string rox, int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
        {
            switch (rox.Substring(0, 3))
            {
                case "ROR":
                    return CalculateLastROR(period, inputType, smoothingPeriod);
                case "ROD":
                    return CalculateLastROD(period, inputType, smoothingPeriod);
                case "ROC":
                    return CalculateLastROC(period);
                default:
                    break;
            }
            GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie _, inputType, smoothingPeriod);

            var min = lowSerie.GetMin(this.LastIndex - period, this.LastIndex);
            return (this.LastValue.CLOSE - min) / min;
        }

        public float CalculateLastROR(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
        {
            GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie _, inputType, smoothingPeriod);

            var min = lowSerie.GetMin(this.LastIndex - period, this.LastIndex);
            return (this.LastValue.CLOSE - min) / min;
        }
        public float CalculateLastROC(int period)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            var @ref = closeSerie[this.LastIndex - period];
            return (this.LastValue.CLOSE - @ref) / @ref;
        }
        public FloatSerie CalculateRateOfRise(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie _, inputType, smoothingPeriod);

            FloatSerie serie = new FloatSerie(Values.Count());
            float min;

            for (int i = 0; i < Math.Min(period, this.Count); i++)
            {
                min = lowSerie.GetMin(0, i);
                serie[i] = (closeSerie[i] - min) / min;
            }
            for (int i = period; i < this.Count; i++)
            {
                min = lowSerie.GetMin(i - period, i);
                serie[i] = (closeSerie[i] - min) / min;
            }
            serie.Name = $"ROR_{period}";
            return serie;
        }
        public float CalculateLastROD(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
        {
            GetHighLowSeries(out FloatSerie _, out FloatSerie highSerie, inputType, smoothingPeriod);

            var max = highSerie.GetMax(this.LastIndex - period, this.LastIndex);
            return (this.LastValue.CLOSE - max) / max;
        }

        public FloatSerie CalculateRateOfDecline(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            GetHighLowSeries(out FloatSerie _, out FloatSerie highSerie, inputType, smoothingPeriod);

            FloatSerie serie = new FloatSerie(Values.Count());
            float min;

            for (int i = 0; i < Math.Min(period, this.Count); i++)
            {
                min = highSerie.GetMax(0, i);
                serie[i] = -(closeSerie[i] - min) / min;
            }
            for (int i = period; i < this.Count; i++)
            {
                min = highSerie.GetMax(i - period, i);
                serie[i] = -(closeSerie[i] - min) / min;
            }
            serie.Name = $"ROD_{period}";
            return serie;
        }

        /// <summary>
        /// Calulcate drawdown in value (euro), not %
        /// </summary>
        /// <param name="period"></param>
        /// <param name="inputType"></param>
        /// <param name="smoothingPeriod"></param>
        /// <returns></returns>
        public FloatSerie CalculateDrawdownValue(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            GetHighLowSeries(out FloatSerie _, out FloatSerie highSerie, inputType, smoothingPeriod);

            FloatSerie serie = new FloatSerie(Values.Count());
            float max;

            for (int i = 0; i < Math.Min(period, this.Count); i++)
            {
                max = highSerie.GetMax(0, i);
                serie[i] = max - closeSerie[i];
            }
            for (int i = period; i < this.Count; i++)
            {
                max = highSerie.GetMax(i - period, i);
                serie[i] = max - closeSerie[i];
            }
            serie.Name = $"DD_{period}";
            return serie;
        }

        public FloatSerie CalculateRateOfChange(int period, int smoothingPeriod = 0)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            if (smoothingPeriod > 1)
                closeSerie = closeSerie.CalculateEMA(smoothingPeriod);

            FloatSerie serie = new FloatSerie(Values.Count());
            if (period == 0)
            {
                FloatSerie openSerie = this.GetSerie(StockDataType.OPEN);
                if (smoothingPeriod > 1)
                    openSerie = openSerie.CalculateEMA(smoothingPeriod);
                for (int i = 0; i < this.Count; i++)
                {
                    serie[i] = (closeSerie[i] - openSerie[i]) / openSerie[i];
                }
            }
            else
            {
                for (int i = 1; i < Math.Min(period, this.Count); i++)
                {
                    serie[i] = (closeSerie[i] - closeSerie[0]) / closeSerie[0];
                }
                for (int i = period; i < this.Count; i++)
                {
                    serie[i] = (closeSerie[i] - closeSerie[i - period]) / closeSerie[i - period];
                }
            }
            serie.Name = $"ROC_{period}";
            return serie;
        }

        /// <summary>
        ///  Get the price range over the period. Higuest in period - Lowest in period.
        /// </summary>
        /// <param name="period"></param>
        /// <param name="inputType"></param>
        /// <returns></returns>
        public float CalculateLastRange(int period, InputType inputType = InputType.HighLow, int smoothingPeriod = -1)
        {
            GetHighLowSeries(out FloatSerie lowSerie, out FloatSerie highSerie, inputType, smoothingPeriod);

            var lowestLow = lowSerie.GetMin(this.Count - period, this.Count);
            var highestHigh = highSerie.GetMax(this.Count - period, this.Count);

            return highestHigh - lowestLow;
        }

        public void CalculateEMATrailStop(int period, int inputSmoothing, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            float alpha = 2.0f / (float)(period + 1);

            longStopSerie = new FloatSerie(this.Count, "TRAILEMA.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILEMA.SS");

            FloatSerie lowEMASerie = this.GetSerie(StockDataType.LOW).CalculateEMA(inputSmoothing);
            FloatSerie highEMASerie = this.GetSerie(StockDataType.HIGH).CalculateEMA(inputSmoothing);
            FloatSerie closeEMASerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE < this.Values[1].CLOSE;
            int i = 1;
            float extremum;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
                extremum = previousValue.HIGH;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
                extremum = previousValue.LOW;
            }

            foreach (StockDailyValue currentValue in this.Values.Skip(1))
            {
                if (upTrend)
                {
                    if (closeEMASerie[i] < longStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = extremum;
                        extremum = lowEMASerie[i];
                    }
                    else
                    {
                        // Trail the stop
                        float longStop = longStopSerie[i - 1];
                        longStopSerie[i] = Math.Max(longStop, longStop + alpha * (lowEMASerie[i] - longStop));
                        //longStopSerie[i] = longStopSerie[i - 1] + alpha * (lowEMASerie[i] - longStopSerie[i - 1]);
                        shortStopSerie[i] = float.NaN;
                        extremum = Math.Max(extremum, highEMASerie[i]);
                    }
                }
                else
                {
                    if (closeEMASerie[i] > shortStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = extremum;
                        shortStopSerie[i] = float.NaN;
                        extremum = highEMASerie[i];
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = float.NaN;
                        float shortStop = shortStopSerie[i - 1];
                        shortStopSerie[i] = Math.Min(shortStop, shortStop + alpha * (highEMASerie[i] - shortStop));
                        extremum = Math.Min(extremum, lowEMASerie[i]);
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }
        public void CalculatePEMATrailStop(int period, int inputSmoothing, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILEMA.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILEMA.SS");

            FloatSerie lowEMASerie = this.GetSerie(StockDataType.LOW).CalculateEMA(inputSmoothing);
            FloatSerie highEMASerie = this.GetSerie(StockDataType.HIGH).CalculateEMA(inputSmoothing);
            FloatSerie closeEMASerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);
            FloatSerie EMASerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(period);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE < this.Values[1].CLOSE;
            int i = 1;
            float extremum;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
                extremum = previousValue.HIGH;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
                extremum = previousValue.LOW;
            }

            foreach (StockDailyValue currentValue in this.Values.Skip(1))
            {
                if (upTrend)
                {
                    if (closeEMASerie[i] < longStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = extremum;
                        extremum = lowEMASerie[i];
                    }
                    else
                    {
                        // Trail the stop
                        float longStop = longStopSerie[i - 1];
                        var step = EMASerie[i] - EMASerie[i - 1];
                        longStopSerie[i] = Math.Max(longStop, longStop + step);
                        //longStopSerie[i] = longStopSerie[i - 1] + alpha * (lowEMASerie[i] - longStopSerie[i - 1]);
                        shortStopSerie[i] = float.NaN;
                        extremum = Math.Max(extremum, highEMASerie[i]);
                    }
                }
                else
                {
                    if (closeEMASerie[i] > shortStopSerie[i - 1])
                    {
                        // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = extremum;
                        shortStopSerie[i] = float.NaN;
                        extremum = highEMASerie[i];
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = float.NaN;
                        float shortStop = shortStopSerie[i - 1];
                        var step = EMASerie[i] - EMASerie[i - 1];
                        shortStopSerie[i] = Math.Min(shortStop, shortStop + step);
                        extremum = Math.Min(extremum, lowEMASerie[i]);
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }

        public void CalculateHighLowSmoothedTrailStop(int period, int smoothing, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILHL.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILHL.SS");

            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW).CalculateEMA(smoothing);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH).CalculateEMA(smoothing);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE > this.Values[1].CLOSE;
            int i = 0;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
            }
            foreach (StockDailyValue currentValue in this.Values)
            {
                if (i > period)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(i - period, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(i - period, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(i - period, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(i - period, i));
                        }
                    }
                }
                else if (i > 0)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(0, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(0, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(0, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(0, i));
                        }
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }

        /// <summary>
        /// Calculate trail stop trailing using the minimum low in period for up trend and maximum high in period for down trend
        /// </summary>
        /// <param name="period"></param>
        /// <param name="longStopSerie"></param>
        /// <param name="shortStopSerie"></param>
        public void CalculateCountbackLineTrailStop(int period, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILCBL.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILCBL.SS");

            if (this.Values.Length < period) return;

            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            bool upTrend = closeSerie[1] > closeSerie[0];
            if (upTrend)
            {
                longStopSerie[0] = Math.Min(lowSerie[0], lowSerie[1]);
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = Math.Max(highSerie[0], highSerie[1]);
            }
            for (int i = 1; i < this.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < longStopSerie[i - 1])
                    {// Trailing stop has been broken => reverse trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = highSerie.GetCountBackHigh(i, period);
                    }
                    else
                    {// Trail the stop  
                        longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetCountBackLow(i, period));
                        shortStopSerie[i] = float.NaN;
                    }
                }
                else
                {
                    if (closeSerie[i] > shortStopSerie[i - 1])
                    {  // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = lowSerie.GetCountBackLow(i, period);
                        shortStopSerie[i] = float.NaN;
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = float.NaN;
                        if (lowSerie[i] < lowSerie[i - 1])
                        {
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetCountBackHigh(i, period));
                        }
                        else
                        {
                            shortStopSerie[i] = shortStopSerie[i - 1];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculate trail stop trailing using the minimum low in period for up trend and maximum high in period for down trend
        /// </summary>
        /// <param name="period"></param>
        /// <param name="longStopSerie"></param>
        /// <param name="shortStopSerie"></param>
        public void CalculateHighLowTrailStop(int period, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAILHL.LS");
            shortStopSerie = new FloatSerie(this.Count, "TRAILHL.SS");

            if (this.Values.Length < period) return;

            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE > this.Values[1].CLOSE;
            int i = 1;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
            }
            foreach (StockDailyValue currentValue in this.Values.Skip(1))
            {
                if (i > period)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(i - period - 1, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(i - period, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(i - period - 1, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(i - period, i));
                        }
                    }
                }
                else
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = highSerie.GetMax(0, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(0, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(0, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(0, i));
                        }
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }


        public void CalculateBandTrailStop(FloatSerie lowerBand, FloatSerie upperBand, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAIL.S");
            shortStopSerie = new FloatSerie(this.Count, "TRAIL.R");
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE < this.Values[1].CLOSE;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
            }
            int i = 0;
            foreach (StockDailyValue currentValue in this.Values)
            {
                if (i > 0)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = upperBand[i];
                        }
                        else
                        {
                            // UpTrend still in place
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowerBand[i]);
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowerBand[i];
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Down trend still in place
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], upperBand[i]);
                        }
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }

        /// <summary>
        /// Calculate a band trail based on upper band overshoting, without upper band to go up.
        /// </summary>
        /// <param name="lowerBand"></param>
        /// <param name="upperBand"></param>
        /// <param name="longStopSerie"></param>
        /// <param name="shortStopSerie"></param>
        public void CalculateBandTrailStop2(FloatSerie lowerBand, FloatSerie upperBand, out FloatSerie longStopSerie, out FloatSerie shortStopSerie)
        {
            longStopSerie = new FloatSerie(this.Count, "TRAIL.S");
            shortStopSerie = new FloatSerie(this.Count, "TRAIL.R");
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            StockDailyValue previousValue = this.Values.First();
            bool upTrend = previousValue.CLOSE < this.Values[1].CLOSE;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
            }
            int i = 0;
            foreach (StockDailyValue currentValue in this.Values)
            {
                if (i > 0)
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = upperBand[i];
                        }
                        else
                        {
                            // UpTrend still in place
                            longStopSerie[i] = lowerBand[i];
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = lowerBand[i];
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Down trend still in place
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = upperBand[i];
                        }
                    }
                }
                previousValue = currentValue;
                i++;
            }
        }
        public void CalculateTOPSAR(float accelerationFactorStep, float accelerationFactorInit, float accelerationFactorMax, out FloatSerie sarSerieSupport, out FloatSerie sarSerieResistance, int inputSmoothing)
        {
            float accelerationFactorUp = accelerationFactorInit;
            float accelerationFactorDown = accelerationFactorInit;
            bool isUpTrend = false;
            bool isDownTrend = false;
            sarSerieSupport = new FloatSerie(this.Values.Count(), "TOPSAR.S");
            sarSerieResistance = new FloatSerie(this.Values.Count(), "TOPSAR.R");
            float previousLow = float.NaN;
            float previousHigh = float.NaN;
            float previousSARUp = float.NaN;
            float previousSARDown = float.NaN;
            sarSerieResistance[0] = sarSerieResistance[1] = float.NaN;
            sarSerieSupport[0] = sarSerieSupport[1] = float.NaN;

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            for (int i = 2; i < this.Values.Count(); i++)
            {
                if (isUpTrend)
                {
                    float nextSAR = Math.Max(previousSARUp, previousSARUp + accelerationFactorUp * (lowSerie[i] - previousSARUp));
                    if (nextSAR >= closeSerie[i]) // UpTrendBroken
                    {
                        isUpTrend = false;
                        accelerationFactorUp = accelerationFactorInit;
                        sarSerieSupport[i] = float.NaN;
                    }
                    else // Up Trend continues
                    {
                        previousSARUp = nextSAR;
                        sarSerieSupport[i] = previousSARUp;
                        accelerationFactorUp = Math.Min(accelerationFactorMax, accelerationFactorUp + accelerationFactorStep);
                    }
                }
                else
                {
                    if (lowSerie.IsBottom(i - 1)) // Uptrend starts
                    {
                        isUpTrend = true;
                        accelerationFactorUp = accelerationFactorInit;
                        sarSerieSupport[i] = previousLow = previousSARUp = lowSerie[i - 1];
                    }
                    else
                    {
                        sarSerieSupport[i] = float.NaN;
                    }
                }
                if (isDownTrend)
                {
                    float nextSAR = Math.Min(previousSARDown, previousSARDown + accelerationFactorDown * (highSerie[i] - previousSARDown));
                    if (nextSAR <= closeSerie[i]) // DownTrendBroken
                    {
                        isDownTrend = false;
                        accelerationFactorDown = accelerationFactorInit;
                        sarSerieResistance[i] = float.NaN;
                    }
                    else // Down Trend continues
                    {
                        previousSARDown = nextSAR;
                        sarSerieResistance[i] = previousSARDown;
                        accelerationFactorDown = Math.Min(accelerationFactorMax, accelerationFactorDown + accelerationFactorStep);
                    }
                }
                else
                {
                    if (highSerie.IsTop(i - 1)) // Downtrend starts
                    {
                        isDownTrend = true;
                        accelerationFactorDown = accelerationFactorInit;
                        sarSerieResistance[i] = previousHigh = previousSARDown = highSerie[i - 1];
                    }
                    else
                    {
                        sarSerieResistance[i] = float.NaN;
                    }
                }
            }
        }
        public void CalculateSAR(float accelerationFactorStep, float accelerationFactorInit, float accelerationFactorMax, out FloatSerie sarSerieSupport, out FloatSerie sarSerieResistance, int inputSmoothing)
        {
            float accelerationFactor = accelerationFactorInit;
            bool isUpTrend = true;
            sarSerieSupport = new FloatSerie(this.Values.Count(), "SAR.S");
            sarSerieResistance = new FloatSerie(this.Values.Count(), "SAR.R");
            float previousExtremum = this.Values.First().LOW;
            float previousSAR = previousExtremum * 0.99f;
            sarSerieResistance[0] = float.NaN;
            sarSerieSupport[0] = previousSAR;

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            for (int i = 1; i < this.Values.Count(); i++)
            {
                if (isUpTrend)
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) >= closeSerie[i])
                    {
                        isUpTrend = false;
                        accelerationFactor = accelerationFactorInit;
                        previousSAR = previousExtremum;
                        sarSerieResistance[i] = previousSAR;
                        sarSerieSupport[i] = float.NaN;
                    }
                    else
                    {
                        if (highSerie[i] > previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = highSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarSerieResistance[i] = float.NaN;
                        sarSerieSupport[i] = previousSAR;
                    }
                }
                else
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) <= closeSerie[i])
                    {
                        isUpTrend = true;
                        accelerationFactor = accelerationFactorInit;
                        previousSAR = previousExtremum;
                        sarSerieSupport[i] = previousSAR;
                        sarSerieResistance[i] = float.NaN;
                    }
                    else
                    {
                        if (lowSerie[i] < previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = lowSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarSerieSupport[i] = float.NaN;
                        sarSerieResistance[i] = previousSAR;
                    }
                }
            }
        }
        public void CalculateSSAR(float accelerationFactor, out FloatSerie sarSerieSupport, out FloatSerie sarSerieResistance, int inputSmoothing)
        {
            bool isUpTrend = true;
            sarSerieSupport = new FloatSerie(this.Values.Count(), "SSAR.S");
            sarSerieResistance = new FloatSerie(this.Values.Count(), "SSAR.R");
            float previousExtremum = this.Values.First().LOW;
            float previousSAR = previousExtremum * 0.99f;
            sarSerieResistance[0] = float.NaN;
            sarSerieSupport[0] = previousSAR;

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            for (int i = 1; i < this.Values.Count(); i++)
            {
                if (isUpTrend)
                {
                    var nextSAR = previousSAR * (1f + accelerationFactor);
                    if (nextSAR >= closeSerie[i]) // Up trend Broken
                    {
                        isUpTrend = false;
                        previousSAR = previousExtremum;
                        sarSerieResistance[i] = previousSAR;
                        sarSerieSupport[i] = float.NaN;
                    }
                    else
                    {
                        if (highSerie[i] > previousExtremum)
                        {
                            previousExtremum = highSerie[i];
                        }
                        previousSAR = nextSAR;
                        sarSerieResistance[i] = float.NaN;
                        sarSerieSupport[i] = previousSAR;
                    }
                }
                else
                {
                    var nextSAR = previousSAR * (1f - accelerationFactor);
                    if (nextSAR <= closeSerie[i]) // Down  trend Broken
                    {
                        isUpTrend = true;
                        previousSAR = previousExtremum;
                        sarSerieSupport[i] = previousSAR;
                        sarSerieResistance[i] = float.NaN;
                    }
                    else
                    {
                        if (lowSerie[i] < previousExtremum)
                        {
                            previousExtremum = lowSerie[i];
                        }
                        previousSAR = nextSAR;
                        sarSerieSupport[i] = float.NaN;
                        sarSerieResistance[i] = previousSAR;
                    }
                }
            }
        }
        public void CalculateSARHL(int HLPeriod, float accelerationFactorStep, float accelerationFactorInit, float accelerationFactorMax, float margin, out FloatSerie sarSerieSupport, out FloatSerie sarSerieResistance)
        {
            IStockEvent trailHRSR = this.GetTrailStop("TRAILHL(" + HLPeriod + ")");
            BoolSerie upBreakSerie = trailHRSR.Events[2];
            BoolSerie downBreakSerie = trailHRSR.Events[3];

            float accelerationFactor = accelerationFactorInit;
            bool isUpTrend = true;
            sarSerieSupport = new FloatSerie(this.Values.Count(), "SAR.S");
            sarSerieResistance = new FloatSerie(this.Values.Count(), "SAR.R");
            float previousExtremum = this.Values.First().LOW;
            float previousSAR = previousExtremum * 0.99f;
            sarSerieResistance[0] = float.NaN;
            sarSerieSupport[0] = previousSAR;

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            for (int i = 1; i < this.Values.Count(); i++)
            {
                if (isUpTrend)
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) >= closeSerie[i])
                    {
                        isUpTrend = false;
                        accelerationFactor = accelerationFactorInit * (1f + ((previousExtremum - closeSerie[i]) / closeSerie[i] * 100f));
                        previousSAR = previousExtremum * (1.0f + margin);
                        sarSerieResistance[i] = previousSAR;
                        sarSerieSupport[i] = float.NaN;
                    }
                    else
                    {
                        // if (upBreakSerie[i]) accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor *2f);
                        if (highSerie[i] > previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = highSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarSerieResistance[i] = float.NaN;
                        sarSerieSupport[i] = previousSAR;
                    }
                }
                else
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) <= closeSerie[i])
                    {
                        isUpTrend = true;
                        accelerationFactor = accelerationFactorInit * (1f + ((closeSerie[i] - previousExtremum) / closeSerie[i] * 100f));
                        accelerationFactor = accelerationFactorInit;
                        previousSAR = previousExtremum * (1.0f - margin); ;
                        sarSerieSupport[i] = previousSAR;
                        sarSerieResistance[i] = float.NaN;
                    }
                    else
                    {
                        // if (downBreakSerie[i]) accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor * 2f);
                        if (lowSerie[i] < previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = lowSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarSerieSupport[i] = float.NaN;
                        sarSerieResistance[i] = previousSAR;
                    }
                }
            }
        }
        public void CalculateSAR2(float accelerationFactorStep, float accelerationFactorInit, float accelerationFactorMax, FloatSerie sarSerieSupport1, FloatSerie sarSerieResistance1, out FloatSerie sarSerieSupport2, out FloatSerie sarSerieResistance2)
        {
            float accelerationFactor = accelerationFactorInit;
            bool isUpTrend = true;
            sarSerieSupport2 = new FloatSerie(this.Values.Count(), "SAR.S");
            sarSerieResistance2 = new FloatSerie(this.Values.Count(), "SAR.R");
            float previousExtremum = this.Values.First().LOW;
            float previousSAR = previousExtremum * 0.99f;
            sarSerieResistance2[0] = float.NaN;
            sarSerieSupport2[0] = previousSAR;

            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);

            for (int i = 1; i < this.Values.Count(); i++)
            {
                if (isUpTrend)
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) >= closeSerie[i])
                    {
                        isUpTrend = false;
                        accelerationFactor = accelerationFactorInit;
                        if (float.IsNaN(sarSerieResistance1[i]))
                        {
                            previousSAR = previousExtremum;
                        }
                        else
                        {
                            previousSAR = sarSerieResistance1[i];
                        }
                        sarSerieResistance2[i] = previousSAR;
                        sarSerieSupport2[i] = float.NaN;
                    }
                    else
                    {
                        if (highSerie[i] > previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = highSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarSerieResistance2[i] = float.NaN;
                        sarSerieSupport2[i] = previousSAR;
                    }
                }
                else
                {
                    if (previousSAR + accelerationFactor * (previousExtremum - previousSAR) <= closeSerie[i])
                    {
                        isUpTrend = true;
                        accelerationFactor = accelerationFactorInit;

                        if (float.IsNaN(sarSerieSupport1[i]))
                        {
                            previousSAR = previousExtremum;
                        }
                        else
                        {
                            previousSAR = sarSerieSupport1[i];
                        }
                        sarSerieSupport2[i] = previousSAR;
                        sarSerieResistance2[i] = float.NaN;
                    }
                    else
                    {
                        if (lowSerie[i] < previousExtremum)
                        {
                            accelerationFactor = Math.Min(accelerationFactorMax, accelerationFactor + accelerationFactorStep);
                            previousExtremum = lowSerie[i];
                        }
                        previousSAR += accelerationFactor * (previousExtremum - previousSAR);
                        sarSerieSupport2[i] = float.NaN;
                        sarSerieResistance2[i] = previousSAR;
                    }
                }
            }
        }
        public FloatSerie CalculateCCI(int period)
        {
            FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);
            FloatSerie maSerie = this.GetIndicator("MA(" + period + ")").Series[0];

            FloatSerie diff = closeSerie - maSerie;

            FloatSerie meanDeviation = new FloatSerie(closeSerie.Count);
            FloatSerie cciSerie = new FloatSerie(closeSerie.Count, "CCI");

            float sum = 0;
            for (int i = 1; i < period && i < closeSerie.Count; i++)
            {
                sum += Math.Abs(diff[i]);
                cciSerie[i] = diff[i] / (0.015f * sum / i);
            }
            float K = 0.015f / period;
            for (int i = period; i < closeSerie.Count; i++)
            {
                sum += Math.Abs(diff[i]) - Math.Abs(diff[i - period]);
                cciSerie[i] = diff[i] / (K * sum);
            }

            return cciSerie;
        }
        /// <summary>
        /// Generates automated trends lines and fill the event array.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="leftStrength"></param>
        /// <param name="rightStrength"></param>
        /// <param name="nbPivots"></param>
        /// <param name="events"> 
        /// [0] = SupportDetected
        /// [1] = SupportBroken
        /// [2] = ResistanceDetected
        /// [3] = ResistanceBroken
        /// [4] = UpTrend => No resistance above
        /// [5] = DownTrend => no support below
        /// </param>
        /// 
        public enum TLEvent
        {
            SupportDetected = 0,
            SupportBroken,
            ResistanceDetected,
            ResistanceBroken,
            UpTrend,
            DownTrend
        }
        public void generateAutomaticHLTrendLines(int startIndex, int endIndex, int period, int nbPivots, ref BoolSerie[] events)
        {
            DrawingItem.CreatePersistent = false;
            try
            {
                IStockTrailStop pivots = this.GetTrailStop("TRAILHL(" + period + ")");
                BoolSerie brokenDown = pivots.Events[3];
                BoolSerie brokenUp = pivots.Events[2];

                Queue<int> highPivotIndexQueue = new Queue<int>(nbPivots);
                Queue<int> lowPivotIndexQueue = new Queue<int>(nbPivots);
                Queue<float> highPivotValueQueue = new Queue<float>(nbPivots);
                Queue<float> lowPivotValueQueue = new Queue<float>(nbPivots);

                FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
                FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
                FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

                float latestHighPivotValue;
                float latestLowPivotValue;

                Line2DBase latestResistanceLine = null;
                Line2DBase latestSupportLine = null;

                List<Line2DBase> supportList = new List<Line2DBase>();
                List<Line2DBase> resistanceList = new List<Line2DBase>();

                if (this.StockAnalysis.DrawingItems.ContainsKey(this.BarDuration))
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Clear();
                }
                else
                {
                    this.StockAnalysis.DrawingItems.Add(this.BarDuration, new StockDrawingItems());
                }

                int j, pivotIndex;
                int lastBreakIndex = 0;
                bool brokenResistance = false;
                bool brokenSupport = false;
                for (int i = startIndex + period; i <= endIndex; i++)
                {
                    #region Check for broken lines
                    if (latestResistanceLine != null)
                    {
                        if (closeSerie[i] > latestResistanceLine.ValueAtX(i))
                        {
                            // Down trend line has been broken
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestResistanceLine.Cut(i, true));
                            resistanceList.Remove(latestResistanceLine);
                            latestResistanceLine = null;
                            events[(int)TLEvent.ResistanceBroken][i] = true;
                            brokenResistance = true;
                            brokenSupport = false;
                        }
                        else
                        {
                            brokenResistance = false;
                        }
                    }
                    if (latestSupportLine != null)
                    {
                        if (closeSerie[i] < latestSupportLine.ValueAtX(i))
                        {
                            // Up trend line has been broken
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestSupportLine.Cut(i, true));
                            supportList.Remove(latestSupportLine);
                            latestSupportLine = null;
                            events[(int)TLEvent.SupportBroken][i] = true;
                            brokenSupport = true;
                            brokenResistance = false;
                        }
                        else
                        {
                            brokenSupport = false;
                        }
                    }
                    #endregion
                    if (brokenDown[i])
                    {
                        pivotIndex = highSerie.FindMaxIndex(lastBreakIndex, i - 1);
                        latestHighPivotValue = highSerie[pivotIndex];
                        lastBreakIndex = i;

                        if (highPivotIndexQueue.Count >= nbPivots)
                        {
                            highPivotIndexQueue.Dequeue();
                            highPivotValueQueue.Dequeue();
                        }
                        highPivotIndexQueue.Enqueue(pivotIndex);
                        highPivotValueQueue.Enqueue(latestHighPivotValue);

                        bool highestPivotFound = false;
                        for (j = highPivotValueQueue.Count - 2; j >= 0; j--)
                        {
                            if (latestHighPivotValue < highPivotValueQueue.ElementAt(j))
                            {
                                highestPivotFound = true;
                                break;
                            }
                        }
                        if (highestPivotFound)
                        {
                            if (latestResistanceLine != null)
                            {
                                // New line has to be drawn
                                //this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestResistanceLine.Cut(i, true));
                                //resistanceList.Remove(latestResistanceLine);
                            }

                            latestResistanceLine =
                                new HalfLine2D(
                                    new PointF(highPivotIndexQueue.ElementAt(j), highPivotValueQueue.ElementAt(j)),
                                    new PointF(pivotIndex, latestHighPivotValue),
                                    Pens.Green);
                            resistanceList.Add(latestResistanceLine);

                            events[(int)TLEvent.ResistanceDetected][i] = true;
                            brokenResistance = false;
                        }
                    }
                    else if (brokenUp[i])
                    {
                        pivotIndex = lowSerie.FindMinIndex(lastBreakIndex, i - 1);
                        latestLowPivotValue = lowSerie[pivotIndex];
                        lastBreakIndex = i;

                        if (lowPivotIndexQueue.Count >= nbPivots)
                        {
                            lowPivotIndexQueue.Dequeue();
                            lowPivotValueQueue.Dequeue();
                        }
                        lowPivotIndexQueue.Enqueue(pivotIndex);
                        lowPivotValueQueue.Enqueue(lowSerie[pivotIndex]);

                        bool lowestPivotFound = false;
                        for (j = lowPivotValueQueue.Count - 2; j >= 0; j--)
                        {
                            if (latestLowPivotValue > lowPivotValueQueue.ElementAt(j))
                            {
                                lowestPivotFound = true;
                                break;
                            }
                        }
                        if (lowestPivotFound)
                        {
                            if (latestSupportLine != null)
                            {
                                // New line has to be drawn
                                //this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestSupportLine.Cut(pivotIndex, true));
                                //supportList.Remove(latestSupportLine);
                            }

                            latestSupportLine =
                                new HalfLine2D(
                                    new PointF(lowPivotIndexQueue.ElementAt(j), lowPivotValueQueue.ElementAt(j)),
                                    new PointF(pivotIndex, latestLowPivotValue),
                                    Pens.Red);
                            supportList.Add(latestSupportLine);

                            events[(int)TLEvent.SupportDetected][i] = true;

                            brokenSupport = false;
                        }
                    }

                    // Detecting upTrend events
                    bool upTrend = resistanceList.Count == 0 && supportList.Count != 0;
                    foreach (Line2DBase line in resistanceList)
                    {
                        if (closeSerie[i] > line.ValueAtX(i))
                        {
                            upTrend = false;
                            break;
                        }
                    }

                    // Detecting downTrend events
                    bool downTrend = supportList.Count == 0 && resistanceList.Count != 0;
                    foreach (Line2DBase line in supportList)
                    {
                        if (closeSerie[i] < line.ValueAtX(i))
                        {
                            downTrend = false;
                            break;
                        }
                    }
                    if (!(downTrend || upTrend))
                    {
                        events[(int)TLEvent.UpTrend][i] = brokenResistance;
                        events[(int)TLEvent.DownTrend][i] = brokenSupport;
                    }
                    else
                    {
                        events[(int)TLEvent.UpTrend][i] = upTrend;
                        events[(int)TLEvent.DownTrend][i] = downTrend;
                    }
                }
                if (latestSupportLine != null)
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestSupportLine);
                }
                if (latestResistanceLine != null)
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Add(latestResistanceLine);
                }
            }
            catch (Exception e)
            {
                StockLog.Write(e);
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }

        public enum DowEvent
        {
            UpTrend,
            DownTrend,
            Ranging,
        }
        public void generateDowTrendLines(int startIndex, int endIndex, int hlPeriod, ref BoolSerie[] events)
        {
            DrawingItem.CreatePersistent = false;
            try
            {
                IStockIndicator hlTrailSR = this.GetIndicator("TRAILHLSR(" + hlPeriod + ")");

                BoolSerie upTrendSerie = events[0];
                BoolSerie downTrendSerie = events[1];
                BoolSerie rangingSerie = events[2];

                BoolSerie supportDetected = hlTrailSR.Events[0];
                BoolSerie resistanceDetected = hlTrailSR.Events[1];

                FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
                FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
                FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

                FloatSerie supportSerie = hlTrailSR.Series[0];
                FloatSerie resistanceSerie = hlTrailSR.Series[1];

                Segment2D latestResistanceLine = new Segment2D(startIndex - 1, highSerie[startIndex], startIndex, highSerie[startIndex]);
                Segment2D latestSupportLine = new Segment2D(startIndex - 1, lowSerie[startIndex], startIndex, lowSerie[startIndex]);

                Segment2D newLine;
                Bullet2D bullet;

                DowEvent trendStatus = DowEvent.Ranging;

                upTrendSerie[0] = trendStatus == DowEvent.UpTrend;
                downTrendSerie[0] = trendStatus == DowEvent.DownTrend;
                rangingSerie[0] = trendStatus == DowEvent.Ranging;

                // Do drawing Items cleanup
                if (this.StockAnalysis.DrawingItems.ContainsKey(this.BarDuration))
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Clear();
                }
                else
                {
                    this.StockAnalysis.DrawingItems.Add(this.BarDuration, new StockDrawingItems());
                }

                // Start Creating lines
                int j;
                for (int i = startIndex + 1; i <= endIndex; i++)
                {
                    if (supportDetected[i])
                    {
                        // Find previous Low value
                        for (j = i; j > latestSupportLine.Point2.X && lowSerie[j] != supportSerie[i]; j--) ;
                        this.StockAnalysis.DrawingItems[this.BarDuration].Add(newLine = new Segment2D(latestSupportLine.Point2.X, latestSupportLine.Point2.Y, j, lowSerie[j]));
                        this.StockAnalysis.DrawingItems[this.BarDuration].Add(bullet = new Bullet2D(newLine.Point2, 3));

                        latestSupportLine = newLine;

                        // Set trend Status
                        if (latestSupportLine.Point2.Y >= latestSupportLine.Point1.Y) // Support Higher Low
                        {
                            trendStatus = latestResistanceLine.Point2.Y >= latestResistanceLine.Point1.Y
                                ? DowEvent.UpTrend
                                : DowEvent.Ranging;
                        }
                        else // Support Lower Low
                        {
                            trendStatus = latestResistanceLine.Point2.Y >= latestResistanceLine.Point1.Y
                                ? ((closeSerie[i] > latestSupportLine.Point1.Y) ? DowEvent.Ranging : trendStatus) // Don't change trend in case of PB to END of trend.
                                : DowEvent.DownTrend;
                        }

                        // Set line color
                        if (newLine.Point2.Y >= newLine.Point1.Y)
                        {
                            newLine.Pen = Pens.Green;
                            bullet.Pen = Pens.Green;
                        }
                        else
                        {
                            newLine.Pen = Pens.Red;
                            bullet.Pen = Pens.Red;
                        }
                    }
                    else
                    {
                        if (resistanceDetected[i])
                        {
                            // Find previous Low value
                            for (j = i; j > latestResistanceLine.Point2.X && highSerie[j] != resistanceSerie[i]; j--) ;
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(newLine = new Segment2D(latestResistanceLine.Point2.X, latestResistanceLine.Point2.Y, j, highSerie[j]));
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(bullet = new Bullet2D(newLine.Point2, 3));

                            latestResistanceLine = newLine;
                            // Set trend Status
                            if (latestSupportLine.Point2.Y >= latestSupportLine.Point1.Y) // Support Higher Low
                            {
                                trendStatus = latestResistanceLine.Point2.Y >= latestResistanceLine.Point1.Y
                                    ? DowEvent.UpTrend
                                    : DowEvent.Ranging;
                            }
                            else // Support Lower Low
                            {
                                trendStatus = latestResistanceLine.Point2.Y >= latestResistanceLine.Point1.Y
                                    ? ((closeSerie[i] < latestResistanceLine.Point1.Y) ? DowEvent.Ranging : trendStatus) // Don't change trend in case of PB to END of trend.
                                    : DowEvent.DownTrend;
                            }

                            // Set line color
                            if (newLine.Point2.Y >= newLine.Point1.Y)
                            {
                                newLine.Pen = Pens.Green;
                                bullet.Pen = Pens.Green;
                            }
                            else
                            {
                                newLine.Pen = Pens.Red;
                                bullet.Pen = Pens.Red;
                            }
                        }
                        else
                        {
                            if (closeSerie[i] < latestSupportLine.Point2.Y) // Broken latest support
                            {
                                trendStatus = DowEvent.DownTrend;
                            }
                            else if (closeSerie[i] > latestResistanceLine.Point2.Y) // Broken latest resistance
                            {
                                trendStatus = DowEvent.UpTrend;
                            }
                        }
                    }
                    upTrendSerie[i] = trendStatus == DowEvent.UpTrend;
                    downTrendSerie[i] = trendStatus == DowEvent.DownTrend;
                    rangingSerie[i] = trendStatus == DowEvent.Ranging;
                }
            }
            catch (Exception e)
            {
                StockLog.Write(e);
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }

        #endregion

        public FloatSerie GenerateSecondarySerieFromOtherSerie(string instrumentId, BarDuration duration)
        {
            var otherSerie = StockDictionary.Instruments[instrumentId].GetDataSerie(duration);
            if (otherSerie == null)
            {
                return null;
            }

            FloatSerie newSerie = new FloatSerie(this.Count);
            newSerie.Name = otherSerie.StockName;

            FloatSerie otherCloseSerie = otherSerie.GetSerie(StockDataType.CLOSE);
            float previousValue = otherCloseSerie[0];
            DateTime startDate = otherSerie.Values.First().DATE;
            DateTime lastDate = otherSerie.LastValue.DATE;

            int i = 0;
            foreach (StockDailyValue dailyValue in this.Values)
            {
                if (dailyValue.DATE > lastDate || dailyValue.DATE < startDate)
                {
                    newSerie[i] = otherCloseSerie[0];
                }
                else
                {
                    if (otherSerie.Values.Any(v => v.DATE == dailyValue.DATE))
                    {
                        newSerie[i] = otherSerie[dailyValue.DATE].GetStockData(StockDataType.CLOSE);
                        previousValue = newSerie[i];
                    }
                    else
                    {
                        newSerie[i] = previousValue;
                    }
                }
                i++;
            }

            return newSerie;
        }


    }
}
