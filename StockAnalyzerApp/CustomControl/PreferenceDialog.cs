using System;
using System.Windows.Forms;
using StockAnalyzerSettings.Properties;

namespace StockAnalyzerApp.CustomControl
{
   public partial class PreferenceDialog : Form
   {
      bool needRestart = false;
      public PreferenceDialog()
      {
         InitializeComponent();

         this.downloadDataCheckBox.Checked = Settings.Default.DownloadData;
         this.intradaySupportCheckBox.Checked = Settings.Default.SupportIntraday;
         this.shortSellSupportCheckBox.Checked = Settings.Default.SupportShortSelling;
         this.generateBreadthCheckBox.Checked = Settings.Default.GenerateBreadth;
         this.enableLoggingCheckBox.Checked = Settings.Default.LoggingEnabled;
         this.barNumberUpDown.Value = Settings.Default.DefaultBarNumber;
         this.showVariationCheckBox.Checked = Settings.Default.ShowVariation;
         this.userIDTextBox.Text = Settings.Default.UserId;
         this.dateTimePicker.Value = Settings.Default.StrategyStartDate;
         needRestart = false;
      }

      private void okBtn_Click(object sender, EventArgs e)
      {
         // Save to properties
         Settings.Default.DownloadData = this.downloadDataCheckBox.Checked;
         Settings.Default.SupportIntraday = this.intradaySupportCheckBox.Checked;
         Settings.Default.SupportShortSelling = this.shortSellSupportCheckBox.Checked;
         Settings.Default.GenerateBreadth = this.generateBreadthCheckBox.Checked;
         Settings.Default.LoggingEnabled = this.enableLoggingCheckBox.Checked;
         Settings.Default.DefaultBarNumber = (int)this.barNumberUpDown.Value;
         Settings.Default.ShowVariation = this.showVariationCheckBox.Checked;
         Settings.Default.UserId = this.userIDTextBox.Text;
         Settings.Default.StrategyStartDate = this.dateTimePicker.Value;
         Settings.Default.Save();

         if (needRestart)
         {
            MessageBox.Show(msg1.Text, "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Application.Restart();
            Environment.Exit(0);
         }

         this.Close();
      }

      private void userIDTextBox_TextChanged(object sender, EventArgs e)
      {
         needRestart = true;
      }

      private void intradaySupportCheckBox_CheckedChanged(object sender, EventArgs e)
      {
         needRestart = true;
      }

      private void getLicenseButton_Click(object sender, EventArgs e)
      {
         GetLicenseForm licenseForm = new GetLicenseForm();
         licenseForm.Show();

         this.userIDTextBox.Text = Settings.Default.UserId;
         needRestart = true;
      }

      void cancelBtn_Click(object sender, System.EventArgs e)
      {
         this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      }

      private void generateBreadthCheckBox_CheckedChanged(object sender, EventArgs e)
      {
         needRestart |= this.generateBreadthCheckBox.Checked;
      }

      private void downloadDataCheckBox_CheckedChanged(object sender, EventArgs e)
      {
         needRestart |= this.downloadDataCheckBox.Checked;
      }
   }
}
