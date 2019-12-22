using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.Portofolio;
using StockAnalyzerSettings.Properties;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class PortfolioDataProvider : StockDataProviderBase
    {
        public PortfolioDataProvider()
        {

        }
        public override bool SupportsIntradayDownload
        {
            get { return false; }
        }

        public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
        {
            if (PortfolioDataProvider.StockPortofolioList != null && PortfolioDataProvider.StockPortofolioList.Count > 0)
                return;

            if (string.IsNullOrEmpty(Settings.Default.PortofolioFile))
            {
                Settings.Default.PortofolioFile = "Portfolio.xml";
            }
            // Read Stock Values from XML
            string orderFileName = Path.Combine(Settings.Default.RootFolder, Settings.Default.PortofolioFile);
            try
            {
                // Parsing portofolios
                if (System.IO.File.Exists(orderFileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(StockPortofolioList));
                    using (FileStream fs = new FileStream(orderFileName, FileMode.Open))
                    {
                        System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                        settings.IgnoreWhitespace = true;
                        System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);

                        PortfolioDataProvider.StockPortofolioList = (StockPortofolioList)serializer.Deserialize(xmlReader);
                    }
                }
                else
                {
                    PortfolioDataProvider.StockPortofolioList = new StockPortofolioList();
                    PortfolioDataProvider.StockPortofolioList.Add(new StockPortofolio("BinckPEA_P", 10000, false));
                    PortfolioDataProvider.StockPortofolioList.Add(new StockPortofolio("BinckTitre_P", 10000, false));
                }

                // Generate Portfolio Series
                foreach (var portfolio in PortfolioDataProvider.StockPortofolioList)
                {
                    StockSerie portfolioSerie = new StockSerie(portfolio.Name, portfolio.Name, StockSerie.Groups.Portfolio, StockDataProvider.Portofolio);
                    stockDictionary.Add(portfolio.Name, portfolioSerie);
                }
            }
            catch (System.Exception exception)
            {
                string message = exception.Message;
                if (exception.InnerException != null)
                {
                    message += "\n\r" + exception.InnerException.Message;
                }
                MessageBox.Show(message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
        {
            return true;
        }

        public override bool LoadData(string rootFolder, StockSerie stockSerie)
        {
            StockPortofolio portfolio = StockPortofolioList.Instance.FirstOrDefault(p => p.Name == stockSerie.StockName);
            if (portfolio == null) return false;

            portfolio.ToSerie(stockSerie);

            return true;
        }

        public static StockPortofolioList StockPortofolioList { get; set; }
    }
}
