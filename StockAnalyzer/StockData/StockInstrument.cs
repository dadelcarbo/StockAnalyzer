using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockDataProviders.AbcDataProvider;
using StockAnalyzer.StockData;
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

        public StockAlert MatchAlert(StockAlertDef alertDef)
        {
            throw new NotImplementedException("MatchAlert method is not implemented in StockInstrument");
        }
        #endregion

    }
}
