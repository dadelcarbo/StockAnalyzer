using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.WatchListDlgs
{
   public partial class StockSelectorDlg : Form
   {
      public IEnumerable<string> SelectedStocks
      {
         get
         {
            return this.stockListBox.CheckedItems.Cast<string>();
         }
      }

      public StockSelectorDlg()
      {
         InitializeComponent();

         // Initialise group combo box
         groupComboBox.Items.AddRange(StockDictionary.StockDictionarySingleton.GetValidGroupNames().ToArray());
         groupComboBox.SelectedValueChanged += new EventHandler(groupComboBox_SelectedValueChanged);

         groupComboBox.SelectedIndex = 0;
      }

      private void groupComboBox_SelectedValueChanged(object sender, EventArgs e)
      {
         var stockNames = StockDictionary.StockDictionarySingleton.Values.Where(
            s => s.BelongsToGroup(groupComboBox.SelectedItem.ToString())).Select(s => s.StockName);

         stockListBox.Items.Clear();
         stockListBox.Items.AddRange(stockNames.ToArray());
      }

      private void cancelButton_Click(object sender, EventArgs e)
      {
         this.DialogResult = DialogResult.Cancel;
      }

      private void okButton_Click(object sender, EventArgs e)
      {
         this.DialogResult = DialogResult.OK;
      }
   }
}
