using System;
using System.Xml.Serialization;

namespace StockAnalyzer.StockClasses
{
    public class StockAlert : IEquatable<StockAlert>
    {
        public DateTime Date { get; set; }
        public string StockName { get; set; }
        public string StockGroup { get; set; }
        public string Alert { get; set; }
        [XmlIgnore]
        public string Event
        {
            get
            {
                if (Alert == null || !Alert.Contains("=>"))
                    return null;
                var index = Alert.IndexOf("=>");
                return Alert.Remove(0, index + 2);
            }
        }
        [XmlIgnore]
        public string Indicator
        {
            get
            {
                if (Alert == null || !Alert.Contains("=>"))
                    return null;
                var index = Alert.IndexOf("=>");
                return Alert.Substring(0, index);
            }
        }
        public StockBarDuration BarDuration { get; set; }

        public float AlertClose { get; set; }

        public long ExchangedMoney { get; set; }

        public string Duration { get { return BarDuration.Duration.ToString(); } }

        public StockAlert()
        {
        }

        public StockAlert(StockAlertDef alertDef, DateTime date, string stockName, string stockGroup, float alertClose, long volume)
        {
            this.Alert = alertDef.EventFullName;
            this.BarDuration = alertDef.BarDuration;
            Date = date;
            StockName = stockName;
            StockGroup = stockGroup;
            AlertClose = alertClose;
            ExchangedMoney = (int)Math.Round(alertClose * (float)volume / 1000.0f);
        }

        public override string ToString()
        {
            return StockName + "\t" + Date + "\t" + BarDuration + "\t" + Alert;
        }

        public static bool operator ==(StockAlert a, StockAlert b)
        {
            if (object.ReferenceEquals(a, null))
                return object.ReferenceEquals(b, null);
            return a.Equals(b);
        }
        public static bool operator !=(StockAlert a, StockAlert b)
        {
            if (object.ReferenceEquals(a, null))
                return !object.ReferenceEquals(b, null);
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as StockAlert);
        }

        public bool Equals(StockAlert other)
        {
            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (Object.ReferenceEquals(other, null))
            {
                return false;
            }
            return this.StockName == other.StockName &&
                   this.StockGroup == other.StockGroup &&
                   this.Date == other.Date &&
                   this.Alert == other.Alert &&
                   this.BarDuration == other.BarDuration &&
                   this.AlertClose == other.AlertClose;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
