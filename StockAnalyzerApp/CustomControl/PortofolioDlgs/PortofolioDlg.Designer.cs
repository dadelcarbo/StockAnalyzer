namespace StockAnalyzerApp.CustomControl
{
   partial class PortofolioDlg
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      public float TotalAddedValue { get; set; }

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
         this.portofolioView = new System.Windows.Forms.ListView();
         this.nameHdr = new System.Windows.Forms.ColumnHeader();
         this.numberHdr = new System.Windows.Forms.ColumnHeader();
         this.buyValueHdr = new System.Windows.Forms.ColumnHeader();
         this.currentValueHdr = new System.Windows.Forms.ColumnHeader();
         this.totalVarHdr = new System.Windows.Forms.ColumnHeader();
         this.dailyVarHdr = new System.Windows.Forms.ColumnHeader();
         this.addedValurHdr = new System.Windows.Forms.ColumnHeader();
         this.totalValueHdr = new System.Windows.Forms.ColumnHeader();
         this.label1 = new System.Windows.Forms.Label();
         this.totalDepositTextBox = new System.Windows.Forms.TextBox();
         this.label2 = new System.Windows.Forms.Label();
         this.currentValueTextBox = new System.Windows.Forms.TextBox();
         this.label3 = new System.Windows.Forms.Label();
         this.addedValueTextBox = new System.Windows.Forms.TextBox();
         this.label4 = new System.Windows.Forms.Label();
         this.availableTextBox = new System.Windows.Forms.TextBox();
         this.SuspendLayout();
         // 
         // portofolioView
         // 
         this.portofolioView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.portofolioView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameHdr,
            this.numberHdr,
            this.buyValueHdr,
            this.currentValueHdr,
            this.totalVarHdr,
            this.dailyVarHdr,
            this.addedValurHdr,
            this.totalValueHdr});
         this.portofolioView.FullRowSelect = true;
         this.portofolioView.GridLines = true;
         this.portofolioView.Location = new System.Drawing.Point(12, 12);
         this.portofolioView.MultiSelect = false;
         this.portofolioView.Name = "portofolioView";
         this.portofolioView.Size = new System.Drawing.Size(727, 275);
         this.portofolioView.TabIndex = 0;
         this.portofolioView.UseCompatibleStateImageBehavior = false;
         this.portofolioView.View = System.Windows.Forms.View.Details;
         this.portofolioView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.portofolioView_MouseDoubleClick);
         // 
         // nameHdr
         // 
         this.nameHdr.Text = "Name";
         this.nameHdr.Width = 160;
         // 
         // numberHdr
         // 
         this.numberHdr.Text = "Number";
         // 
         // buyValueHdr
         // 
         this.buyValueHdr.Text = "Buy Unit Value";
         this.buyValueHdr.Width = 66;
         // 
         // currentValueHdr
         // 
         this.currentValueHdr.Text = "Current Unit Value";
         this.currentValueHdr.Width = 82;
         // 
         // totalVarHdr
         // 
         this.totalVarHdr.Text = "Total Variation";
         this.totalVarHdr.Width = 85;
         // 
         // dailyVarHdr
         // 
         this.dailyVarHdr.Text = "Daily Variation";
         this.dailyVarHdr.Width = 85;
         // 
         // addedValurHdr
         // 
         this.addedValurHdr.Text = "Added Value";
         this.addedValurHdr.Width = 76;
         // 
         // totalValueHdr
         // 
         this.totalValueHdr.Text = "Total Value";
         this.totalValueHdr.Width = 70;
         // 
         // label1
         // 
         this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 299);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(70, 13);
         this.label1.TabIndex = 1;
         this.label1.Text = "Total Deposit";
         // 
         // totalDepositTextBox
         // 
         this.totalDepositTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.totalDepositTextBox.Location = new System.Drawing.Point(88, 296);
         this.totalDepositTextBox.Name = "totalDepositTextBox";
         this.totalDepositTextBox.ReadOnly = true;
         this.totalDepositTextBox.Size = new System.Drawing.Size(73, 20);
         this.totalDepositTextBox.TabIndex = 2;
         // 
         // label2
         // 
         this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(167, 299);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(71, 13);
         this.label2.TabIndex = 1;
         this.label2.Text = "Current Value";
         // 
         // currentValueTextBox
         // 
         this.currentValueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.currentValueTextBox.Location = new System.Drawing.Point(241, 296);
         this.currentValueTextBox.Name = "currentValueTextBox";
         this.currentValueTextBox.ReadOnly = true;
         this.currentValueTextBox.Size = new System.Drawing.Size(65, 20);
         this.currentValueTextBox.TabIndex = 2;
         // 
         // label3
         // 
         this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(573, 299);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(95, 13);
         this.label3.TabIndex = 1;
         this.label3.Text = "Total Added Value";
         // 
         // addedValueTextBox
         // 
         this.addedValueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.addedValueTextBox.Location = new System.Drawing.Point(674, 296);
         this.addedValueTextBox.Name = "addedValueTextBox";
         this.addedValueTextBox.ReadOnly = true;
         this.addedValueTextBox.Size = new System.Drawing.Size(65, 20);
         this.addedValueTextBox.TabIndex = 2;
         // 
         // label4
         // 
         this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(312, 299);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(50, 13);
         this.label4.TabIndex = 1;
         this.label4.Text = "Available";
         // 
         // availableTextBox
         // 
         this.availableTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.availableTextBox.Location = new System.Drawing.Point(368, 296);
         this.availableTextBox.Name = "availableTextBox";
         this.availableTextBox.ReadOnly = true;
         this.availableTextBox.Size = new System.Drawing.Size(65, 20);
         this.availableTextBox.TabIndex = 2;
         // 
         // PortofolioDlg
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(751, 322);
         this.Controls.Add(this.addedValueTextBox);
         this.Controls.Add(this.availableTextBox);
         this.Controls.Add(this.currentValueTextBox);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.label4);
         this.Controls.Add(this.totalDepositTextBox);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.portofolioView);
         this.Name = "PortofolioDlg";
         this.Text = "Portofolio";
         this.ResumeLayout(false);
         this.PerformLayout();

      }
      #endregion

      private System.Windows.Forms.ListView portofolioView;
      private System.Windows.Forms.ColumnHeader nameHdr;
      private System.Windows.Forms.ColumnHeader numberHdr;
      private System.Windows.Forms.ColumnHeader buyValueHdr;
      private System.Windows.Forms.ColumnHeader currentValueHdr;
      private System.Windows.Forms.ColumnHeader totalVarHdr;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.TextBox totalDepositTextBox;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.TextBox currentValueTextBox;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.TextBox addedValueTextBox;
      private System.Windows.Forms.ColumnHeader totalValueHdr;
      private System.Windows.Forms.ColumnHeader dailyVarHdr;
      private System.Windows.Forms.ColumnHeader addedValurHdr;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.TextBox availableTextBox;
   }
}