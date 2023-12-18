using System;
using System.Net.Mail;
using System.Net.NetworkInformation;

namespace StockAnalyzer.StockWeb
{
    public class StockMail
    {
        private static string SMTPServer => StockAnalyzerSettings.Properties.Settings.Default.UserSMTP;

        private static string To => StockAnalyzerSettings.Properties.Settings.Default.UserEMail;

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
                    catch (Exception)
                    {
                        //System.Windows.Forms.MessageBox.Show(exp.Message, "Email error !");
                    }
                }
            }
        }
    }
}
