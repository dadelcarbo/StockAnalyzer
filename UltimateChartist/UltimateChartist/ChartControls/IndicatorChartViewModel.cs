﻿using System.Windows.Input;
using Telerik.Windows.Controls;
using UltimateChartist.Indicators;

namespace UltimateChartist.ChartControls
{
    public class IndicatorChartViewModel : ViewModelBase
    {
        public ChartViewModel ChartViewModel { get; }
        public IIndicator Indicator { get; }

        public IndicatorChartViewModel(ChartViewModel chartViewModel, IIndicator indicator)
        {
            this.ChartViewModel = chartViewModel;
            Indicator = indicator;
        }

        private string name;
        public string Name { get => name; set { if (name != value) { name = value; RaisePropertyChanged(); } } }




        #region Commands

        private DelegateCommand deleteIndicatorCommand;
        public ICommand DeleteIndicatorCommand => deleteIndicatorCommand ??= new DelegateCommand(DeleteIndicator);

        private void DeleteIndicator(object commandParameter)
        {
            this.ChartViewModel.RemoveIndicator(this);
        }
        #endregion
    }
}