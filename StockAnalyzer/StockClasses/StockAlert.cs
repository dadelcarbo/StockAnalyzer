using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockDrawing;

namespace StockAnalyzer.StockClasses
{
    public class StockAlert : IEquatable<StockAlert>
    {
        public DateTime Date { get; set; }
        public string StockName { get; set; }
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

        public string Duration { get { return BarDuration.Duration.ToString(); } }


        public StockAlert()
        {
        }

        public StockAlert(StockAlertDef alertDef, DateTime date, string stockName, float alertClose)
        {
            this.Alert = alertDef.EventFullName;
            this.BarDuration = alertDef.BarDuration;
            Date = date;
            StockName = stockName;
            AlertClose = alertClose;
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
            if (System.Object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (System.Object.ReferenceEquals(other, null))
            {
                return false;
            }
            return this.StockName == other.StockName &&
                   this.Date == other.Date &&
                   this.Alert == other.Alert &&
                   this.BarDuration == other.BarDuration &&
                   this.AlertClose == other.AlertClose;
        }
    }
}
