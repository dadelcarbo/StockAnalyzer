using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace StockAnalyzerApp.CustomControl.DrawingDlg
{
    /// <summary>
    /// Interaction logic for DrawingControl.xaml
    /// </summary>
    public partial class DrawingControl : UserControl
    {
        public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockAndDurationChanged;
        private System.Windows.Forms.Form Form { get; }
        public DrawingControl(System.Windows.Forms.Form form)
        {
            InitializeComponent();
            this.Form = form;
            this.drawingGridView.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnCell), true);
        }

        private void MouseDownOnCell(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (this.SelectedStockAndDurationChanged == null) return;
                var row = ((UIElement)e.OriginalSource).ParentOfType<GridViewRow>();
                if (row?.Item == null)
                    return;
                var item = row.Item as DrawingViewModel;
                if (item == null)
                    return;

                this.SelectedStockAndDurationChanged(item.StockName, item.Duration, true);
                this.Form.TopMost = true;
                this.Form.TopMost = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void FilterOperatorsLoading(object sender, FilterOperatorsLoadingEventArgs e)
        {
            var column = e.Column as GridViewBoundColumnBase;
            if (column == null)
                return;
            if (column.DataType == typeof(string))
            {
                e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.Contains;
                e.DefaultOperator2 = Telerik.Windows.Data.FilterOperator.Contains;
            }
        }

        private void RadPropertyGrid_AutoGeneratingPropertyDefinition(object sender, Telerik.Windows.Controls.Data.PropertyGrid.AutoGeneratingPropertyDefinitionEventArgs e)
        {
            var attribute = e.PropertyDefinition.PropertyDescriptor.Attributes[typeof(PropertyAttribute)];
            if (attribute == null)
                e.Cancel = true;
            else
                e.Cancel = false;
        }

        private void drawingGridView_AutoGeneratingColumn(object sender, GridViewAutoGeneratingColumnEventArgs e)
        {

        }
    }
}
