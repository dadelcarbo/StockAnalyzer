using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockDataProviders.AbcDataProvider;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockData;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;
using System.Xml.Serialization;

namespace StockAnalyzerApp.StockData
{
    [DebuggerDisplay("{Id}")]
    public class StockInstrument
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Isin { get; set; }
        public string Symbol { get; set; }
        public string Ticker { get; set; }
        public Groups Group { get; set; }
        public StockDataProvider DataProvider { get; set; }
        public long SaxoId { get; set; }
        public StockAnalysis StockAnalysis { get; set; }

        public StockSerie StockSerie { get; set; }

        public StockInstrument(StockSerie serie)
        {
            this.StockSerie = serie;

            this.Id = serie.StockName;
            this.DisplayName = serie.StockName;
            this.Isin = serie.ISIN;
            this.Ticker = serie.Symbol;
            this.DataProvider = serie.DataProvider;
            this.Symbol = serie.Symbol;
            this.SaxoId = serie.SaxoId;

            this.Group = serie.StockGroup;
            this.StockAnalysis = serie.StockAnalysis;
        }

        private SortedDictionary<BarDuration, DataSerie> cache = new SortedDictionary<BarDuration, DataSerie>();

        public void ClearCache()
        {
            cache.Clear();
        }

        /// <summary>
        /// Return data from cache dictionnary
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public DataSerie GetDataSerie(BarDuration duration)
        {
            if (!cache.ContainsKey(duration))
            {
                var dp = StockDataProviderBase.GetDataProvider(this.DataProvider);

                if (dp == null)
                    throw new InvalidOperationException($"Data Provider {this.DataProvider} not found, cannot get data serie !");

                if (!dp.SupportsDuration(duration))
                    return null;

                var dataSerie = dp.GetData(this, duration);

                if (dataSerie != null)
                {
                    cache.Add(duration, new DataSerie(this, duration, StockSerie.ValueArray));
                }
                else
                    return null;
            }
            return cache[duration];
        }

        public DataSerie GetDefaultDataSerie()
        {
            var dp = StockDataProviderBase.GetDataProvider(this.DataProvider);

            if (dp == null)
                throw new InvalidOperationException($"Data Provider {this.DataProvider} not found, cannot get data serie !");

            return GetDataSerie(dp.DefaultDuration);
        }

        public BarDuration[] SupportedDurations => StockDataProviderBase.GetDataProvider(this.DataProvider)?.SupportedDurations;

        #region Group Management

        public bool BelongsToGroup(Groups group)
        {
            if (StockAnalysis != null && StockAnalysis.Excluded) return false;
            return BelongsToGroupFull(group);
        }
        public bool BelongsToGroupFull(Groups group)
        {
            if (group == Groups.NONE)
                return false;
            if (this.Group == group || group == Groups.ALL)
                return true;

            switch (group)
            {
                case Groups.SAXO:
                    return this.SaxoId > 0;
            }

            if (DataProvider == StockDataProvider.ABC)
                return ABCDataProvider.BelongsToGroup(this.StockSerie, group);

            return false;
        }
        public bool BelongsToGroup(string groupName)
        {
            return this.BelongsToGroup((Groups)Enum.Parse(typeof(Groups), groupName));
        }
        #endregion

        #region Analysis Serialisation Members
        public void ReadAnalysisFromXml(System.Xml.XmlReader reader)
        {
            try
            {
                // Deserialize StockAnalysis
                reader.ReadStartElement(); // Start StockAnalysisItem
                XmlSerializer serializer = new XmlSerializer(typeof(StockAnalysis));

                this.StockAnalysis = (StockAnalysis)serializer.Deserialize(reader);

                reader.ReadEndElement(); // End StockAnalysisItem
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error parsing analysis file");
            }
        }
        public void WriteAnalysisToXml(System.Xml.XmlWriter writer)
        {
            if (!this.StockAnalysis.IsEmpty())
            {
                // Serialize StockAnalysis
                XmlSerializer serializer = new XmlSerializer(typeof(StockAnalysis));
                serializer.Serialize(writer, this.StockAnalysis);
            }
        }
        #endregion

