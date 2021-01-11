using System.Net.NetworkInformation;
using System.Net.Mail;

namespace StockAnalyzer.StockWeb
{
    public class StockMail
    {
        private static string SMTPServer
        {
            get { return StockAnalyzerSettings.Properties.Settings.Default.UserSMTP; }
        }

        private static string To
        {
            get { return StockAnalyzerSettings.Properties.Settings.Default.UserEMail; }
        }

        public static void SendEmail(string subject, string alert)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                using (MailMessage message = new MailMessage())
                {
                    try
                    {
                        message.Body = alert;
                        if (SMTPServer.Contains("free.fr"))
                        {
                            message.From = new MailAddress("noreply@free.fr");
                        }
                        else
                        {
                            return;
                            //message.From = new MailAddress("david.carbonel@volvo.com");
                        }
                        string[] toList = To.Split(';');
                        foreach (var address in toList)
                        {
                            message.To.Add(address);
                        }
                        message.Subject = subject;
                        message.IsBodyHtml = false;
                        SmtpClient smtp = new SmtpClient(SMTPServer);

                        //smtp.Send(message);
                    }
                    catch (System.Exception exp)
                    {
                        //System.Windows.Forms.MessageBox.Show(exp.Message, "Email error !");
                    }
                }
            }
        }
    }
}
