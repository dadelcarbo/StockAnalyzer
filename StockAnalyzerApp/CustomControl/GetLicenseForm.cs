using System;
using System.Windows.Forms;
using StockAnalyzerSettings.Properties;

namespace StockAnalyzerApp.CustomControl
{
    public partial class GetLicenseForm : Form
   {
      public GetLicenseForm()
      {
         InitializeComponent();
      }
      private void GetLicenseForm_Shown(object sender, EventArgs e)
      {
         this.userIdTextBox.Text = Settings.Default.UserId;
         if (userIdTextBox.Text == string.Empty)
         {
            this.userIdTextBox.Focus();
         }
         else
         {
            this.passwordTextBox.Focus();
         }
      }
      private void getLicenseButton_Click(object sender, EventArgs e)
      {
         if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
         {
            try
            {
               //StockWebServices.LicenseManagerSoapClient lm = new StockWebServices.LicenseManagerSoapClient();
               //StockUser user = new StockUser(userIdTextBox.Text, passwordTextBox.Text);

               //if (lm.IsRegisteredUser(user.Pseudo, user.EncryptedPassword))
               //{
               //   string licenseString = lm.GetLicense(user.Pseudo, user.EncryptedPassword, StockToolKit.GetMachineUID());
               //   if (licenseString.StartsWith("err"))
               //   {
               //      if (this.Controls.ContainsKey(licenseString))
               //      {
               //         Control errLabel = this.Controls.Find(licenseString, true)[0];
               //         MessageBox.Show(errLabel.Text, this.msg5.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
               //      }
               //   }
               //   else
               //   {
               //      // Succeeded, save file to local disk.
               //      string licenseFileName = Path.Combine( Folders.PersonalFolder , @"\license.dat");
               //      using (StreamWriter sr = new StreamWriter(licenseFileName, false))
               //      {
               //         sr.Write(licenseString);
               //      }


               //      Settings.Default.UserId = this.userIdTextBox.Text;
               //      Settings.Default.Save();

               //      // Congratulations
               //      MessageBox.Show(this.msg1.Text, this.msg6.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

               //      this.Close();
               //   }
               //}
               //else
               //{
               //   MessageBox.Show(this.msg2.Text, this.msg5.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
               //}

               throw new NotImplementedException("License server not implemented");
            }
            catch
            {
               MessageBox.Show(this.msg4.Text, this.msg3.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
         }
         else
         {
            MessageBox.Show(this.msg4.Text, this.msg3.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }
      private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         System.Diagnostics.Process.Start(Localisation.UltimateChartistStrings.WebSite);
      }
   }
}
