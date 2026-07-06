using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;
using StockAnalyzer.StockMath;
using StockAnalyzerApp.StockData;
using System;
using System.Linq;

namespace StockAnalyzer.StockAgent
{
    public class StockTrade
    {
        public int Qty { get; set; }
        public StockInstrument Instrument { get; private set; }
        public DataSerie DataSerie { get; set; }

        public int EntryIndex { get; private set; }
        public DateTime EntryDate { get; private set; }
        public float EntryValue { get; private set; }
        public float EntryAmount => EntryValue * Qty;
        public float EntryStop { get; private set; }

        public int ExitIndex { get; private set; }
        public DateTime ExitDate { get; private set; }

        public float ExitValue { get; private set; }
        public float ExitAmount => ExitValue * Qty;

        public bool IsClosed { get; private set; }
        public bool IsStopped { get; private set; }

        public int Duration => IsClosed ? ExitIndex - EntryIndex : this.DataSerie.LastIndex - EntryIndex;

        public float Gain { get; private set; }
        public float Drawdown { get; private set; }

        public float RiskRewardRatio { get; private set; }

        public StockTrade(DataSerie dataSerie, int entryIndex, float entryValue, int qty = 1, float stop = 0)
        {
            this.DataSerie = dataSerie;
            this.Instrument = dataSerie.Instrument;

            this.EntryValue = entryValue;
            this.EntryStop = stop;
            this.EntryIndex = entryIndex;
            this.EntryDate = dataSerie.Values[entryIndex].DATE;

            this.ExitIndex = -1;
            this.Qty = qty;


            this.Gain = 0;
            this.Drawdown = 0;

            this.IsClosed = false;
        }

        public void Close(int exitIndex, float exitValue, bool stopped = false)
        {
            this.ExitIndex = exitIndex;

            this.ExitValue = exitValue;
            this.ExitDate = this.DataSerie.Values[exitIndex].DATE;

            this.Gain = (this.ExitValue - this.EntryValue) / this.EntryValue;
            float minValue = this.DataSerie.GetSerie(StockDataType.LOW).GetMin(this.EntryIndex, exitIndex);
            this.Drawdown = (minValue - this.EntryValue) / this.EntryValue;
            this.RiskRewardRatio = (this.ExitValue - this.EntryValue) / (this.EntryValue - this.EntryStop);

            this.IsClosed = true;
            this.IsStopped = stopped;
        }
        public void CloseAtOpen(int exitIndex)
        {
            if (exitIndex >= this.DataSerie.Count)
                throw new InvalidOperationException("Cannot close trade at next open because its latest bar");

            this.ExitIndex = exitIndex;

            this.ExitValue = this.DataSerie.GetSerie(StockDataType.OPEN)[exitIndex];
            this.ExitDate = this.DataSerie.Values[exitIndex].DATE;

            this.Gain = (this.ExitValue - this.EntryValue) / this.EntryValue;
            float minValue = this.DataSerie.GetSerie(StockDataType.LOW).GetMin(this.EntryIndex, exitIndex - 1);
            this.Drawdown = (minValue - this.EntryValue) / this.EntryValue;
            this.RiskRewardRatio = (this.ExitValue - this.EntryValue) / (this.EntryValue - this.EntryStop);
            this.IsClosed = true;
        }

        public void Evaluate()
        {
            if (this.IsClosed)
                throw new InvalidOperationException("Cannot evaluate a closed trade");

            this.ExitValue = this.DataSerie.GetSerie(StockDataType.CLOSE).Last;

            this.Gain = (this.ExitValue - this.EntryValue) / this.EntryValue;
            float minValue = this.DataSerie.GetSerie(StockDataType.LOW).GetMin(this.EntryIndex, this.DataSerie.LastIndex);
            this.Drawdown = (minValue - this.EntryValue) / this.EntryValue;
            this.RiskRewardRatio = (this.ExitValue - this.EntryValue) / (this.EntryValue - this.EntryStop);
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
            var closeSerie = this.DataSerie.GetSerie(StockDataType.CLOSE);

            float indexValue = closeSerie[index];

            return (indexValue - this.EntryValue) / this.EntryValue;
        }

        public float AmountAt(int index)
        {
            if (index < this.EntryIndex)
            {
                throw new ArgumentOutOfRangeException("Cannot evaluate before trade is opened stock: " + this.Instrument.DisplayName);
            }
            if (this.IsClosed && index > this.EntryIndex)
            {
                throw new ArgumentOutOfRangeException("Cannot evaluate after trade is closed stock: " + this.Instrument.DisplayName);
            }
            var closeSerie = this.DataSerie.GetSerie(StockDataType.CLOSE);
            return Qty * closeSerie[index];
        }

        public static string ToHeaderLog()
        {
            return "StockName;EntryIndex;ExitIndex;EntryValue;ExitValue;Gain;DrawDown;RiskRewardRatio";
        }
        public string ToLog()
        {
            return $"{this.Instrument.DisplayName};{this.EntryIndex};{this.ExitIndex};{this.EntryValue};{this.ExitValue};{this.Gain.ToString("P2")};{this.Drawdown.ToString("P2")};{this.RiskRewardRatio}";
        }
    }
}
