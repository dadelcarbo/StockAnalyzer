namespace StockAnalyzerApp.CustomControl
{
    partial class OrderListDlg
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
            this.portofolioView = new System.Windows.Forms.ListView();
            this.nameHdr = new System.Windows.Forms.ColumnHeader();
            this.idHdr = new System.Windows.Forms.ColumnHeader();
            this.typeHdr = new System.Windows.Forms.ColumnHeader();
            this.stateHdr = new System.Windows.Forms.ColumnHeader();
            this.dateHdr = new System.Windows.Forms.ColumnHeader();
            this.numberHdr = new System.Windows.Forms.ColumnHeader();
            this.valueHdr = new System.Windows.Forms.ColumnHeader();
            this.feeHdr = new System.Windows.Forms.ColumnHeader();
            this.gapHdr = new System.Windows.Forms.ColumnHeader();
            this.unitCostHdr = new System.Windows.Forms.ColumnHeader();
            this.totalCostHdr = new System.Windows.Forms.ColumnHeader();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.purgeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shortHdr = new System.Windows.Forms.ColumnHeader();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // portofolioView
            // 
            this.portofolioView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameHdr,
            this.idHdr,
            this.typeHdr,
            this.shortHdr,
            this.stateHdr,
            this.dateHdr,
            this.numberHdr,
            this.valueHdr,
            this.feeHdr,
            this.gapHdr,
            this.unitCostHdr,
            this.totalCostHdr});
            this.portofolioView.ContextMenuStrip = this.contextMenuStrip1;
            this.portofolioView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.portofolioView.FullRowSelect = true;
            this.portofolioView.GridLines = true;
            this.portofolioView.Location = new System.Drawing.Point(0, 0);
            this.portofolioView.MultiSelect = false;
            this.portofolioView.Name = "portofolioView";
            this.portofolioView.Size = new System.Drawing.Size(880, 295);
            this.portofolioView.TabIndex = 0;
            this.portofolioView.UseCompatibleStateImageBehavior = false;
            this.portofolioView.View = System.Windows.Forms.View.Details;
            this.portofolioView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.portofolioView_MouseDoubleClick);
            // 
            // nameHdr
            // 
            this.nameHdr.Text = "Name";
            this.nameHdr.Width = 142;
            // 
            // idHdr
            // 
            this.idHdr.Text = "Id";
            this.idHdr.Width = 40;
            // 
            // typeHdr
            // 
            this.typeHdr.Text = "Type";
            this.typeHdr.Width = 92;
            // 
            // stateHdr
            // 
            this.stateHdr.Text = "UpDownState";
            this.stateHdr.Width = 71;
            // 
            // dateHdr
            // 
            this.dateHdr.Text = "Date";
            this.dateHdr.Width = 68;
            // 
            // numberHdr
            // 
            this.numberHdr.Text = "Number";
            // 
            // valueHdr
            // 
            this.valueHdr.Text = "Value";
            this.valueHdr.Width = 66;
            // 
            // feeHdr
            // 
            this.feeHdr.Text = "Fee";
            this.feeHdr.Width = 59;
            // 
            // gapHdr
            // 
            this.gapHdr.Text = "Gap";
            this.gapHdr.Width = 51;
            // 
            // unitCostHdr
            // 
            this.unitCostHdr.Text = "Unit Cost";
            this.unitCostHdr.Width = 72;
            // 
            // totalCostHdr
            // 
            this.totalCostHdr.Text = "Total Cost";
            this.totalCostHdr.Width = 71;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator1,
            this.purgeToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(162, 76);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(158, 6);
            // 
            // purgeToolStripMenuItem
            // 
            this.purgeToolStripMenuItem.Name = "purgeToolStripMenuItem";
            this.purgeToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.purgeToolStripMenuItem.Text = "Purge this stock";
            this.purgeToolStripMenuItem.Click += new System.EventHandler(this.purgeToolStripMenuItem_Click);
            // 
            // shortHdr
            // 
            this.shortHdr.Text = "Short";
            this.shortHdr.Width = 45;
            // 
            // OrderListDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(880, 295);
            this.Controls.Add(this.portofolioView);
            this.Name = "OrderListDlg";
            this.Text = "Order History";
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView portofolioView;
        private System.Windows.Forms.ColumnHeader nameHdr;
        private System.Windows.Forms.ColumnHeader numberHdr;
        private System.Windows.Forms.ColumnHeader valueHdr;
        private System.Windows.Forms.ColumnHeader feeHdr;
        private System.Windows.Forms.ColumnHeader unitCostHdr;
        private System.Windows.Forms.ColumnHeader typeHdr;
        private System.Windows.Forms.ColumnHeader totalCostHdr;
        private System.Windows.Forms.ColumnHeader dateHdr;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem purgeToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader idHdr;
        private System.Windows.Forms.ColumnHeader stateHdr;
        private System.Windows.Forms.ColumnHeader gapHdr;
        private System.Windows.Forms.ColumnHeader shortHdr;
    }
}