using StockAnalyzer.StockClasses;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.AlertDialog
{
    public partial class AlertDlg : Form
    {
        private readonly AlertControl alertControl;

        public AlertDlg()
        {
            InitializeComponent();

            //this.alertControl = this.elementHost1.Child as AlertControl;
            //this.alertControl.TimeFrameComboBox.SelectedIndex = StockAlertConfig.AlertConfigs.IndexOf(alertCfg);

            //StockAnalyzerForm.MainFrame.AlertDetected += MainFrame_AlertDetected;
            //StockAnalyzerForm.MainFrame.AlertDetectionProgress += MainFrame_AlertDetectionProgress;
            //StockAnalyzerForm.MainFrame.AlertDetectionStarted += MainFrame_AlertDetectionStarted;
        }

        void MainFrame_AlertDetectionProgress(string stockName)
        {
            //this.alertControl.SelectedTimeFrame.AlertLog.ProgressName = stockName;
            //this.alertControl.SelectedTimeFrame.AlertLog.ProgressValue++;
        }

        void MainFrame_AlertDetectionStarted(int nbStock, string alertTitle)
        {
            //this.alertControl.SelectedTimeFrame.AlertLog.ProgressValue = 0;
            //this.alertControl.SelectedTimeFrame.AlertLog.ProgressMax = nbStock;
            //this.alertControl.SelectedTimeFrame.AlertLog.ProgressVisibility = true;
            //this.alertControl.SelectedTimeFrame.AlertLog.ProgressTitle = alertTitle;
            //this.alertControl.SelectedTimeFrame.AlertLog.ProgressName = null;
        }

        void MainFrame_AlertDetected()
        {
            //this.Activate();
            //this.alertControl.SelectedTimeFrame.AlertLog.ProgressValue = 0;
            //this.alertControl.SelectedTimeFrame.AlertLog.ProgressVisibility = false;
            //this.alertControl.SelectedTimeFrame.AlertLog.ProgressTitle = null;
            //this.alertControl.SelectedTimeFrame.AlertLog.ProgressName = null;
        }
    }
}
