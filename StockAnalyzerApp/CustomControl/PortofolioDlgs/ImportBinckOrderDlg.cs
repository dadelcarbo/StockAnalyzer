﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs
{
   public partial class ImportBinckOrderDlg : Form
   {
      public ImportBinckOrderDlg()
      {
         InitializeComponent();

         (this.elementHost1.Child as ImportBinckOrdersControl).ParentDialog = this;
         this.DialogResult = System.Windows.Forms.DialogResult.Ignore;
      }
   }
}