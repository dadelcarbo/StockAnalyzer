﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using StockAnalyzerSettings.Properties;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs
{
   public partial class YahooDataProviderConfigDlg : Form
   {
      YahooDataProvider dataProvider = null;
      StockDictionary stockDictionary = null;

      bool needRestart = false;

      public YahooDataProviderConfigDlg(StockDictionary stockDico)
      {
         InitializeComponent();

         this.dataProvider = new YahooDataProvider();
         this.stockDictionary = stockDico;

         this.step1GroupBox.Enabled = true;
         this.step2GroupBox.Enabled = false;

         // Init group combo box
         this.groupComboBox.Items.Clear();
         this.groupComboBox.Items.AddRange(stockDico.GetValidGroupNames().ToArray());

         string[] userGroups = { "USER1", "USER2", "USER3" };
         foreach (string group in userGroups)
         {
            if (!this.groupComboBox.Items.Contains(group))
            {
               this.groupComboBox.Items.Add(group);
            }
         }
         this.groupComboBox.SelectedItem = userGroups[0];

         // Init personal list view
         string fileName = Settings.Default.RootFolder + this.dataProvider.UserConfigFileName;
         if (File.Exists(fileName))
         {
            using (StreamReader sr = new StreamReader(fileName, true))
            {
               string line;
               while (!sr.EndOfStream)
               {
                  line = sr.ReadLine();
                  if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                  {
                     string[] row = line.Split(',');

                     ListViewItem viewItem = new ListViewItem(row[0]);
                     viewItem.SubItems.Add(row[1]);
                     viewItem.SubItems.Add(row[2]);
                     this.personalListView.Items.Add(viewItem);
                  }
               }
            }
         }
         needRestart = false;
      }
      private void testButton_Click(object sender, EventArgs e)
      {
         bool succeeded = false;

         if (string.IsNullOrWhiteSpace(this.symbolTextBox.Text)) return;

         this.symbolTextBox.Text = this.symbolTextBox.Text.ToUpper().Trim();

         // Check if already registered in the dico
         if (this.stockDictionary.Values.Count(s => s.ShortName == this.symbolTextBox.Text) > 0)
         {
            // This serie already registered is system configuration
            MessageBox.Show(DataProviderString.DlgContent_SerieAlreadyRegistered, DataProviderString.DlgTitle_SerieAlreadyRegistered, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
         }

         bool found = false;
         foreach (ListViewItem viewItem in this.personalListView.Items)
         {
            if (viewItem.Text == this.symbolTextBox.Text)
            {
               found = true;
               break;
            }
         }
         if (found)
         {
            // This serie is already registered in user configuration
            MessageBox.Show(DataProviderString.DlgContent_SerieAlreadyRegistered, DataProviderString.DlgTitle_SerieAlreadyRegistered, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
         }

         if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
         {
            try
            {
               this.Cursor = Cursors.WaitCursor;

               using (WebClient wc = new WebClient())
               {
                  wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
                  string url = "http://download.finance.yahoo.com/d/quotes.csv?s=" + this.symbolTextBox.Text.Replace("^", "%5E") + "&f=snl1nn&e=.csv";
                  string value = wc.DownloadString(url);
                  string[] row = value.Replace("\"", "").Trim().Split(',');
                  if (row.Length >= 3 & row[0] == this.symbolTextBox.Text && row[2] != "0.00")
                  {
                     succeeded = true;
                     this.nameTextBox.Text = row[1];
                  }
                  else
                  {
                     succeeded = false;
                  }
               }
            }
            catch
            {
               succeeded = false;
            }
            finally
            {
               this.Cursor = Cursors.Arrow;
            }
            if (succeeded)
            {
               step1GroupBox.Enabled = false;
               step2GroupBox.Enabled = true;
            }
            else
            {
               MessageBox.Show(DataProviderString.DlgContent_SymbolNotFound, DataProviderString.DlgTitle_SymbolNotFound, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
         }
      }
      private void personalListView_MouseClick(object sender, MouseEventArgs e)
      {
         if (e.Button == System.Windows.Forms.MouseButtons.Right && this.personalListView.SelectedItems.Count > 0)
         {
            this.contextMenuStrip.Tag = this.personalListView.SelectedItems;
            this.contextMenuStrip.Show(this.personalListView, e.X, e.Y);
         }
      }
      private void backToStep1Button_Click(object sender, EventArgs e)
      {
         this.step1GroupBox.Enabled = true;
         this.step2GroupBox.Enabled = false;
      }
      private void removeToolStripMenuItem_Click(object sender, EventArgs e)
      {
         foreach (ListViewItem viewItem in (ListView.SelectedListViewItemCollection)this.contextMenuStrip.Tag)
         {
            if (this.stockDictionary.ContainsKey(viewItem.SubItems[1].Text))
            {
               this.stockDictionary.Remove(viewItem.SubItems[1].Text);
            }
            this.personalListView.Items.Remove(viewItem);
         }
         this.contextMenuStrip.Tag = null;
      }
      void personalListView_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
      {
         if (e.KeyCode == System.Windows.Forms.Keys.Delete)
         {
            foreach (ListViewItem viewItem in this.personalListView.SelectedItems)
            {
               if (this.stockDictionary.ContainsKey(viewItem.SubItems[1].Text))
               {
                  this.stockDictionary.Remove(viewItem.SubItems[1].Text);
               }
               this.personalListView.Items.Remove(viewItem);
            }
         }
      }
      private void cancelButton_Click(object sender, EventArgs e)
      {
         this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.Close();
      }
      private void okButton_Click(object sender, EventArgs e)
      {
         string fileName = Settings.Default.RootFolder + this.dataProvider.UserConfigFileName;
         using (StreamWriter sw = new StreamWriter(fileName, false))
         {
            sw.WriteLine("# Personal Yahoo download config file");
            foreach (ListViewItem viewItem in this.personalListView.Items)
            {
               sw.WriteLine(viewItem.Text + "," + viewItem.SubItems[1].Text + "," + viewItem.SubItems[2].Text);
            }
         }
         if (needRestart)
         {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
         }
         else
         {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         }
         this.Close();
      }
      private void addButton_Click(object sender, EventArgs e)
      {
         if (this.stockDictionary.Values.Count(s => s.StockName == this.nameTextBox.Text) == 0)
         {
            ListViewItem newItem = new ListViewItem(this.symbolTextBox.Text);
            newItem.SubItems.Add(this.nameTextBox.Text);
            newItem.SubItems.Add(this.groupComboBox.SelectedItem.ToString());
            this.personalListView.Items.Insert(0, newItem);

            needRestart = true;

            // Back to step 1
            this.step1GroupBox.Enabled = true;
            this.step2GroupBox.Enabled = false;
            this.symbolTextBox.Text = string.Empty;
         }
         else
         {
            MessageBox.Show(DataProviderString.DlgContent_SerieNameAlreadyExists, DataProviderString.DlgTitle_SerieNameAlreadyExists, MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }
   }
}