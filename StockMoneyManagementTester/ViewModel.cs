using System;
using System.Linq;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace MonteCarloTester
{
    public class ViewModel : ViewModelBase
    {
        public ViewModel()
        {
            this.risk = 0.5;
            this.winLossRatio = 1;
            this.nbIter = 100;
            this.nbValues = 1000;
            this.Perform();
        }
        public GraphValue[] BestReturn { get; set; }
        public GraphValue[] WorstReturn { get; set; }
        public GraphValue[] AvgReturn { get; set; }

        public double Risk { get => risk; set { risk = value; this.Perform(); } }
        public double WinLossRatio { get => winLossRatio; set { winLossRatio = value; this.Perform(); } }


        public int NbValues { get => nbValues; set { nbValues = value; this.Perform(); } }
        public int NbIter { get => nbIter; set { nbIter = value; this.Perform(); } }

        double initialBalance = 1;

        static Random rnd = new Random();
        private double risk;
        private double winLossRatio;
        private int nbValues;
        private int nbIter;

        public void Perform(object commandParameter = null)
        {
            this.AvgReturn = new GraphValue[NbValues];
            for (int j = 0; j < nbValues; j++)
            {
                this.AvgReturn[j] = new GraphValue { X = j };
            }
            this.WorstReturn = this.BestReturn = Calculate();
            for (int i = 1; i < NbIter; i++)
            {
                var series = this.Calculate();
                if (this.WorstReturn.Last().Y > series.Last().Y)
                {
                    this.WorstReturn = series;
                }
                else if (this.BestReturn.Last().Y < series.Last().Y)
                {
                    this.BestReturn = series;
                }
                for (int j = 0; j < nbValues; j++)
                {
                    this.AvgReturn[j].Y += series[j].Y / nbIter;
                }
            }

            OnPropertyChanged("BestReturn");
            OnPropertyChanged("WorstReturn");
            OnPropertyChanged("AvgReturn");
        }

        private GraphValue[] Calculate()
        {
            GraphValue[] values = new GraphValue[NbValues];
            double balance = initialBalance;
            values[0] = new GraphValue { X = 0, Y = balance };
            for (int i = 1; i < NbValues; i++)
            {
                var tradeReturn = CalculateTradeReturn();
                balance *= 1 + (tradeReturn * this.risk / 100);
                values[i] = new GraphValue { X = i, Y = balance };
            }
            return values;
        }

        double[] returns = { -3, -2, -1, -1, -1, -1, -1, 2, 3, 9 };
        private double CalculateTradeReturn()
        {
            return returns[rnd.Next(returns.Length)];
        }

        private DelegateCommand performCommand;

        public ICommand PerformCommand
        {
            get
            {
                if (performCommand == null)
                {
                    performCommand = new DelegateCommand(Perform);
                }

                return performCommand;
            }
        }
    }
}
