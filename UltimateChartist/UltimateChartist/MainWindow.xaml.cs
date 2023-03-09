using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Telerik.Windows.Controls;
using UltimateChartist.UserControls.ChartControls;
using UltimateChartist.Indicators;
using UltimateChartist.UserControls.InstrumentControls;

namespace UltimateChartist;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainWindowViewModel viewModel;
    public MainWindow()
    {
        StyleManager.ApplicationTheme = new Office2019Theme();

        InitializeComponent();

        this.viewModel = MainWindowViewModel.Instance;
        this.viewModel.PropertyChanged += ViewModel_PropertyChanged;
        this.DataContext = viewModel;

        this.viewModel.StartUp();

        this.NewChartCommand_Executed(null, null);
    }

    private void MainTabControl_SelectionChanged(object sender, RadSelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count != 0)
        {
            this.viewModel.CurrentChartView = (e.AddedItems[0] as RadTabItem).DataContext as ChartViewModel;
        }
    }

    #region MENU COMMANDS

    #region Instruments

    InstrumentWindow instrumentBrowseWindow;
    private void InstrumentBrowseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        instrumentBrowseWindow = new InstrumentWindow();
        instrumentBrowseWindow.Closed += (s, e) => { instrumentBrowseWindow = null; };
        instrumentBrowseWindow.Show();
    }
    private void InstrumentBrowseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = instrumentBrowseWindow == null;
    }

    PalmaresWindow instrumentPalmaresWindow;
    private void InstrumentPalmaresCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        instrumentPalmaresWindow = new PalmaresWindow();
        instrumentPalmaresWindow.Closed += (s, e) => { instrumentPalmaresWindow = null; };
        instrumentPalmaresWindow.Show();
    }
    private void InstrumentPalmaresCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = instrumentPalmaresWindow == null;
    }

    ScreenerWindow instrumentScreenerWindow;
    private void InstrumentScreenerCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        instrumentScreenerWindow = new ScreenerWindow();
        instrumentScreenerWindow.Closed += (s, e) => { instrumentScreenerWindow = null; };
        instrumentScreenerWindow.Show();
    }
    private void InstrumentScreenerCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = instrumentScreenerWindow == null;
    }
    #endregion
    private void NewChartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var chartViewModel = new ChartViewModel();
        var tabItem = new RadTabItem()
        {
            DataContext = chartViewModel,
            CloseButtonVisibility = Visibility.Visible
        };
        Binding binding = new Binding();
        binding.Path = new PropertyPath("Name");
        BindingOperations.SetBinding(tabItem, RadTabItem.HeaderProperty, binding);

        tabItem.Content = new PriceChartUserControl(chartViewModel);
        this.MainTabControl.Items.Add(tabItem);
        this.MainTabControl.SelectedItem = tabItem;
    }
    private void CloseChartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (this.MainTabControl.SelectedItem != null)
        {
            this.MainTabControl.Items.Remove(this.MainTabControl.SelectedItem);
            GC.Collect();
        }
    }
    private void CloseChartCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = this.MainTabControl.SelectedItem != null;
    }

    private void AddEmaCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (this.MainTabControl.SelectedItem != null)
        {
            IIndicator indicator = new StockIndicator_TrailATR();

            this.viewModel.CurrentChartView.PriceIndicators.Add(indicator);
        }
    }
    private void AddEmaCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = this.MainTabControl.SelectedItem != null;
    }
    #endregion

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        //if (this.viewModel.ChartViews.Count == 0)
        //{
        //    this.viewModel.CurrentChartView = null;
        //}
        //else
        //{
        //    var chartViewModel = (e.RemovedItems[0] as RadTabItem)?.DataContext as ChartViewModel;
        //    this.viewModel.ChartViews.Remove(chartViewModel);
        //}
    }

}
