using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockDrawing;

namespace StockAnalyzer.StockClasses
{
    public class StockAlertLog : INotifyPropertyChanged
    {
        private DateTime lastRefreshDate;

        public DateTime LastRefreshDate
        {
            get { return lastRefreshDate; }
            set
            {
                if (value != lastRefreshDate)
                {
                    lastRefreshDate = value;
                    NotifyPropertyChanged("LastRefreshDate");
                }
            }
        }

        private ObservableCollection<StockAlert> alerts;

        public ObservableCollection<StockAlert> Alerts
        {
            get { return alerts; }
            set
            {
                if (value != alerts)
                {
                    alerts = value;
                    NotifyPropertyChanged("Alerts");
                    NotifyPropertyChanged("StockNames");
                }
            }
        }

        private string selectedStock;
        public string SelectedStock { get { return selectedStock; } set { if (value != selectedStock) { selectedStock = value; NotifyPropertyChanged("SelectedStock"); } } }

        public IEnumerable<string> StockNames { get { return this.Alerts.Select(a => a.StockName).Distinct(); } }

        private int progressValue;
        public int ProgressValue { get { return progressValue; } set { if (value != progressValue) { progressValue = value; NotifyPropertyChanged("ProgressValue"); } } }

        private int progressMax;
        public int ProgressMax { get { return progressMax; } set { if (value != progressMax) { progressMax = value; NotifyPropertyChanged("ProgressMax"); } } }

        private bool progressVisibility;
        public bool ProgressVisibility { get { return progressVisibility; } set { if (value != progressVisibility) { progressVisibility = value; NotifyPropertyChanged("ProgressVisibility"); } } }

        private string progressName;
        public string ProgressName { get { return progressName; } set { if (value != progressName) { progressName = value; NotifyPropertyChanged("ProgressName"); } } }

        private readonly string fileName;
        public StockAlertLog(string fileName)
        {
            this.lastRefreshDate = DateTime.MinValue;
            this.alerts = new ObservableCollection<StockAlert>();
            this.progressVisibility = false;
            this.fileName = fileName;

            this.Load();
        }

        private void Load()
        {
            string filepath = Path.GetTempPath() + this.fileName;
            if (File.Exists(filepath))
            {
                LastRefreshDate = File.GetLastWriteTime(filepath);
            }
            else
            {
                LastRefreshDate = DateTime.MinValue;
                return;
            }
            using (var fs = new FileStream(filepath, FileMode.OpenOrCreate))
            {
                System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                XmlSerializer serializer = new XmlSerializer(typeof(List<StockAlert>));
                this.Alerts = new ObservableCollection<StockAlert>((serializer.Deserialize(xmlReader) as List<StockAlert>).OrderByDescending(a => a.Date));
            }
        }

        public void Save()
        {
            string filepath = Path.GetTempPath() + this.fileName;
            this.LastRefreshDate = DateTime.Now;
            using (FileStream fs = new FileStream(filepath, FileMode.Create))
            {
                System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings
                {
                    Indent = true,
                    NewLineOnAttributes = true
                };
                System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(fs, settings);
                XmlSerializer serializer = new XmlSerializer(typeof (List<StockAlert>));
                DateTime limitDate = DateTime.Today.AddDays(-7);
                serializer.Serialize(xmlWriter, alerts.Where(a => a.Date >= limitDate).OrderByDescending(a => a.Date).ToList());
            }
        }

        public void Clear()
        {
            string filePath = Path.GetTempPath() + this.fileName;
            if (File.Exists(filePath))
                File.Delete(filePath);
            this.Alerts?.Clear();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public bool IsUpToDate(DateTime date)
        {
            return LastRefreshDate >= date;
        }
    }
}
