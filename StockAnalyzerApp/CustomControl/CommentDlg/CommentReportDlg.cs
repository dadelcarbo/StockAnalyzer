using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Mail;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;
using StockAnalyzerApp.CustomControl.GraphControls;

namespace StockAnalyzerApp.CustomControl.CommentDlg
{
   public partial class CommentReportDlg : Form
   {
      private StockSerie.Groups stockGroup;
      static private string commentTitleTemplate = "COMMENT_TITLE_TEMPLATE";
      static private string commentTemplate = "COMMENT_TEMPLATE";
      static private string imageFileCID = "IMAGE_FILE_LINK";
      static private string htmlCommentTemplate = "<P STYLE=\"margin-bottom: 0cm\"><B><U>" + commentTitleTemplate + "</U></B></P>" +
"<P STYLE=\"margin-bottom: 0cm; text-decoration: none\">" + commentTemplate + "</P>" +
"<P STYLE=\"margin-bottom: 0cm; text-decoration: none\"><IMG SRC=\"cid:" + imageFileCID + "\" ALIGN=LEFT BORDER=1><BR CLEAR=LEFT><BR></P>";

      private GraphCloseControl graphCloseControl;
      private StockDictionary stockDictionary;
      public CommentReportDlg(StockDictionary stockDictionary, GraphCloseControl graphCloseControl, StockSerie.Groups group)
      {
         InitializeComponent();
         this.stockDictionary = stockDictionary;
         this.graphCloseControl = graphCloseControl;
         this.stockGroup = group;
      }

      private void CommentReportDlg_Load(object sender, EventArgs e)
      {
         this.startDateTimePicker.Value = DateTime.Today.AddDays(-5);
         this.endDateTimePicker.Value = DateTime.Today.AddDays(1);

         this.commentReportTextBox.Text = GenerateReport(false, null);
      }

      private void SendMail()
      {
         CleanImageFolder();

         using (System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage())
         {
            string report = GenerateReport(true, message);

            message.To.Add("david.carbonel@free.fr");
            message.To.Add("david.carbonel@volvo.com");
            message.Subject = "Stock Analysis - " + this.stockGroup + " - " + DateTime.Now.ToString();
            message.From = new System.Net.Mail.MailAddress("david.carbonel@free.fr");
            message.IsBodyHtml = true;
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.free.fr", 25);
            try
            {
               smtp.Send(message);
               System.Windows.Forms.MessageBox.Show("Email sent successfully");
            }
            catch (System.Exception e)
            {
               System.Windows.Forms.MessageBox.Show(e.Message, "Email error !");
            }
         }
      }
      private string GenerateReport(bool htmlFormat, System.Net.Mail.MailMessage email)
      {
         string report = string.Empty;
         DateTime fromDate = this.startDateTimePicker.Value;
         DateTime endDate = this.endDateTimePicker.Value;
         string commentTitle = string.Empty;
         string commentBody = string.Empty;
         int imageCount = 0;
         ImageFormat imageFormat = ImageFormat.Gif;
         List<string> cidList = new List<string>();
         List<string> fileNameList = new List<string>();
         foreach (StockSerie serie in this.stockDictionary.Values.Where(s => s.StockAnalysis.Comments.Count > 0))
         {
            if (/*serie.BelongsToGroup(this.stockGroup) && */serie.StockAnalysis.Comments.Count > 0)
            {
               foreach (DateTime date in serie.StockAnalysis.Comments.Keys.Where(d => d >= fromDate && d <= endDate))
               {
                  string comment = serie.StockAnalysis.Comments[date];
                  if (htmlFormat)
                  {
                     // Generate bitmap
                     serie.Initialise();

                     Bitmap bitmap = this.graphCloseControl.GetSnapshot();
                     string fileName = GetFileName(ref endDate, serie, imageFormat.ToString().ToLower());
                     if (!System.IO.File.Exists(fileName))
                     {
                        bitmap.Save(fileName, imageFormat);
                        fileNameList.Add(fileName);

                        // Get image CID
                        string cid = "Image_" + imageCount++;
                        cidList.Add(cid);

                        commentTitle = "\r\n" + serie.StockName + ": " + date.ToShortDateString() + "\r\n";
                        commentBody = comment;

                        // Build report from html template
                        report += htmlCommentTemplate.Replace(CommentReportDlg.commentTitleTemplate, commentTitle).Replace(CommentReportDlg.commentTemplate, commentBody)
                            .Replace(CommentReportDlg.imageFileCID, cid);
                     }
                  }
                  else
                  {
                     report += "\r\n" + serie.StockName + ": " + comment + "\r\n";
                  }
               }
            }
         }
         if (htmlFormat)
         {
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(report, null, "text/html");
            int i = 0;
            foreach (string cid in cidList)
            {
               LinkedResource imagelink = new LinkedResource(fileNameList[i++], "image/" + imageFormat.ToString());
               imagelink.ContentId = cid;
               imagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
               htmlView.LinkedResources.Add(imagelink);
            }
            email.AlternateViews.Add(htmlView);
         }
         return report;
      }

      private static string GetFileName(ref DateTime readDate, StockSerie serie, string extension)
      {
         string directoryName = StockAnalyzerSettings.Properties.Settings.Default.RootFolder + @"\CommentReport\";
         if (!System.IO.Directory.Exists(directoryName))
         {
            System.IO.Directory.CreateDirectory(directoryName);
         }
         directoryName += readDate.ToString("dd_MM_yyyy");
         if (!System.IO.Directory.Exists(directoryName))
         {
            System.IO.Directory.CreateDirectory(directoryName);
         }
         string fileName = directoryName + @"\" + serie.StockName + "." + extension;
         return fileName;
      }
      private static void CleanImageFolder()
      {
         string directoryName = StockAnalyzerSettings.Properties.Settings.Default.RootFolder + @"\CommentReport\";
         if (System.IO.Directory.Exists(directoryName))
         {
            System.IO.Directory.Delete(directoryName, true);
         }
      }

      private void startDateTimePicker_ValueChanged(object sender, EventArgs e)
      {
         this.commentReportTextBox.Text = GenerateReport(false, null);
      }

      private void endDateTimePicker_ValueChanged(object sender, EventArgs e)
      {
         this.commentReportTextBox.Text = GenerateReport(false, null);
      }

      private void button2_Click(object sender, EventArgs e)
      {
         SendMail();
      }

      private void button1_Click(object sender, EventArgs e)
      {
         this.Close();
      }
   }
}
