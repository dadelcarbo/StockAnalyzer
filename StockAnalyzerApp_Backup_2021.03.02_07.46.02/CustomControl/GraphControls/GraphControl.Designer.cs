namespace StockAnalyzerApp.CustomControl.GraphControls
{
   partial class GraphControl
   {
      /// <summary> 
      /// Required designer variable.
      /// </summary>
      protected System.ComponentModel.IContainer components = null;

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

      #region Component Designer generated code

      /// <summary> 
      /// Required method for Designer support - do not modify 
      /// the contents of this method with the code editor.
      /// </summary>
      protected virtual void InitializeComponent()
      {
         this.SuspendLayout();
         // 
         // GraphControl
         // 
         this.Cursor = System.Windows.Forms.Cursors.Cross;
         this.Name = "GraphControl";
         this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
         this.MouseEnter += new System.EventHandler(this.Form1_MouseEnter);
         this.MouseLeave += new System.EventHandler(this.Form1_MouseLeave);
         this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
         this.ResumeLayout(false);
         this.PerformLayout();

      }
      #endregion
   }
}
