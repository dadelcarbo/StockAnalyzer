namespace StockAnalyzerApp.CustomControl.WatchlistDlgs
{
   partial class WatchListDlg
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
         this.components = new System.ComponentModel.Container();
         this.watchListComboBox = new System.Windows.Forms.ComboBox();
         this.stockWatchListsBindingSource = new System.Windows.Forms.BindingSource(this.components);
         this.deleteWatchlistBtn = new System.Windows.Forms.Button();
         this.addWatchlistBtn = new System.Windows.Forms.Button();
         this.stockListBox = new System.Windows.Forms.ListBox();
         this.stockWatchListBindingSource = new System.Windows.Forms.BindingSource(this.components);
         this.deleteStockbtn = new System.Windows.Forms.Button();
         this.addStockbtn = new System.Windows.Forms.Button();
         this.okBtn = new System.Windows.Forms.Button();
         this.cancelBtn = new System.Windows.Forms.Button();
         ((System.ComponentModel.ISupportInitialize)(this.stockWatchListsBindingSource)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.stockWatchListBindingSource)).BeginInit();
         this.SuspendLayout();
         // 
         // watchListComboBox
         // 
         this.watchListComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
         | System.Windows.Forms.AnchorStyles.Right)));
         this.watchListComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
         this.watchListComboBox.DataSource = this.stockWatchListsBindingSource;
         this.watchListComboBox.DisplayMember = "Name";
         this.watchListComboBox.FormattingEnabled = true;
         this.watchListComboBox.Location = new System.Drawing.Point(12, 12);
         this.watchListComboBox.Name = "watchListComboBox";
         this.watchListComboBox.Size = new System.Drawing.Size(230, 21);
         this.watchListComboBox.TabIndex = 0;
         this.watchListComboBox.ValueMember = "Name";
         this.watchListComboBox.SelectedIndexChanged += new System.EventHandler(this.watchListComboBox_SelectedIndexChanged);
         // 
         // stockWatchListsBindingSource
         // 
         this.stockWatchListsBindingSource.DataSource = typeof(StockAnalyzer.StockClasses.StockWatchList);
         // 
         // deleteWatchlistBtn
         // 
         this.deleteWatchlistBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.deleteWatchlistBtn.AutoSize = true;
         this.deleteWatchlistBtn.Image = global::StockAnalyzerApp.Properties.Resources.Delete;
         this.deleteWatchlistBtn.Location = new System.Drawing.Point(248, 12);
         this.deleteWatchlistBtn.Name = "deleteWatchlistBtn";
         this.deleteWatchlistBtn.Size = new System.Drawing.Size(24, 24);
         this.deleteWatchlistBtn.TabIndex = 1;
         this.deleteWatchlistBtn.UseVisualStyleBackColor = true;
         this.deleteWatchlistBtn.Click += new System.EventHandler(this.deleteWatchlistBtn_Click);
         // 
         // addWatchlistBtn
         // 
         this.addWatchlistBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.addWatchlistBtn.Location = new System.Drawing.Point(278, 12);
         this.addWatchlistBtn.Name = "addWatchlistBtn";
         this.addWatchlistBtn.Size = new System.Drawing.Size(24, 24);
         this.addWatchlistBtn.TabIndex = 3;
         this.addWatchlistBtn.Text = "+";
         this.addWatchlistBtn.UseVisualStyleBackColor = true;
         this.addWatchlistBtn.Click += new System.EventHandler(this.addWatchlistBtn_Click);
         // 
         // stockListBox
         // 
         this.stockListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
         | System.Windows.Forms.AnchorStyles.Left)
         | System.Windows.Forms.AnchorStyles.Right)));
         this.stockListBox.DataSource = this.stockWatchListBindingSource;
         this.stockListBox.DisplayMember = "Name";
         this.stockListBox.FormattingEnabled = true;
         this.stockListBox.Location = new System.Drawing.Point(12, 42);
         this.stockListBox.Name = "stockListBox";
         this.stockListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
         this.stockListBox.Size = new System.Drawing.Size(229, 316);
         this.stockListBox.TabIndex = 4;
         this.stockListBox.ValueMember = "Name";
         this.stockListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.stockListBox_MouseDoubleClick);
         // 
         // stockWatchListBindingSource
         // 
         this.stockWatchListBindingSource.DataSource = typeof(StockAnalyzer.StockClasses.StockWatchList);
         // 
         // deleteStockbtn
         // 
         this.deleteStockbtn.Anchor = System.Windows.Forms.AnchorStyles.Right;
         this.deleteStockbtn.AutoSize = true;
         this.deleteStockbtn.Image = global::StockAnalyzerApp.Properties.Resources.Delete;
         this.deleteStockbtn.Location = new System.Drawing.Point(277, 113);
         this.deleteStockbtn.Name = "deleteStockbtn";
         this.deleteStockbtn.Size = new System.Drawing.Size(24, 24);
         this.deleteStockbtn.TabIndex = 1;
         this.deleteStockbtn.UseVisualStyleBackColor = true;
         this.deleteStockbtn.Click += new System.EventHandler(this.deleteStockbtn_Click);
         // 
         // addStockbtn
         // 
         this.addStockbtn.Anchor = System.Windows.Forms.AnchorStyles.Right;
         this.addStockbtn.Location = new System.Drawing.Point(277, 143);
         this.addStockbtn.Name = "addStockbtn";
         this.addStockbtn.Size = new System.Drawing.Size(24, 24);
         this.addStockbtn.TabIndex = 3;
         this.addStockbtn.Text = "+";
         this.addStockbtn.UseVisualStyleBackColor = true;
         this.addStockbtn.Click += new System.EventHandler(this.addStockbtn_Click);
         // 
         // okBtn
         // 
         this.okBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.okBtn.Location = new System.Drawing.Point(248, 309);
         this.okBtn.Name = "okBtn";
         this.okBtn.Size = new System.Drawing.Size(56, 23);
         this.okBtn.TabIndex = 1;
         this.okBtn.Text = "OK";
         this.okBtn.UseVisualStyleBackColor = true;
         // 
         // cancelBtn
         // 
         this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.cancelBtn.Location = new System.Drawing.Point(248, 338);
         this.cancelBtn.Name = "cancelBtn";
         this.cancelBtn.Size = new System.Drawing.Size(56, 23);
         this.cancelBtn.TabIndex = 2;
         this.cancelBtn.Text = "Cancel";
         this.cancelBtn.UseVisualStyleBackColor = true;
         // 
         // WatchListDlg
         // 
         this.AcceptButton = this.okBtn;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.cancelBtn;
         this.ClientSize = new System.Drawing.Size(312, 374);
         this.ControlBox = false;
         this.Controls.Add(this.cancelBtn);
         this.Controls.Add(this.okBtn);
         this.Controls.Add(this.stockListBox);
         this.Controls.Add(this.addStockbtn);
         this.Controls.Add(this.deleteStockbtn);
         this.Controls.Add(this.addWatchlistBtn);
         this.Controls.Add(this.deleteWatchlistBtn);
         this.Controls.Add(this.watchListComboBox);
         this.Name = "WatchListDlg";
         this.ShowInTaskbar = false;
         this.Text = "Watch List Management";
         ((System.ComponentModel.ISupportInitialize)(this.stockWatchListsBindingSource)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.stockWatchListBindingSource)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }
      #endregion

      private System.Windows.Forms.ComboBox watchListComboBox;
      private System.Windows.Forms.BindingSource stockWatchListsBindingSource;
      private System.Windows.Forms.Button deleteWatchlistBtn;
      private System.Windows.Forms.Button addWatchlistBtn;
      private System.Windows.Forms.ListBox stockListBox;
      private System.Windows.Forms.BindingSource stockWatchListBindingSource;
      private System.Windows.Forms.Button deleteStockbtn;
      private System.Windows.Forms.Button addStockbtn;
      private System.Windows.Forms.Button okBtn;
      private System.Windows.Forms.Button cancelBtn;
   }
}