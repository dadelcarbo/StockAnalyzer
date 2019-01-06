using StockAnalyzer.StockClasses;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.AlertDialog
{
    public partial class AlertDlg : Form
    {
        private StockAlertLog alertLog;
        private AlertControl alertControl;

        public AlertDlg(StockAlertLog log, List<StockAlertDef> alertDefs)
        {
            InitializeComponent();

            this.alertControl = this.elementHost1.Child as AlertControl;

            alertLog = log;
            this.alertControl.DataContext = alertLog;
            this.alertControl.AlertDefs = alertDefs;

            StockAnalyzerForm.MainFrame.AlertDetected += MainFrame_AlertDetected;
            StockAnalyzerForm.MainFrame.AlertDetectionProgress += MainFrame_AlertDetectionProgress;
            StockAnalyzerForm.MainFrame.AlertDetectionStarted += MainFrame_AlertDetectionStarted;
            
        }

        void MainFrame_AlertDetectionProgress(string stockName)
        {
            alertLog.ProgressName = stockName;
            alertLog.ProgressValue++;
        }

        void MainFrame_AlertDetectionStarted(int nbStock)
        {
            alertLog.ProgressValue = 0;
            alertLog.ProgressMax = nbStock;
            alertLog.ProgressVisibility = true;
        }

        void MainFrame_AlertDetected()
        {
            this.Activate();
            alertLog.ProgressValue = 0;
            alertLog.ProgressVisibility = false;
        }
    }
}
