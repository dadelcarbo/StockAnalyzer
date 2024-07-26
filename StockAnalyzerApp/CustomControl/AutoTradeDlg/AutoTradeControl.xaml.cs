using StockAnalyzer.StockPortfolio.AutoTrade;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.AutoTradeDlg
{
    /// <summary>
    /// Interaction logic for AutoTradeControl.xaml
    /// </summary>
    public partial class AutoTradeControl : UserControl
    {
        System.Windows.Forms.Form form;

        public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockChanged;
        public event StockAnalyzerForm.SelectedStockAndDurationAndThemeChangedEventHandler SelectedStockAndThemeChanged;

        public AutoTradeControl(System.Windows.Forms.Form form)
        {
            InitializeComponent();

            this.DataContext = Resources["ViewModel"];
            this.form = form;
        }

        private void AgentRunGrid_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            // Open on the alert stock
            var agent = (((RadGridView)sender).SelectedItem as AgentRunViewModel)?.Agent?.AgentDef;
            if (agent == null)
                return;

            UpdateMainWindow(agent);
        }

        private void AgentDefsGrid_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            // Open on the alert stock
            var agent = ((RadGridView)sender).SelectedItem as TradeAgentDef;
            if (agent == null)
                return;

            UpdateMainWindow(agent);
        }

        void UpdateMainWindow(TradeAgentDef agent)
        {
            StockAnalyzerForm.MainFrame.Activate();
            if (!string.IsNullOrEmpty(agent.Theme))
            {
                this.SelectedStockAndThemeChanged?.Invoke(agent.StockName, agent.BarDuration, agent.Theme, true);
            }
            else
            {
                this.SelectedStockChanged(agent.StockName, agent.BarDuration, true);
                // §§§§
                //var alertDef = StockAlertConfig.AllAlertDefs.FirstOrDefault(a => a.Id == agent.AlertDefId);
                //if (alertDef != null)
                //{
                //    StockAnalyzerForm.MainFrame.SetThemeFromIndicator(alertDef.IndicatorFullName);
                //}
            }
            this.form.TopMost = true;
            this.form.TopMost = false;
        }
    }
}
