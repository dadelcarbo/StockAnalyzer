using System;
using System.Windows.Forms;
using StockAnalyzer.StockWeb;
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
         this.shortSellSupportCheckBox.Checked = Settings.Default.SupportShortSelling;
         this.generateBreadthCheckBox.Checked = Settings.Default.GenerateBreadth;
         this.enableLoggingCheckBox.Checked = Settings.Default.LoggingEnabled;
         this.barNumberUpDown.Value = Settings.Default.DefaultBarNumber;
         this.showVariationCheckBox.Checked = Settings.Default.ShowVariation;
         this.userIDTextBox.Text = Settings.Default.UserId;
         this.dateTimePicker.Value = Settings.Default.StrategyStartDate;
         this.smtpTextBox.Text = Settings.Default.UserSMTP;
         this.addressTextBox.Text = Settings.Default.UserEMail;
         this.alertFrequencyUpDown.Value = Settings.Default.AlertsFrequency;
         this.alertActiveCheckBox.Checked = Settings.Default.RaiseAlerts;
         this.generateDailyReportCheckBox.Checked = Settings.Default.GenerateDailyReport;
         needRestart = false;
      }

      private void okBtn_Click(object sender, EventArgs e)
      {
         // Save to properties
         Settings.Default.DownloadData = this.downloadDataCheckBox.Checked;
         Settings.Default.SupportShortSelling = this.shortSellSupportCheckBox.Checked;
         Settings.Default.GenerateBreadth = this.generateBreadthCheckBox.Checked;
         Settings.Default.LoggingEnabled = this.enableLoggingCheckBox.Checked;
         Settings.Default.DefaultBarNumber = (int)this.barNumberUpDown.Value;
         Settings.Default.ShowVariation = this.showVariationCheckBox.Checked;
         Settings.Default.UserId = this.userIDTextBox.Text;
         Settings.Default.StrategyStartDate = this.dateTimePicker.Value;
         Settings.Default.UserSMTP = this.smtpTextBox.Text;
         Settings.Default.UserEMail = this.addressTextBox.Text;

         Settings.Default.AlertsFrequency = (int)this.alertFrequencyUpDown.Value;
         Settings.Default.RaiseAlerts = this.alertActiveCheckBox.Checked;
         Settings.Default.GenerateDailyReport = this.generateDailyReportCheckBox.Checked;

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

      private void testButton_Click(object sender, EventArgs e)
      {
         Cursor cursor = this.Cursor;
         this.Cursor = Cursors.WaitCursor;

         Settings.Default.UserSMTP = this.smtpTextBox.Text;
         Settings.Default.UserEMail = this.addressTextBox.Text;

         StockMail.SendEmail("Test Email", "Test Email");

         this.Cursor = cursor;
      }

      private void alertFrequencyUpDown_ValueChanged(object sender, EventArgs e)
      {
         needRestart |= this.generateBreadthCheckBox.Checked;
      }

      private void AlertActiveCheckBox_CheckedChanged(object sender, EventArgs e)
      {
         needRestart |= this.generateBreadthCheckBox.Checked;
      }

      private void generateDailyReportCheckBox_CheckedChanged(object sender, EventArgs e)
      {
         needRestart |= this.generateBreadthCheckBox.Checked;
      }
   }
}
