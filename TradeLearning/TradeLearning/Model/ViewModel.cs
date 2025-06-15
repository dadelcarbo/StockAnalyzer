using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Telerik.Windows.Controls;
using TradeLearning.Model.Trading;

namespace TradeLearning.Model
{
    public class ViewModel : INotifyPropertyChanged
    {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
        #endregion

        Random rnd = new Random(123);
        public ViewModel()
        {
            this.DataSerie = DataSerie.FromArray(DataSerie.GeneratePeriodic(sampleSize, period1, amplitude1, period2, amplitude2, startPrice, drift), "Periodic");
        }

        private DataSerie dataSerie;
        public DataSerie DataSerie { get => dataSerie; set => SetProperty(ref dataSerie, value); }

        private DataSerie portfolio;
        public DataSerie Portfolio { get => portfolio; set { SetProperty(ref portfolio, value); CalculateMetrics(); } }

        private double positionRisk = 0.05;
        public double PositionRisk { get => positionRisk; set => SetProperty(ref positionRisk, value); }

        private double tradeStop = 0.1;
        public double TradeStop { get => tradeStop; set => SetProperty(ref tradeStop, value); }

        private double startPrice = 100;
        public double StartPrice { get => startPrice; set => SetProperty(ref startPrice, value); }

        private int sampleSize = 1000;
        public int SampleSize { get => sampleSize; set => SetProperty(ref sampleSize, value); }

        private int period1 = 200;
        public int Period1 { get => period1; set => SetProperty(ref period1, value); }

        private double amplitude1 = 10;
        public double Amplitude1 { get => amplitude1; set => SetProperty(ref amplitude1, value); }

        private int period2 = 50;
        public int Period2 { get => period2; set => SetProperty(ref period2, value); }

        private double amplitude2 = 2;
        public double Amplitude2 { get => amplitude2; set => SetProperty(ref amplitude2, value); }

        private double sigma = 0.025;
        public double Sigma { get => sigma; set => SetProperty(ref sigma, value); }

        private double drift = 0;
        public double Drift { get => drift; set => SetProperty(ref drift, value); }

        private int ema1 = 12;
        public int Ema1 { get => ema1; set => SetProperty(ref ema1, value); }


        private int ema2 = 36;
        public int Ema2 { get => ema2; set => SetProperty(ref ema2, value); }

        #region Start Command
        private DelegateCommand startCommand;
        public ICommand StartCommand
        {
            get
            {
                if (startCommand == null)
                {
                    startCommand = new DelegateCommand(start);
                }

                return startCommand;
            }
        }

        private void start(object commandParameter)
        {
            this.Portfolio = null;

            var engine = new TradingSimulator(this.dataSerie.Data, new Ema2TradingStrategy() { EmaPeriod1 = this.Ema1, EmaPeriod2 = this.Ema2 }, 1000);
            engine.MaxPortfolioRisk = this.PositionRisk;
            engine.StopPercent = this.TradeStop;
            engine.Run();

            this.Portfolio = DataSerie.FromArray(engine.PortfolioValue, "Portfolio");
        }
        #endregion

        private DelegateCommand generatePeriodicCommand;
        public ICommand GeneratePeriodicCommand => generatePeriodicCommand ??= new DelegateCommand(GeneratePeriodic);

        private void GeneratePeriodic(object commandParameter)
        {
            this.DataSerie = DataSerie.FromArray(DataSerie.GeneratePeriodic(sampleSize, period1, amplitude1, period2, amplitude2, startPrice, drift), "Periodic");
            this.Portfolio = null;
        }
        private DelegateCommand generateRandomCommand;
        public ICommand GenerateRandomCommand => generateRandomCommand ??= new DelegateCommand(GenerateRandom);

        private void GenerateRandom(object commandParameter)
        {
            this.DataSerie = DataSerie.FromArray(rnd.GenerateBrownianPath(startPrice, sigma, sampleSize, drift), "Periodic");
            this.Portfolio = null;
        }

        private double totalReturn;
        public double TotalReturn { get => totalReturn; set => SetProperty(ref totalReturn, value); }

        private double barReturn;
        public double BarReturn { get => barReturn; set => SetProperty(ref barReturn, value); }

        private double sharpeRatio;
        public double SharpeRatio { get => sharpeRatio; set => SetProperty(ref sharpeRatio, value); }

        private double sortinoRatio;
        public double SortinoRatio { get => sortinoRatio; set => SetProperty(ref sortinoRatio, value); }

        private double maxDrawDown;
        public double MaxDrawDown { get => maxDrawDown; set => SetProperty(ref maxDrawDown, value); }

        private void CalculateMetrics()
        {
            if (this.portfolio == null)
            {
                TotalReturn = 0; BarReturn = 0; SharpeRatio = 0; MaxDrawDown = 0; SortinoRatio = 0; return;
            }

            var returns = portfolio.Data.CalculateReturns();
            TotalReturn = (portfolio.Data.Last() - portfolio.Data.First()) / portfolio.Data.First();
            BarReturn = totalReturn / portfolio.Data.Length;
            SharpeRatio = returns.CalculateSharpeRatio();
            MaxDrawDown = portfolio.Data.CalculateMaxDrawdown();
            SortinoRatio = returns.CalculateSortinoRatio();
        }
    }
}
