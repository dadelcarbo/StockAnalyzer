using System.Collections.Generic;

namespace TradeLearning.Model.Trading
{
    public class TradingSimulator
    {
        public double Cash { get; private set; }
        public int positionSize { get; private set; }
        public float risk { get; private set; }
        public double PortfolioValue => Cash + positionSize * _currentPrice;

        private double _currentPrice;
        private readonly double[] _priceSeries;
        private readonly ITradingStrategy _strategy;

        public List<(int Index, TradeAction Action, double Price)> TradeLog { get; } = new();

        public TradingSimulator(double[] priceSeries, ITradingStrategy strategy, double initialCash = 10000)
        {
            _priceSeries = priceSeries;
            _strategy = strategy;
            Cash = initialCash;
            positionSize = 0;
        }

        double[] portfolioSerie;
        public void Run()
        {
            portfolioSerie = new double[_priceSeries.Length];
            _strategy.Initialize(_priceSeries);
            for (int i = 0; i < _priceSeries.Length; i++)
            {
                _currentPrice = _priceSeries[i];
                var action = _strategy.Decide(i, positionSize > 0);

                switch (action)
                {
                    case TradeAction.Buy:
                        if (Cash >= _currentPrice)
                        {
                            positionSize = 1;
                            Cash -= _currentPrice;
                            TradeLog.Add((i, action, _currentPrice));
                        }
                        break;

                    case TradeAction.Sell:
                        if (positionSize > 0)
                        {
                            positionSize = 0;
                            Cash += _currentPrice;
                            TradeLog.Add((i, action, _currentPrice));
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
