using System;
using System.Collections.Generic;

namespace TradeLearning.Model.Trading
{
    public class TradingSimulator
    {
        public double Cash { get; private set; }
        public int positionSize { get; private set; }
        public double risk { get; private set; } = 0.05;
        public double[] PortfolioValue;

        private double _currentPrice;
        private readonly double[] _priceSeries;
        private readonly ITradingStrategy _strategy;

        public List<(int Index, TradeAction Action, double Price, int Qty)> TradeLog { get; } = new();

        public TradingSimulator(double[] priceSeries, ITradingStrategy strategy, double initialCash = 10000)
        {
            _priceSeries = priceSeries;
            _strategy = strategy;
            Cash = initialCash;
            positionSize = 0;
            this.PortfolioValue = new double[priceSeries.Length];
        }

        double[] portfolioSerie;
        public void Run()
        {
            portfolioSerie = new double[_priceSeries.Length];
            _strategy.Initialize(_priceSeries);
            for (int i = 0; i < _priceSeries.Length; i++)
            {
                this.PortfolioValue[i] = Cash + positionSize * _currentPrice;
                _currentPrice = _priceSeries[i];
                var action = _strategy.Decide(i, positionSize > 0);

                switch (action)
                {
                    case TradeAction.Buy:
                        //Calculatio position Size
                        positionSize = (int)Math.Floor(Cash * risk / (_currentPrice * 0.05));
                        if (positionSize > 0)
                        {
                            Cash -= _currentPrice * positionSize;
                            TradeLog.Add((i, action, _currentPrice, positionSize));
                        }
                        break;

                    case TradeAction.Sell:
                        if (positionSize > 0)
                        {
                            Cash += _currentPrice * positionSize;
                            TradeLog.Add((i, action, _currentPrice, positionSize));
                            positionSize = 0;
                        }
                        break;

                    case TradeAction.Nop:
                        // No action
                        break;
                }
            }
        }
    }

}
