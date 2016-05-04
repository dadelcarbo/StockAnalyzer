namespace StockAnalyzerApp.CustomControl.AlertDialog
{
   partial class AlertDlg
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
         this.alertControl1 = new AlertDialog.AlertControl();
         this.SuspendLayout();
         // 
         // elementHost1
         // 
         this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.elementHost1.Location = new System.Drawing.Point(0, 0);
         this.elementHost1.Name = "elementHost1";
         this.elementHost1.Size = new System.Drawing.Size(631, 418);
         this.elementHost1.TabIndex = 0;
         this.elementHost1.Text = "elementHost1";
         this.elementHost1.Child = this.alertControl1;
         // 
         // AlertDlg
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(631, 418);
         this.Controls.Add(this.elementHost1);
         this.Name = "AlertDlg";
         this.Text = "AlertDlg";
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Integration.ElementHost elementHost1;
      public AlertControl alertControl1 { get; set; }
   }
}