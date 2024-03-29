﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.ExpectedValueDlg
{
    /// <summary>
    /// Interaction logic for StatisticsControl.xaml
    /// </summary>
    public partial class StatisticsControl : UserControl
    {
        readonly StatisticsViewModel viewModel = new StatisticsViewModel("TRUE(1)", "True");

        public StatisticsControl()
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }

        private void CalculateBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            try
            {
                viewModel.Calculate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Cursor = Cursors.Arrow;
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "ExpectedValue" || e.PropertyName == "MaxReturnValue" || e.PropertyName == "MinReturnValue")
            {
                (e.Column as DataGridTextColumn).Binding.StringFormat = "P2";
            }
            if (e.PropertyName == "ReturnValues")
            {
                e.Cancel = true;
            }
        }
    }
}
