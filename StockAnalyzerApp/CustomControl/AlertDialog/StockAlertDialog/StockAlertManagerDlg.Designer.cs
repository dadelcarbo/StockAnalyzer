﻿
namespace StockAnalyzerApp.CustomControl.AlertDialog.StockAlertDialog
{
    partial class StockAlertManagerDlg
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
        private void InitializeComponent(StockAlertManagerViewModel viewModel)
        {
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.stockAlertManagerCtrl = new CustomControl.AlertDialog.StockAlertDialog.StockAlertManagerControl(this, viewModel);
            this.SuspendLayout();
            // 
            // elementHost1
            // 
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(0, 0);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(1000, 550);
            this.elementHost1.TabIndex = 0;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = this.stockAlertManagerCtrl;
            // 
            // StockAlertManagerDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 550);
            this.Controls.Add(this.elementHost1);
            this.Name = "StockAlertManagerDlg";
            this.Text = "Alert Manager";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private CustomControl.AlertDialog.StockAlertDialog.StockAlertManagerControl stockAlertManagerCtrl;
    }
}