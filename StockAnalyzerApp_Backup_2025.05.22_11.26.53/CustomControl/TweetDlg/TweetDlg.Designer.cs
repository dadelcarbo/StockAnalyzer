using StockAnalyzerApp.CustomControl.GraphControls;
namespace StockAnalyzerApp.CustomControl.TweetDlg
{
    partial class TweetDlg
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
            this.SuspendLayout();
            // 
            // fullGraphUserControl
            // 
            this.fullGraphUserControl.CurrentStockSerie = null;
            this.fullGraphUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fullGraphUserControl.EndIndex = 0;
            this.fullGraphUserControl.Location = new System.Drawing.Point(3, 3);
            this.fullGraphUserControl.Name = "fullGraphUserControl";
            this.fullGraphUserControl.Size = new System.Drawing.Size(239, 422);
            this.fullGraphUserControl.StartIndex = 0;
            this.fullGraphUserControl.TabIndex = 0;
            // 
            // MultiTimeFrameChartDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(737, 461);
            this.Controls.Add(this.fullGraphUserControl);
            this.Name = "MultiTimeFrameChartDlg";
            this.Text = "Send Tweet";
            this.ResumeLayout(false);

        }
        #endregion

        private FullGraphUserControl fullGraphUserControl;

    }
}