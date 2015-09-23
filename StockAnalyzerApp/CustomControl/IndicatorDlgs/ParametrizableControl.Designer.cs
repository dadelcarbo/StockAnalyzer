namespace StockAnalyzerApp.CustomControl.IndicatorDlgs
{
    partial class ParametrizableControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.indicatorConfigBox = new System.Windows.Forms.GroupBox();
            this.paramListView = new System.Windows.Forms.ListView();
            this.valueColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.nameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.typeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.indicatorConfigBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // indicatorConfigBox
            // 
            this.indicatorConfigBox.Controls.Add(this.paramListView);
            this.indicatorConfigBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.indicatorConfigBox.Location = new System.Drawing.Point(0, 0);
            this.indicatorConfigBox.Name = "indicatorConfigBox";
            this.indicatorConfigBox.Size = new System.Drawing.Size(281, 181);
            this.indicatorConfigBox.TabIndex = 2;
            this.indicatorConfigBox.TabStop = false;
            this.indicatorConfigBox.Text = "Indicator Parameters";
            // 
            // paramListView
            // 
            this.paramListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.valueColumnHeader,
            this.nameColumnHeader,
            this.typeColumnHeader});
            this.paramListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.paramListView.FullRowSelect = true;
            this.paramListView.LabelEdit = true;
            this.paramListView.Location = new System.Drawing.Point(3, 16);
            this.paramListView.MultiSelect = false;
            this.paramListView.Name = "paramListView";
            this.paramListView.Scrollable = false;
            this.paramListView.Size = new System.Drawing.Size(275, 162);
            this.paramListView.TabIndex = 2;
            this.paramListView.UseCompatibleStateImageBehavior = false;
            this.paramListView.View = System.Windows.Forms.View.Details;
            this.paramListView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(paramListView_AfterLabelEdit);
            // 
            // valueColumnHeader
            // 
            this.valueColumnHeader.Text = "Value";
            this.valueColumnHeader.Width = 54;
            // 
            // nameColumnHeader
            // 
            this.nameColumnHeader.Text = "Name";
            this.nameColumnHeader.Width = 80;
            // 
            // typeColumnHeader
            // 
            this.typeColumnHeader.Text = "Type";
            this.typeColumnHeader.Width = 80;
            // 
            // ParametrizableControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.indicatorConfigBox);
            this.Name = "ParametrizableControl";
            this.Size = new System.Drawing.Size(281, 181);
            this.indicatorConfigBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox indicatorConfigBox;
        private System.Windows.Forms.ListView paramListView;
        private System.Windows.Forms.ColumnHeader valueColumnHeader;
        private System.Windows.Forms.ColumnHeader nameColumnHeader;
        private System.Windows.Forms.ColumnHeader typeColumnHeader;
    }
}
