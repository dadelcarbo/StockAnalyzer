using System;
using System.Linq;

namespace StockAnalyzer.StockClasses
{
    public class StockAlert : IEquatable<StockAlert>
    {
        public StockAlert()
        {
        }
        public DateTime Date { get; set; }
        public string StockName { get; set; }
        public string StockGroup { get; set; }
        public int AlertDefId { get; set; }
        public string Theme => this.alertDef?.Theme;
        public StockBarDuration BarDuration => alertDef?.BarDuration;
        public string AlertDescription
        {
            get
            {
                if (alertDef == null)
                    return null;
                switch (this.alertDef.Type)
                {
                    case AlertType.Group:
                        {
                            return this.alertDef?.Title;
                        }
                    case AlertType.Stock:
                        {
                            return "User Alert: " + this.alertDef.EventName;
                        }
                    case AlertType.Price:
                        {
                            return this.alertDef.TriggerBrokenUp ? $"{this.alertDef.PriceTrigger} Broken Up" : $"{this.alertDef.PriceTrigger} Broken Down";
                        }
                };
                return null;
            }
        }

        public float StopValue { get; set; }
        public float StopPercent => float.IsNaN(StopValue) ? float.NaN : (AlertClose - StopValue) / AlertClose;
        public float AlertClose { get; set; }
        public float Speed { get; set; }

        public long ExchangedMoney { get; set; }

        private StockAlertDef alertDef;
        public void SetAlertDef()
        {
            this.alertDef = StockAlertConfig.AllAlertDefs.FirstOrDefault(alertDef => alertDef.Id == this.AlertDefId);
        }

        public StockAlert(StockAlertDef alertDef, DateTime date, string stockName, string stockGroup, float alertClose, float alertStop, long volume, float speed)
        {
            this.alertDef = alertDef;
            this.AlertDefId = alertDef.Id;
            Date = date;
            StockName = stockName;
            StockGroup = stockGroup;
            AlertClose = alertClose;
            StopValue = alertStop;
            Speed = speed;
            ExchangedMoney = (int)Math.Round(alertClose * (float)volume / 1000.0f);
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
            return this.AlertDefId == other.AlertDefId &&                   this.StockName == other.StockName &&                   this.Date == other.Date;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
