using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzerApp.StockData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockAgent.BackTests
{
    public interface IBackTest : IStockAgent
    {
        public bool Initialize(StockInstrument instrument, BarDuration duration, int minIndex);

        public float GetStop(int index);
    }

    public abstract class BackTestBase : StockAgentBase, IBackTest
    {
        new static public IBackTest CreateInstance(string shortName)
        {
            Type type = typeof(IBackTest).Assembly.GetType($"StockAnalyzer.StockAgent.BackTests.{shortName}BackTest");
            return (IBackTest)Activator.CreateInstance(type);
        }

        public bool Initialize(StockInstrument instrument, BarDuration duration, int minIndex)
        {
            try
            {
                this.Instrument = instrument;
                this.DataSerie = instrument.GetDataSerie(duration);
                this.DataSerie.ResetAllCache();

                if (this.DataSerie == null || this.DataSerie.Count < minIndex)
                {
                    return false;
                }

                closeSerie = DataSerie.GetSerie(StockDataType.CLOSE);
                openSerie = DataSerie.GetSerie(StockDataType.OPEN);
                lowSerie = DataSerie.GetSerie(StockDataType.LOW);
                highSerie = DataSerie.GetSerie(StockDataType.HIGH);
                volumeSerie = DataSerie.GetSerie(StockDataType.VOLUME);
                volumeEuroSerie = DataSerie.GetSerie(StockDataType.VOLUME).CalculateEMA(10);
                this.Trade = null;

                return Init();
            }
            catch (Exception ex)
            {
                StockLog.Write($"Agent: {this.GetType()} Exception: {ex.Message}");
                return false;
            }
        }

        public abstract float GetStop(int index);

        public override TradeAction Decide(int index)
        {
            if (this.Trade == null)
            {
                if (volumeEuroSerie[index] < 0.5f)
                    return TradeAction.Nothing;
                var action = this.TryToOpenPosition(index);
                if (action == TradeAction.Buy)
                {
                    this.EntryStopValue = this.GetStop(index);
                }
                return action;
            }
            else
            {
                if (lowSerie[index] < this.EntryStopValue)
                {
                    this.Trade.Close(index, Math.Min(this.EntryStopValue, this.openSerie[index]), true);
                    this.Trade = null;

                    return TradeAction.Nothing;
                }
                return this.TryToClosePosition(index);
            }
        }
    }
}
