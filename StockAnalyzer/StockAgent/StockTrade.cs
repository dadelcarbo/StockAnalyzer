using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;
using System.Linq;

namespace StockAnalyzer.StockAgent
{
    public class StockTrade
    {
        public int Qty { get; set; }
        public StockSerie Serie { get; private set; }
        public int EntryIndex { get; private set; }
        public DateTime EntryDate { get; private set; }
        public float EntryValue { get; private set; }
        public float EntryAmount => EntryValue * Qty;
        public float EntryStop { get; private set; }

        public int ExitIndex { get; private set; }
        public DateTime ExitDate { get; private set; }

        public float ExitValue { get; private set; }
        public float ExitAmount => ExitValue * Qty;

        public bool IsLong { get; private set; }
        public bool IsClosed { get; private set; }
        public bool IsStopped { get; private set; }

        public int Duration
        {
            get
            {
                return IsClosed ?
                    ExitIndex - EntryIndex :
                    Serie.LastIndex - EntryIndex;
            }
        }

        public float Gain { get; private set; }
        public float Drawdown { get; private set; }

        public float RiskRewardRatio { get; private set; }

        FloatSerie openSerie => this.Serie.GetSerie(StockDataType.OPEN);
        FloatSerie highSerie => this.Serie.GetSerie(StockDataType.HIGH);
        FloatSerie lowSerie => this.Serie.GetSerie(StockDataType.LOW);

        public StockTrade(StockSerie serie, int entryIndex, int qty = 1, float stop = 0, bool isLong = true)
        {
            this.Serie = serie;
            this.EntryIndex = entryIndex;
            this.EntryDate = serie.Keys.ElementAt(entryIndex);
            this.ExitIndex = -1;
            this.IsLong = isLong;
            this.Qty = qty;

            this.EntryValue = openSerie[entryIndex];
            this.EntryStop = stop;

            this.Gain = 0;
            this.Drawdown = 0;

            this.IsClosed = false;
        }
        public StockTrade(StockSerie serie, int entryIndex, float entryValue, int qty = 1, bool isLong = true)
        {
            this.Serie = serie;
            this.EntryIndex = entryIndex;
            this.EntryDate = serie.Keys.ElementAt(entryIndex);
            this.ExitIndex = -1;
            this.IsLong = isLong;
            this.Qty = qty;

            this.EntryValue = entryValue;

            this.Gain = 0;
            this.Drawdown = 0;

            this.IsClosed = false;
        }

