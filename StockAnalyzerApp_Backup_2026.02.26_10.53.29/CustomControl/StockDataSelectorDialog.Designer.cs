namespace StockAnalyzerApp.CustomControl
{
    partial class StockDataSelectorDialog
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
            this.OKBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.dataSelectionList = new CustomControl.ListViewEx();
            this.dataTypeHdr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.thicknessHdr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colorHdr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.previewHdr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // OKBtn
            // 
            this.OKBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKBtn.Location = new System.Drawing.Point(231, 324);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 0;
            this.OKBtn.Text = "OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(312, 324);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(77, 23);
            this.cancelBtn.TabIndex = 1;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // dataSelectionList
            // 
            this.dataSelectionList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataSelectionList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.dataTypeHdr,
            this.thicknessHdr,
            this.colorHdr,
            this.previewHdr});
            this.dataSelectionList.FullRowSelect = true;
            this.dataSelectionList.GridLines = true;
            this.dataSelectionList.Location = new System.Drawing.Point(12, 12);
            this.dataSelectionList.Name = "dataSelectionList";
            this.dataSelectionList.OwnerDraw = true;
            this.dataSelectionList.Size = new System.Drawing.Size(377, 305);
            this.dataSelectionList.TabIndex = 2;
            this.dataSelectionList.UseCompatibleStateImageBehavior = false;
            this.dataSelectionList.View = System.Windows.Forms.View.Details;
            this.dataSelectionList.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.dataSelectionList_DrawColumnHeader);
            this.dataSelectionList.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.dataSelectionList_DrawSubItem);
            this.dataSelectionList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dataSelectionList_MouseClick);
            // 
            // dataTypeHdr
            // 
            this.dataTypeHdr.Text = "DataType";
            this.dataTypeHdr.Width = 92;
            // 
            // thicknessHdr
            // 
            this.thicknessHdr.Text = "Thickness";
            this.thicknessHdr.Width = 80;
            // 
            // colorHdr
            // 
            this.colorHdr.Text = "Color";
            this.colorHdr.Width = 80;
            // 
            // previewHdr
            // 
            this.previewHdr.Text = "Preview";
            // 
            // StockDataSelectorDialog
            // 
            this.AcceptButton = this.OKBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(401, 362);
            this.Controls.Add(this.dataSelectionList);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.OKBtn);
            this.Name = "StockDataSelectorDialog";
            this.Text = "Select data to display in main graph";
            this.Load += new System.EventHandler(this.StockDataSelectorDialog_Load);
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private ListViewEx dataSelectionList;
        private System.Windows.Forms.ColumnHeader dataTypeHdr;
        private System.Windows.Forms.ColumnHeader thicknessHdr;
        private System.Windows.Forms.ColumnHeader colorHdr;
        private System.Windows.Forms.ColumnHeader previewHdr;

    }
}