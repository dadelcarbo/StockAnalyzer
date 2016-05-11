using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl.AlertDialog
{
   public partial class AlertDlg : Form
   {
      private StockAlertLog alertLog;
      private AlertControl alertControl;

      public AlertDlg()
      {
         InitializeComponent();

         this.alertControl = this.elementHost1.Child as AlertControl;

         alertLog = StockAlertLog.Instance;
         this.alertControl.DataContext = alertLog;

         StockAnalyzerForm.MainFrame.AlertDetected += MainFrame_AlertDetected;
         StockAnalyzerForm.MainFrame.AlertDetectionProgress += MainFrame_AlertDetectionProgress;
         StockAnalyzerForm.MainFrame.AlertDetectionStarted += MainFrame_AlertDetectionStarted;
      }

      void MainFrame_AlertDetectionProgress(string stockName)
      {
         alertLog.ProgressName = stockName;
         alertLog.ProgressValue ++;
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
