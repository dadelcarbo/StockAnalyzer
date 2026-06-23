using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using StockAnalyzerApp.StockData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockData
{
    public class DataSerie
    {
        public StockInstrument Instrument { get; set; }
        public DataSerie(StockInstrument instrument, BarDuration barDuration, StockDailyValue[] values)
        {
            this.Instrument = instrument;
            this.StockName = instrument.DisplayName;
            this.BarDuration = barDuration;
            this.Values = values;

            this.ValueSeries = new FloatSerie[Enum.GetValues(typeof(StockDataType)).Length];
        }
        public StockDailyValue[] Values;
        public StockDailyValue this[DateTime key] => this.Values?.FirstOrDefault(v => v.DATE == key);

        public string StockName { get; set; }

        public BarDuration BarDuration { get; set; }
        public int LastIndex => Values == null ? -1 : Values.Length - 1;
        public StockDailyValue LastValue => Values == null || Values.Length == 0 ? null : Values[Values.Length - 1];

        public int Count => Values == null ? 0 : Values.Length;


        public FloatSerie[] ValueSeries { get; set; }
        public FloatSerie GetSerie(StockDataType dataType)
        {
            if (ValueSeries[(int)dataType] == null)
            {
                switch (dataType)
                {
                    case StockDataType.CLOSE:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.CLOSE).ToArray(), "CLOSE");
                        break;
                    case StockDataType.OPEN:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.OPEN).ToArray(), "OPEN");
                        break;
                    case StockDataType.HIGH:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.HIGH).ToArray(), "HIGH");
                        break;
                    case StockDataType.LOW:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.LOW).ToArray(), "LOW");
                        break;
                    case StockDataType.BODYHIGH:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.BodyHigh).ToArray(), "BODYHIGH");
                        break;
                    case StockDataType.BODYLOW:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.BodyLow).ToArray(), "BODYLOW");
                        break;
                    case StockDataType.VARIATION:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.VARIATION).ToArray(), "VARIATION");
                        break;
                    case StockDataType.ATR:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.ATR).ToArray(), "ATR");
                        break;
                    case StockDataType.ADR:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.ADR).ToArray(), "ADR");
                        break;
                    case StockDataType.VOLUME:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.VOLUME * 1.0f).ToArray(), "VOLUME");
                        break;
                    case StockDataType.EXCHANGED:
                        ValueSeries[(int)dataType] = new FloatSerie(this.Values.Select(d => d.EXCHANGED).ToArray(), "EXCHANGED");
                        break;
                }
            }

            return ValueSeries[(int)dataType];
        }

    }
}
