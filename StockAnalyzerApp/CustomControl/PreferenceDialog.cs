using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using StockAnalyzer.StockWeb;
using StockAnalyzerSettings;
using StockAnalyzerSettings.Properties;

namespace StockAnalyzerApp.CustomControl
{
    public partial class PreferenceDialog : Form
    {
        bool needRestart = false;
        public PreferenceDialog()
        {
            InitializeComponent();

            this.startYearTextBox.Maximum = DateTime.Today.Year - 2;
            this.downloadDataCheckBox.Checked = Settings.Default.DownloadData;
            this.generateBreadthCheckBox.Checked = Settings.Default.GenerateBreadth;
            this.enableLoggingCheckBox.Checked = Settings.Default.LoggingEnabled;
            this.showBarSmoothingCheckBox.Checked = Settings.Default.ShowBarSmoothing;
            this.barNumberUpDown.Value = Settings.Default.DefaultBarNumber;
            this.showVariationCheckBox.Checked = Settings.Default.ShowVariation;
            this.userIDTextBox.Text = Settings.Default.UserId;
            this.startYearTextBox.Value = Settings.Default.LoadStartYear;
            this.smtpTextBox.Text = Settings.Default.UserSMTP;
            this.addressTextBox.Text = Settings.Default.UserEMail;
            this.alertFrequencyUpDown.Value = Settings.Default.AlertsFrequency;
            this.alertActiveCheckBox.Checked = Settings.Default.RaiseAlerts;
            this.generateDailyReportCheckBox.Checked = Settings.Default.GenerateDailyReport;
            this.dataFolderTextBox.Text = Folders.DataFolder;
            this.personalFolderTextBox.Text = Folders.PersonalFolder;
            needRestart = false;
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            // Save to properties
            Settings.Default.DownloadData = this.downloadDataCheckBox.Checked;
            Settings.Default.GenerateBreadth = this.generateBreadthCheckBox.Checked;
            Settings.Default.LoggingEnabled = this.enableLoggingCheckBox.Checked;
            Settings.Default.ShowBarSmoothing = this.showBarSmoothingCheckBox.Checked;
            Settings.Default.DefaultBarNumber = (int)this.barNumberUpDown.Value;
            Settings.Default.ShowVariation = this.showVariationCheckBox.Checked;
            Settings.Default.UserId = this.userIDTextBox.Text;
            Settings.Default.LoadStartYear = (int)this.startYearTextBox.Value;
            Settings.Default.UserSMTP = this.smtpTextBox.Text;
            Settings.Default.UserEMail = this.addressTextBox.Text;

            Settings.Default.AlertsFrequency = (int)this.alertFrequencyUpDown.Value;
            Settings.Default.RaiseAlerts = this.alertActiveCheckBox.Checked;
            Settings.Default.GenerateDailyReport = this.generateDailyReportCheckBox.Checked;

            Folders.DataFolder = this.dataFolderTextBox.Text;
            Folders.PersonalFolder = this.personalFolderTextBox.Text;

            Settings.Default.Save();

            if (needRestart)
            {
                var result = MessageBox.Show(msg1.Text, "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    Application.Restart();
                    Environment.Exit(0);
                }
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
            this.Close();
        }

        private void generateBreadthCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            needRestart |= this.generateBreadthCheckBox.Checked;
        }
        private void showBarSmoothingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.ShowBarSmoothing = this.showBarSmoothingCheckBox.Checked;
            StockAnalyzerForm.MainFrame.UpdateBarSmoothingVisibility();
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

        private void browseDataButton_Click(object sender, EventArgs e)
        {
            this.folderBrowserDlg.SelectedPath = this.dataFolderTextBox.Text;
            if (this.folderBrowserDlg.ShowDialog(this) == DialogResult.OK)
            {
                Folders.DataFolder = this.folderBrowserDlg.SelectedPath;
                this.dataFolderTextBox.Text = Folders.DataFolder;
                needRestart = true;
            }
        }

        private void dataFolderTextBox_TextChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(dataFolderTextBox.Text))
            {
                Folders.DataFolder = dataFolderTextBox.Text;
                needRestart = true;
            }
        }

        private void dataFolderTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Directory.Exists(dataFolderTextBox.Text))
            {
                dataFolderTextBox.BackColor = Color.Red;
                e.Cancel = true;
            }
            else
            {
                dataFolderTextBox.BackColor = Color.White;
            }
        }

        private void personalFolderTextBox_TextChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(personalFolderTextBox.Text))
            {
                Folders.PersonalFolder = personalFolderTextBox.Text;
                needRestart = true;
            }
        }

        private void personalFolderTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Directory.Exists(personalFolderTextBox.Text))
            {
                personalFolderTextBox.BackColor = Color.Red;
                e.Cancel = true;
            }
            else
            {
                personalFolderTextBox.BackColor = Color.White;
            }
        }

        private void browsePersonalButton_Click(object sender, EventArgs e)
        {
            this.folderBrowserDlg.SelectedPath = this.personalFolderTextBox.Text;
            if (this.folderBrowserDlg.ShowDialog(this) == DialogResult.OK)
            {
                Folders.PersonalFolder = this.folderBrowserDlg.SelectedPath;
                this.personalFolderTextBox.Text = Folders.PersonalFolder;
                needRestart = true;
            }
        }
    }
}
