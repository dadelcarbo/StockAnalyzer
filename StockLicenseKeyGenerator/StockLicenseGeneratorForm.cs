using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using StockAnalyzer.StockSecurity;
using System.IO;

namespace StockLicenseKeyGenerator
{
    public partial class StockLicenseGeneratorForm : Form
    {
        public StockLicenseGeneratorForm()
        {
            InitializeComponent();

            this.expiryDateTimePicker.Value = DateTime.Today.AddYears(1);
        }

        private void getMachineIDBtn_Click(object sender, EventArgs e)
        {
            this.machineIDTextBox.Text = StockToolKit.GetMachineUID();
        }

        private void generateKeyButton_Click(object sender, EventArgs e)
        {
            if (this.machineIDTextBox.Text == string.Empty)
            {
                this.machineIDTextBox.Text = StockToolKit.GetMachineUID();
            }
            StockLicense stockLicense = new StockLicense(this.expiryDateTimePicker.Value,
                this.pseudoTextBox.Text,
                this.machineIDTextBox.Text,
                this.featureTextBox.Lines.ToList(),
                int.Parse(this.majorVersionTextBox.Text),
                int.Parse(this.minorVersionTextBox.Text));

            this.licenseKeyTextBox.Text = stockLicense.LicenseKey;
        }

        private void loadKeyButton_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "License files (*.dat)|*.dat";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string licenseKey = string.Empty;
                using (StreamReader sr = new StreamReader(fileDialog.FileName))
                {
                    while (!sr.EndOfStream)
                    {
                        licenseKey += sr.ReadLine();
                    }
                }
                this.licenseKeyTextBox.Text += licenseKey;
            }
            else
            {
                this.licenseKeyTextBox.Text = "";
            }
        }

        private void saveKeyButton_Click(object sender, EventArgs e)
        {
            if (licenseKeyTextBox.Text == string.Empty) return;
            FileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "License files (*.dat)|*.dat";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using(StreamWriter sw = new StreamWriter(fileDialog.FileName,false))
                {
                    sw.Write(this.licenseKeyTextBox.Text);
                }
            }
        }
        private void verifyKeyButton_Click(object sender, EventArgs e)
        {
            try
            {
                StockLicense stockLicense = new StockLicense(this.expiryDateTimePicker.Value,
                this.pseudoTextBox.Text,
                this.machineIDTextBox.Text,
                this.featureTextBox.Lines.ToList(),
                int.Parse(this.majorVersionTextBox.Text),
                int.Parse(this.minorVersionTextBox.Text));

                if (this.licenseKeyTextBox.Text == stockLicense.LicenseKey)
                {
                    MessageBox.Show("Congratulations your license key is valid !!!" + System.Environment.NewLine + System.Environment.NewLine +
                    "Expiry date:\t" + stockLicense.ExpiryDate + System.Environment.NewLine +
                    "Version:\t\t" + stockLicense.MajorVerion + "." + stockLicense.MinorVerion + System.Environment.NewLine +
                    "User ID:\t\t" + stockLicense.UserID + System.Environment.NewLine +
                    "Machine ID:\t" + stockLicense.MachineID, "Success");
                }
                else
                {
                    StockLicense otherLicense = new StockLicense(this.pseudoTextBox.Text, this.licenseKeyTextBox.Text);

                    if (otherLicense.UserID == stockLicense.UserID &&
                        otherLicense.ExpiryDate == stockLicense.ExpiryDate &&
                        otherLicense.MachineID == stockLicense.MachineID)
                    {
                        MessageBox.Show("Your algo is crap");
                    }
                    else
                    {
                        MessageBox.Show("Expiry date:\t" + otherLicense.ExpiryDate + System.Environment.NewLine +
                                                    "User ID:   \t" + otherLicense.UserID + System.Environment.NewLine +
                                                "Machine ID:\t" + otherLicense.MachineID, "License Key Error");
                    }
                }
            }
            catch
            {
                MessageBox.Show("Exception creating the license objet");
            }
        }
    }
}
