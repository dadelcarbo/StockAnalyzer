using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace StockAnalyzer.StockClasses
{
    public class StockAlertLog : INotifyPropertyChanged
    {
        public static string AlertLogFolder => Folders.AlertLog;

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

        public IEnumerable<string> StockNames => this.Alerts.Select(a => a.StockName).Distinct();

        private int progressValue;
        public int ProgressValue { get { return progressValue; } set { if (value != progressValue) { progressValue = value; NotifyPropertyChanged("ProgressValue"); } } }

        private int progressMax;
        public int ProgressMax { get { return progressMax; } set { if (value != progressMax) { progressMax = value; NotifyPropertyChanged("ProgressMax"); } } }

        private bool progressVisibility;
        public bool ProgressVisibility { get { return progressVisibility; } set { if (value != progressVisibility) { progressVisibility = value; NotifyPropertyChanged("ProgressVisibility"); } } }

        private string progressName;
        public string ProgressName { get { return progressName; } set { if (value != progressName) { progressName = value; NotifyPropertyChanged("ProgressName"); } } }

        private string progressTitle;
        public string ProgressTitle { get { return progressTitle; } set { if (value != progressTitle) { progressTitle = value; NotifyPropertyChanged("ProgressTitle"); } } }

        private readonly string fileName;
        private StockAlertLog(string fileName, DateTime startDate)
        {
            this.lastRefreshDate = DateTime.MinValue;
            this.alerts = new ObservableCollection<StockAlert>();
            this.progressVisibility = false;
            this.fileName = fileName;
            this.StartDate = startDate;

            this.Load();

            alertLogs.Add(fileName, this);
        }

        private static readonly SortedDictionary<string, StockAlertLog> alertLogs = new SortedDictionary<string, StockAlertLog>();

        public DateTime StartDate { get; set; }
        public static StockAlertLog Load(string fileName, DateTime startDate)
        {
            if (alertLogs.ContainsKey(fileName))
            {
                return alertLogs[fileName];
            }
            else
            {
                return new StockAlertLog(fileName, startDate);
            }
        }

        private void Load()
        {
            string filepath = Path.Combine(AlertLogFolder, this.fileName);
            if (File.Exists(filepath))
            {
                LastRefreshDate = File.GetLastWriteTime(filepath);
            }
            else
            {
                LastRefreshDate = DateTime.MinValue;
                return;
            }

            try
            {
                using var fs = new FileStream(filepath, FileMode.OpenOrCreate);
                System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings
                {
                    IgnoreWhitespace = true
                };
                System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<StockAlert>));
                this.Alerts = new ObservableCollection<StockAlert>((serializer.Deserialize(xmlReader) as ObservableCollection<StockAlert>).OrderByDescending(a => a.Date).ThenBy(a => a.StockName));

                foreach (var alert in this.Alerts)
                {
                    alert.SetAlertDef();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error loading alert file", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                this.Alerts.Clear();
                File.Delete(filepath);
            }
        }

        public void Save()
        {
            // Root folder sanity check
            if (!Directory.Exists(AlertLogFolder))
                Directory.CreateDirectory(AlertLogFolder);
            string filepath = Path.Combine(AlertLogFolder, this.fileName);
            this.LastRefreshDate = DateTime.Now;
            using FileStream fs = new FileStream(filepath, FileMode.Create);
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings
            {
                Indent = true,
                NewLineOnAttributes = true
            };
            System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(fs, settings);
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<StockAlert>));
            this.Alerts = new ObservableCollection<StockAlert>(alerts.Where(a => a.Date >= StartDate).OrderByDescending(a => a.Date).ThenBy(a => a.AlertDefId).ThenBy(a => a.StockName));
            serializer.Serialize(xmlWriter, this.alerts);
        }

        public void Clear()
        {
            string filePath = Path.Combine(AlertLogFolder, this.fileName);
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

        public string FileName => fileName;
    }
}
