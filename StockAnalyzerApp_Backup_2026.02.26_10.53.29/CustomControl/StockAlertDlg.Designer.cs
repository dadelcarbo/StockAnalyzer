namespace StockAnalyzerApp.CustomControl
{
   partial class StockAlertDlg
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
         this.alertListView = new System.Windows.Forms.ListView();
         this.stockNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.groupHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.alertHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.SuspendLayout();
         // 
         // alertListView
         // 
         this.alertListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.stockNameHeader,
            this.groupHeader,
            this.alertHeader});
         this.alertListView.Dock = System.Windows.Forms.DockStyle.Fill;
         this.alertListView.Location = new System.Drawing.Point(0, 0);
         this.alertListView.Name = "alertListView";
         this.alertListView.Size = new System.Drawing.Size(536, 187);
         this.alertListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
         this.alertListView.TabIndex = 0;
         this.alertListView.UseCompatibleStateImageBehavior = false;
         this.alertListView.View = System.Windows.Forms.View.Details;
         this.alertListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.alertListView_MouseDoubleClick);
         this.alertListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(alertListView_ColumnClick);
         // 
         // stockNameHeader
         // 
         this.stockNameHeader.Text = "Stock Name";
         this.stockNameHeader.Width = 146;
         // 
         // groupHeader
         // 
         this.groupHeader.Text = "Group";
         // 
         // alertHeader
         // 
         this.alertHeader.Text = "AlertName";
         this.alertHeader.Width = 242;
         // 
         // StockAlertDlg
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(536, 187);
         this.Controls.Add(this.alertListView);
         this.Name = "StockAlertDlg";
         this.Text = "Stock Alerts";
         this.ResumeLayout(false);

      }
      #endregion

      private System.Windows.Forms.ListView alertListView;
      private System.Windows.Forms.ColumnHeader stockNameHeader;
      private System.Windows.Forms.ColumnHeader alertHeader;
      private System.Windows.Forms.ColumnHeader groupHeader;
   }
}