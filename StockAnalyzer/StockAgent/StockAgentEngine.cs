using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent
{
    public class StockAgentEngine
    {
        public StockContext Context { get; set; }

        public StockAgentEngine(StockSerie serie)
        {
            this.Context = new StockContext()
            {
                CurrentIndex = 100,
                Serie = serie,
                PositionStatus = StokPositionStatus.Closed
            };
        }

        public void Perform()
        {
            IStockAgent agent = new StupidAgent(this.Context);
            FloatSerie closeSerie = this.Context.Serie.GetSerie(StockDataType.CLOSE);
            List<float> gains = new List<float>();
            for (int i = 100; i < this.Context.Serie.Count; i++)
            {
                this.Context.CurrentIndex = i;

                switch (agent.Decide())
                {
                    case TradeAction.Nothing:
                        break;
                    case TradeAction.Buy:
                        this.Context.PositionStatus = StokPositionStatus.Opened;
                        this.Context.OpenIndex = i;
                        this.Context.OpenValue = closeSerie[i];
                        break;
                    case TradeAction.Sell:
                        this.Context.PositionStatus = StokPositionStatus.Closed;
                        gains.Add((closeSerie[i] - this.Context.OpenValue) / this.Context.OpenValue);
                        this.Context.OpenIndex = -1;
                        this.Context.OpenValue = float.NaN;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            foreach (var gain in gains)
            {
                Console.WriteLine(gain.ToString("P2"));
            }
        }
    }
}
