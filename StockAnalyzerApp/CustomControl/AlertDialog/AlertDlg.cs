using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl.AlertDialog
{
   public partial class AlertDlg : Form
   {
      private AlertControl alertControl;
      public AlertDlg()
      {
         InitializeComponent();

         this.alertControl = this.elementHost1.Child as AlertControl;
         StockAnalyzerForm.MainFrame.AlertDetected += MainFrame_AlertDetected;
      }
      
      void MainFrame_AlertDetected()
      {
         this.alertControl.DataContext = StockAlert.ParseAlertFile();
         this.Activate();
      }
   }
}
