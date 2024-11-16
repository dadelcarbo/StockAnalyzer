using StockAnalyzer.StockClasses;
using StockAnalyzer.StockDrawing;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace StockAnalyzerApp.CustomControl.TrendDlgs
{
    /// <summary>
    /// Interaction logic for BestTrendUserControl.xaml
    /// </summary>
    public partial class BestTrendUserControl : UserControl
    {
        public event StockAnalyzerForm.SelectedStockAndDurationAndIndexChangedEventHandler SelectedStockChanged;

        private System.Windows.Forms.Form Form { get; }
        public BestTrendUserControl(System.Windows.Forms.Form form)
        {
            InitializeComponent();

            this.Form = form;
        }

        public BestTrendViewModel ViewModel => (BestTrendViewModel)this.DataContext;

        private void performBtn_Click(object sender, RoutedEventArgs e)
        {
            Cursor previousCursor = this.Cursor;
            this.Cursor = Cursors.Wait;

            this.ViewModel.Perform();

            this.Cursor = previousCursor;
        }

        private void grid_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            // Open on the alert stock
            var momentum = ((RadGridView)sender).SelectedItem as MomentumViewModel;

            if (momentum == null) return;

            if (SelectedStockChanged != null)
            {
                try
                {
                    DrawingItem.CreatePersistent = false;
                    momentum.StockSerie.StockAnalysis.DeleteTransientDrawings();

                    if (!momentum.StockSerie.StockAnalysis.DrawingItems.ContainsKey(momentum.BarDuration))
                    {
                        momentum.StockSerie.StockAnalysis.DrawingItems.Add(momentum.BarDuration, new StockDrawingItems());
                    }
                    momentum.StockSerie.StockAnalysis.DrawingItems[momentum.BarDuration].Add(
                        new Rectangle2D(
                            new PointF(momentum.StartIndex, momentum.StockSerie.GetSerie(StockDataType.LOW)[momentum.StartIndex]),
                            new PointF(momentum.EndIndex, momentum.StockSerie.GetSerie(StockDataType.HIGH)[momentum.EndIndex])));

                    DrawingItem.KeepTransient = true;
                    this.SelectedStockChanged(momentum.StockSerie.StockName, Math.Max(0, momentum.StartIndex - 100), Math.Min(momentum.StockSerie.Count - 1, momentum.EndIndex + 100), momentum.BarDuration, true);
                    this.Form.TopMost = true;
                    this.Form.TopMost = false;
                }
                catch { }
                finally
                {
                    DrawingItem.CreatePersistent = true;
                    DrawingItem.KeepTransient = false;
                }
            }
        }
        private void grid_FilterOperatorsLoading(object sender, FilterOperatorsLoadingEventArgs e)
        {
            var column = e.Column as Telerik.Windows.Controls.GridViewBoundColumnBase;
            if (column != null && column.DataType == typeof(string))
            {
                e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.Contains;
                e.DefaultOperator2 = Telerik.Windows.Data.FilterOperator.Contains;
            }
            else if (column != null && column.DataType == typeof(DateTime))
            {
                e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.IsGreaterThanOrEqualTo;
                e.DefaultOperator2 = Telerik.Windows.Data.FilterOperator.IsGreaterThanOrEqualTo;
            }
        }
    }
}
