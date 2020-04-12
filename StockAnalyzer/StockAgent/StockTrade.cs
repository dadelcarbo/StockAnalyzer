using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockAnalyzer.StockAgent
{
    public class StockTrade
    {
        public StockSerie Serie { get; private set; }
        public int EntryIndex { get; private set; }
        public float EntryValue { get; private set; }
        public int ExitIndex { get; private set; }
        public float ExitValue { get; private set; }
        public bool IsLong { get; private set; }
        public bool IsClosed { get; private set; }

        public int Duration
        {
            get
            {
                return IsClosed ?
                    ExitIndex - EntryIndex :
                    -1;
            }
        }

        public float Gain { get; private set; }
        public float DrawDown { get; private set; }
        public float MaxGain { get; private set; }

        FloatSerie openSerie;
        FloatSerie highSerie;
        FloatSerie lowSerie;

        public StockTrade(StockSerie serie, int entryIndex, bool isLong = true)
        {
            this.Serie = serie;
            this.EntryIndex = entryIndex;
            this.ExitIndex = -1;
            this.IsLong = isLong;

            openSerie = this.Serie.GetSerie(StockDataType.OPEN);
            highSerie = this.Serie.GetSerie(StockDataType.HIGH);
            lowSerie = this.Serie.GetSerie(StockDataType.LOW);

            this.EntryValue = openSerie[entryIndex];

            this.Gain = float.NaN;
            this.MaxGain = float.NaN;
            this.DrawDown = float.NaN;

            this.IsClosed = false;
        }

        public void Close(int exitIndex)
        {
            this.ExitIndex = exitIndex;

            this.ExitValue = openSerie[exitIndex];
            float maxValue = highSerie.GetMax(this.EntryIndex, exitIndex);
            float minValue = lowSerie.GetMin(this.EntryIndex, exitIndex);

            if (this.IsLong)
            {
                this.Gain = (this.ExitValue - this.EntryValue) / this.EntryValue;
                this.MaxGain = (maxValue - this.EntryValue) / this.EntryValue;
                this.DrawDown = (minValue - this.EntryValue) / this.EntryValue;
            }
            else
            {
                this.Gain = (this.EntryValue - this.ExitValue) / this.EntryValue;
                this.MaxGain = (this.EntryValue - minValue) / this.EntryValue;
                this.DrawDown = (this.EntryValue - maxValue) / this.EntryValue;
            }

            this.IsClosed = true;
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

        public static string ToHeaderLog()
        {
            return "StockName;IsLong;EntryIndex;ExitIndex;EntryValue;ExitValue;Gain;MaxGain;DrawDown";
        }
        public string ToLog()
        {
            return this.Serie.StockName + ";" + this.IsLong + ";" + this.EntryIndex + ";" + this.ExitIndex + ";" + this.EntryValue + ";" + this.ExitValue + ";" + this.Gain.ToString("P2") + ";" + this.MaxGain.ToString("P2") + ";" + this.DrawDown.ToString("P2") + ";";
        }
    }
}
