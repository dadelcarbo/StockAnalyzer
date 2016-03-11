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
      public HorseRaceDlg(string group)
      {
         InitializeComponent();

         (this.elementHost1.Child as HorseRaceControl).ViewModel.Group = group;
      }
   }
}
