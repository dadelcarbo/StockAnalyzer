using System;
using System.Threading;
using System.Windows.Forms;

namespace StockAnalyzerApp
{
   public partial class StockSplashScreen : Form
   {
      static System.Windows.Forms.Timer timer;
      static public double FadeInOutSpeed = .1;

      public StockSplashScreen()
      {
         InitializeComponent();

         timer = new System.Windows.Forms.Timer();
         timer.Interval = 50;
         timer.Start();
         timer.Tick += new EventHandler(timer_Tick);
      }

      void timer_Tick(object sender, EventArgs e)
      {
         if (visible == false)
         {
            splashScreen.Visible = visible;
            return;
         }
         if (splashScreen.Visible == false && visible == true)
         {
            // We show it again
            this.Opacity = 0;
            this.Visible = true;
            this.TopLevel = true;
            this.Activate();
         }
         if (closing && splashScreen != null)
         {
            if (this.Opacity > 0)
               this.Opacity -= FadeInOutSpeed;
            else
            {
               visible = false;
            }
         }
         else
         {
            if (this.Opacity < 1)
               this.Opacity += FadeInOutSpeed;

            this.label.Text = ProgressText;
            this.label1.Text = ProgressSubText;
            this.progressBar1.Minimum = ProgressMin;
            this.progressBar1.Maximum = ProgressMax;
            this.progressBar1.Value = ProgressVal;
         }
      }
      static StockSplashScreen splashScreen = null;
      static Thread thread = null;

      static public string ProgressSubText { get; set; }
      static private string progressText;
      static public string ProgressText
      {
         get { return progressText; }
         set
         {
            progressText = value;
            ProgressSubText = "";
         }
      }

      static public int ProgressMin { get; set; }
      static public int ProgressMax { get; set; }
      static public int ProgressVal { get; set; }
      static private bool closing = false;

      static private bool visible = true;

      static public void ShowSplashScreen()
      {
         closing = false;
         visible = true;

         // Make sure it is only launched once.
         if (splashScreen != null)
            return;
         thread = new Thread(new ThreadStart(StockSplashScreen.ShowForm));
         thread.IsBackground = true;
         thread.SetApartmentState(ApartmentState.STA);
         thread.Start();
      }

      // A static entry point to launch SplashScreen.
      static private void ShowForm()
      {
         splashScreen = new StockSplashScreen();
         splashScreen.TopLevel = true;
         Application.Run(splashScreen);
      }

      // A static method to close the SplashScreen
      static public void CloseForm(bool withFadeOut)
      {
         if (withFadeOut)
         {
            closing = true;
         }
         else
         {
            visible = false;
         }
      }

      public static bool topLevel { get; set; }
   }
}
