using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl.HorseRaceDlgs
{
   public partial class HorseRaceDlg : Form
   {
      public HorseRaceDlg(string group, StockSerie.StockBarDuration barDuration)
      {
         InitializeComponent();

         (this.elementHost1.Child as HorseRaceControl).ViewModel.BarDuration = barDuration;
         (this.elementHost1.Child as HorseRaceControl).ViewModel.Group = group;
         (this.elementHost1.Child as HorseRaceControl).SelectedStockChanged += StockAnalyzerForm.MainFrame.OnSelectedStockChanged;

         StockAnalyzerForm.MainFrame.NotifyBarDurationChanged += MainFrame_NotifyBarDurationChanged;
      }

      void MainFrame_NotifyBarDurationChanged(StockSerie.StockBarDuration barDuration)
      {
         (this.elementHost1.Child as HorseRaceControl).ViewModel.BarDuration = barDuration;
      }

      protected override void OnClosed(EventArgs e)
      {
         (this.elementHost1.Child as HorseRaceControl).SelectedStockChanged -= StockAnalyzerForm.MainFrame.OnSelectedStockChanged;
         StockAnalyzerForm.MainFrame.NotifyBarDurationChanged -= MainFrame_NotifyBarDurationChanged;
         base.OnClosed(e);
      }
   }
}