        public void Close(int exitIndex, float exitValue, bool stopped = false)
        {
            this.ExitIndex = exitIndex;

            this.ExitValue = exitValue;
            this.ExitDate = Serie.Keys.ElementAt(exitIndex);

            if (this.IsLong)
            {
                this.Gain = (this.ExitValue - this.EntryValue) / this.EntryValue;
                float minValue = lowSerie.GetMin(this.EntryIndex, exitIndex);
                this.Drawdown = (minValue - this.EntryValue) / this.EntryValue;
                this.RiskRewardRatio = (this.ExitValue - this.EntryValue) / (this.EntryValue - this.EntryStop);
            }
            else
            {
                this.Gain = (this.EntryValue - this.ExitValue) / this.EntryValue;
                float maxValue = highSerie.GetMax(this.EntryIndex, exitIndex);
                this.Drawdown = (this.EntryValue - maxValue) / this.EntryValue;
                this.RiskRewardRatio = (this.EntryValue - this.ExitValue) / (this.EntryStop - this.EntryValue);
            }

            this.IsClosed = true;
            this.IsStopped = stopped;
        }
        public void CloseAtOpen(int exitIndex)
        {
            if (exitIndex >= openSerie.Count)
                throw new InvalidOperationException("Cannot close trade at next open because its latest bar");
            this.ExitIndex = exitIndex;

            this.ExitValue = openSerie[exitIndex];
            this.ExitDate = Serie.Keys.ElementAt(exitIndex);

            if (this.IsLong)
            {
                this.Gain = (this.ExitValue - this.EntryValue) / this.EntryValue;
                float minValue = lowSerie.GetMin(this.EntryIndex, exitIndex - 1);
                this.Drawdown = (minValue - this.EntryValue) / this.EntryValue;
                this.RiskRewardRatio = (this.ExitValue - this.EntryValue) / (this.EntryValue - this.EntryStop);
            }
            else
            {
                this.Gain = (this.EntryValue - this.ExitValue) / this.EntryValue;
                float maxValue = highSerie.GetMax(this.EntryIndex, exitIndex - 1);
                this.Drawdown = (this.EntryValue - maxValue) / this.EntryValue;
                this.RiskRewardRatio = (this.EntryValue - this.ExitValue) / (this.EntryStop - this.EntryValue);
            }

            this.IsClosed = true;
        }
        public void Evaluate()
        {
            if (this.IsClosed)
                throw new InvalidOperationException("Cannot evaluate a closed trade");

            this.ExitValue = Serie.GetSerie(StockDataType.CLOSE).Last;
            if (this.IsLong)
            {
                this.Gain = (this.ExitValue - this.EntryValue) / this.EntryValue;
                float minValue = lowSerie.GetMin(this.EntryIndex, Serie.LastIndex);
                this.Drawdown = (minValue - this.EntryValue) / this.EntryValue;
                this.RiskRewardRatio = (this.ExitValue - this.EntryValue) / (this.EntryValue - this.EntryStop);
            }
            else
            {
                this.Gain = (this.EntryValue - this.ExitValue) / this.EntryValue;
                float maxValue = highSerie.GetMax(this.EntryIndex, Serie.LastIndex);
                this.Drawdown = (this.EntryValue - maxValue) / this.EntryValue;
                this.RiskRewardRatio = (this.EntryValue - this.ExitValue) / (this.EntryStop - this.EntryValue);
            }
        }

        public float GainAt(int index)
        {
            if (index < this.EntryIndex)
            {
                throw new ArgumentOutOfRangeException("Cannot get gain before trade is opened");
            }
            if (this.IsClosed && index > this.EntryIndex)
            {
                throw new ArgumentOutOfRangeException("Cannot get gain after trade is closed");
            }
            FloatSerie closeSerie = this.Serie.GetSerie(StockDataType.CLOSE);

            float indexValue = closeSerie[index];

            return this.IsLong ?
                (indexValue - this.EntryValue) / this.EntryValue :
                (this.EntryValue - indexValue) / this.EntryValue;
        }
        public float AmountAt(DateTime date)
        {
            if (date < this.EntryDate)
            {
                return this.EntryAmount;
            }
            int index = this.Serie.IndexOf(date);
            return index == -1 ? this.EntryAmount : this.AmountAt(index);
        }
        public float AmountAt(int index)
        {
            if (index < this.EntryIndex)
            {
                throw new ArgumentOutOfRangeException("Cannot evaluate before trade is opened stock: " + this.Serie.StockName);
            }
            if (this.IsClosed && index > this.EntryIndex)
            {
                throw new ArgumentOutOfRangeException("Cannot evaluate after trade is closed stock: " + this.Serie.StockName);
            }
            FloatSerie closeSerie = this.Serie.GetSerie(StockDataType.CLOSE);
            return Qty * closeSerie[index];
        }

        public static string ToHeaderLog()
        {
            return "StockName;IsLong;EntryIndex;ExitIndex;EntryValue;ExitValue;Gain;DrawDown;RiskRewardRatio";
        }
        public string ToLog()
        {
            return $"{this.Serie.StockName};{this.IsLong};{this.EntryIndex};{this.ExitIndex};{this.EntryValue};{this.ExitValue};{this.Gain.ToString("P2")};{this.Drawdown.ToString("P2")};{this.RiskRewardRatio}";
        }
    }
}
