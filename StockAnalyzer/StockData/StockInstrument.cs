using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockData.DataProviders;
using StockAnalyzer.StockData.DataProviders.AbcBourse;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Xml.Serialization;

namespace StockAnalyzer.StockData
{
    [DebuggerDisplay("{Id}-{DisplayName}")]
    public class StockInstrument
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DisplayName => Name;
        public string Isin { get; set; }
        public string Symbol { get; set; }
        public char AbcSuffix { get; set; }
        public long Ticker { get; set; }
        public Groups Group { get; set; }
        public DataProvider Provider { get; set; }

        public Market Market { get; set; }

        public long SaxoId { get; set; }
        public StockAnalysis StockAnalysis { get; set; }

        public StockSerie StockSerie { get; set; }

        public StockInstrument()
        {
            this.StockAnalysis = new StockAnalysis();
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
                var dp = DataProviderBase.GetDataProvider(this.Provider);

                if (dp == null)
                    throw new InvalidOperationException($"Data Provider {this.Provider} not found, cannot get data serie !");

                if (!dp.SupportsDuration(duration))
                    return null;

                var dataSerie = dp.LoadData(this, duration);
                if (dataSerie != null)
                {
                    cache.Add(duration, dataSerie);
                }
                else
                    return null;
            }
            return cache[duration];
        }
        public void SetDataSerie(BarDuration duration, DataSerie dataSerie)
        {
            this.ClearCache();
            cache.Add(duration, dataSerie);
        }

        public DataSerie GetDefaultDataSerie()
        {
            var dp = DataProviderBase.GetDataProvider(this.Provider);

            if (dp == null)
                throw new InvalidOperationException($"Data Provider {this.Provider} not found, cannot get data serie !");

            return GetDataSerie(dp.DefaultDuration);
        }

        public BarDuration[] SupportedDurations => DataProviderBase.GetDataProvider(this.Provider)?.SupportedDurations;

        #region Group Management

        public bool BelongsToGroup(Groups group)
        {
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

            if (this.Provider == DataProvider.ABC)
                return (DataProviderBase.GetDataProvider(this.Provider) as AbcDataProvider).BelongsToGroup(this, group);

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

        public string ToDef()
        {
            return $"{Id},{Name},{Isin},{Symbol},{Ticker},{Group},{Market}";
        }
    }
}
