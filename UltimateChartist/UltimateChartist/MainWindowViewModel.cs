﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using Telerik.Windows.Controls;
using UltimateChartist.ChartControls;
using UltimateChartist.DataModels;
using UltimateChartist.DataModels.DataProviders;

namespace UltimateChartist
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Singleton
        private MainWindowViewModel()
        {
            this.Instruments = new ObservableCollection<Instrument>();
        }
        static private MainWindowViewModel instance = null;
        static public MainWindowViewModel Instance => instance ??= new MainWindowViewModel();
        #endregion

        #region Chart Views
        private ChartViewModel currentChartView;
        public ChartViewModel CurrentChartView { get => currentChartView; set { if (currentChartView != value) { currentChartView = value; RaisePropertyChanged(); } } }

        private DelegateCommand addIndicatorCommand;
        public ICommand AddIndicatorCommand => addIndicatorCommand ??= new DelegateCommand(AddIndicator);

        private void AddIndicator(object commandParameter)
        {
            if (this.CurrentChartView == null)
                return;
            this.currentChartView.AddIndicator();
        }
        #endregion

        public ObservableCollection<Instrument> Instruments { get; }

        public void StartUp()
        {
            this.Instruments.AddRange(StockDataProviderBase.InitStockDictionary());
        }
    }
}