        #region Alert Detection

        private bool MatchEvent(StockAlertDef stockAlert)
        {
            try
            {
                int eventIndex;
                IStockEvent stockEvent = null;
                IStockViewableSeries indicator;

                switch (stockAlert.Type)
                {
                    case AlertType.Group:
                    case AlertType.Stock:
                        {
                            //if (!string.IsNullOrEmpty(stockAlert.Script))
                            //{
                            //    var screener = StockScriptManager.Instance.CreateStockFilterInstance(stockAlert.Script);
                            //    if (screener != null)
                            //    {
                            //        if (!screener.MatchFilter(this, stockAlert.BarDuration))
                            //            return false;
                            //    }
                            //}
                            if (!string.IsNullOrEmpty(stockAlert.FilterFullName))
                            {
                                var dataSerie = this.GetDataSerie(stockAlert.FilterDuration);
                                if (dataSerie == null)
                                    return false;

                                indicator = StockViewableItemsManager.GetViewableItem(stockAlert.FilterFullName);
                                if (dataSerie.HasVolume || !indicator.RequiresVolumeData)
                                {
                                    stockEvent = (IStockEvent)StockViewableItemsManager.CreateInitialisedFrom(indicator, dataSerie);
                                }
                                else
                                {
                                    return false;
                                }
                                eventIndex = Array.IndexOf(stockEvent.EventNames, stockAlert.FilterEventName);
                                if (eventIndex == -1)
                                {
                                    StockLog.Write("Event " + stockAlert.EventName + " not found in " + indicator.Name);
                                    return false;
                                }
                                else
                                {
                                    if (!stockEvent.Events[eventIndex][dataSerie.LastIndex])
                                        return false;
                                }
                            }
                            if (!string.IsNullOrEmpty(stockAlert.IndicatorName))
                            {
                                var dataSerie = this.GetDataSerie(stockAlert.BarDuration);
                                if (dataSerie == null)
                                    return false;

                                indicator = StockViewableItemsManager.GetViewableItem(stockAlert.IndicatorFullName);
                                if (dataSerie.HasVolume || !indicator.RequiresVolumeData)
                                {
                                    stockEvent = (IStockEvent)StockViewableItemsManager.CreateInitialisedFrom(indicator, dataSerie);
                                }
                                else
                                {
                                    return false;
                                }
                                eventIndex = Array.IndexOf(stockEvent.EventNames, stockAlert.EventName);
                                if (eventIndex == -1)
                                {
                                    StockLog.Write("Event " + stockAlert.EventName + " not found in " + indicator.Name);
                                    return false;
                                }
                                else
                                {
                                    return stockEvent.Events[eventIndex][dataSerie.LastIndex];
                                }
                            }
                            return true;
                        }
                    case AlertType.Price:
                        if (stockAlert.PriceTrigger != 0)
                        {
                            var dataSerie = this.GetDataSerie(stockAlert.BarDuration);
                            var closeSerie = dataSerie.GetSerie(StockDataType.CLOSE);
                            var index = dataSerie.LastIndex;
                            if (index == 0)
                                return false;

                            if (stockAlert.TriggerBrokenUp)
                            {
                                return closeSerie[index - 1] < stockAlert.PriceTrigger && closeSerie[index] > stockAlert.PriceTrigger;
                            }
                            else
                            {
                                return closeSerie[index - 1] > stockAlert.PriceTrigger && closeSerie[index] < stockAlert.PriceTrigger;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }

            return false;
        }

        public StockAlert MatchAlertDef(StockAlertDef alertDef)
        {
            if (this.MatchEvent(alertDef))
            {
                return new StockAlert()
                {
                    Date = this.GetDataSerie(alertDef.BarDuration).LastValue.DATE,
                    AlertDef = alertDef,
                    Instrument = this
                };
            }
            return null;
        }




        #endregion
    }
}
