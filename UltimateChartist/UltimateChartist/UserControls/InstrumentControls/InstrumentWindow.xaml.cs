using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Instrument = UltimateChartist.DataModels.Instrument;

namespace UltimateChartist.UserControls.InstrumentControls
{
    /// <summary>
    /// Interaction logic for InstrumentWindow.xaml
    /// </summary>
    public partial class InstrumentWindow : Window
    {
        public InstrumentWindow()
        {
            InitializeComponent();
        }

        private void RadGridView_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            MainWindowViewModel.Instance.CurrentChartView.Instrument = e.AddedItems[0] as Instrument;
        }
    }
}
