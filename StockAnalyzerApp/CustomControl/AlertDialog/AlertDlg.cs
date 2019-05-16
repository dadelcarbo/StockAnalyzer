using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.AlertDialog
{
    public partial class AlertDlg : Form
    {
        private AlertControl alertControl;
        StockAlertConfig alertConfig;

        public AlertDlg(StockAlertConfig alertCfg)
        {
            InitializeComponent();

            this.alertControl = this.elementHost1.Child as AlertControl;
            this.alertConfig = alertCfg;
            this.alertControl.SelectedTimeFrame = alertCfg;

            StockAnalyzerForm.MainFrame.AlertDetected += MainFrame_AlertDetected;
            StockAnalyzerForm.MainFrame.AlertDetectionProgress += MainFrame_AlertDetectionProgress;
            StockAnalyzerForm.MainFrame.AlertDetectionStarted += MainFrame_AlertDetectionStarted;            
        }

        public AlertDlg(StockAlertLog dailyAlertLog, List<StockAlertDef> dailyAlertDefs)
        {
            throw new NotImplementedException("AlertDlg(StockAlertLog dailyAlertLog, List<StockAlertDef> dailyAlertDefs)");
        }

        void MainFrame_AlertDetectionProgress(string stockName)
        {
            alertConfig.AlertLog.ProgressName = stockName;
            alertConfig.AlertLog.ProgressValue++;
        }

        void MainFrame_AlertDetectionStarted(int nbStock)
        {
            alertConfig.AlertLog.ProgressValue = 0;
            alertConfig.AlertLog.ProgressMax = nbStock;
            alertConfig.AlertLog.ProgressVisibility = true;
        }

        void MainFrame_AlertDetected()
        {
            this.Activate();
            alertConfig.AlertLog.ProgressValue = 0;
            alertConfig.AlertLog.ProgressVisibility = false;
        }
    }
}
