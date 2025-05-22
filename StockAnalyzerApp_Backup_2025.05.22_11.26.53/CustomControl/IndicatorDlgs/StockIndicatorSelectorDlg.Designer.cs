using System.Windows.Forms;
namespace StockAnalyzerApp.CustomControl.IndicatorDlgs
{
    partial class StockIndicatorSelectorDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StockIndicatorSelectorDlg));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.indicatorConfigBox = new System.Windows.Forms.GroupBox();
            this.paramListView = new System.Windows.Forms.ListView();
            this.valueColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.nameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.minColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.maxColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.typeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.graphConfigBox = new System.Windows.Forms.GroupBox();
            this.secondarySerieGroupBox = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.secondaryThicknessComboBox = new System.Windows.Forms.ComboBox();
            this.secondaryColorPanel = new System.Windows.Forms.Panel();
            this.gridColorPanel = new System.Windows.Forms.Panel();
            this.textBackgroundColorPanel = new System.Windows.Forms.Panel();
            this.backgroundColorPanel = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.previewLabel = new System.Windows.Forms.Label();
            this.graphPreviewPanel = new System.Windows.Forms.Panel();
            this.showGridCheckBox = new System.Windows.Forms.CheckBox();
            this.chartModeComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.applyToAllButton = new System.Windows.Forms.Button();
            this.lineConfigBox = new System.Windows.Forms.GroupBox();
            this.lineColorPanel = new System.Windows.Forms.Panel();
            this.lineValueTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.opacityTrackBar = new System.Windows.Forms.TrackBar();
            this.thicknessComboBox = new System.Windows.Forms.ComboBox();
            this.lineTypeComboBox = new System.Windows.Forms.ComboBox();
            this.curveConfigBox = new System.Windows.Forms.GroupBox();
            this.visibleCheckBox = new System.Windows.Forms.CheckBox();
            this.curvePreviewLabel = new System.Windows.Forms.Label();
            this.previewPanel = new System.Windows.Forms.Panel();
            this.paintBarGroupBox = new System.Windows.Forms.GroupBox();
            this.trailStopGroupBox = new System.Windows.Forms.GroupBox();
            this.graphMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.indicatorMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addIndicatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addCloudToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addHorizontalLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addPaintBarsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addAutoDrawingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addTrailStopsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addDecoratorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addTrailToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.someTextLabel = new System.Windows.Forms.Label();
            this.copyStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.indicatorConfigBox.SuspendLayout();
            this.graphConfigBox.SuspendLayout();
            this.secondarySerieGroupBox.SuspendLayout();
            this.lineConfigBox.SuspendLayout();
            this.curveConfigBox.SuspendLayout();
            this.graphMenuStrip.SuspendLayout();
            this.indicatorMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.HideSelection = false;
            resources.ApplyResources(this.treeView1, "treeView1");
            this.treeView1.Name = "treeView1";
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            this.treeView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView1_KeyDown);
            // 
            // indicatorConfigBox
            // 
            this.indicatorConfigBox.Controls.Add(this.paramListView);
            resources.ApplyResources(this.indicatorConfigBox, "indicatorConfigBox");
            this.indicatorConfigBox.Name = "indicatorConfigBox";
            this.indicatorConfigBox.TabStop = false;
            // 
            // paramListView
            // 
            this.paramListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.valueColumnHeader,
            this.nameColumnHeader,
            this.minColumnHeader,
            this.maxColumnHeader,
            this.typeColumnHeader});
            this.paramListView.FullRowSelect = true;
            this.paramListView.LabelEdit = true;
            resources.ApplyResources(this.paramListView, "paramListView");
            this.paramListView.MultiSelect = false;
            this.paramListView.Name = "paramListView";
            this.paramListView.UseCompatibleStateImageBehavior = false;
            this.paramListView.View = System.Windows.Forms.View.Details;
            this.paramListView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.paramListView_AfterLabelEdit);
            this.paramListView.Click += new System.EventHandler(this.paramListView_Click);
            // 
            // valueColumnHeader
            // 
            resources.ApplyResources(this.valueColumnHeader, "valueColumnHeader");
            // 
            // nameColumnHeader
            // 
            resources.ApplyResources(this.nameColumnHeader, "nameColumnHeader");
            // 
            // minColumnHeader
            // 
            resources.ApplyResources(this.minColumnHeader, "minColumnHeader");
            // 
            // maxColumnHeader
            // 
            resources.ApplyResources(this.maxColumnHeader, "maxColumnHeader");
            // 
            // typeColumnHeader
            // 
            resources.ApplyResources(this.typeColumnHeader, "typeColumnHeader");
            // 
            // graphConfigBox
            // 
            this.graphConfigBox.Controls.Add(this.secondarySerieGroupBox);
            this.graphConfigBox.Controls.Add(this.gridColorPanel);
            this.graphConfigBox.Controls.Add(this.textBackgroundColorPanel);
            this.graphConfigBox.Controls.Add(this.backgroundColorPanel);
            this.graphConfigBox.Controls.Add(this.label8);
            this.graphConfigBox.Controls.Add(this.previewLabel);
            this.graphConfigBox.Controls.Add(this.graphPreviewPanel);
            this.graphConfigBox.Controls.Add(this.showGridCheckBox);
            this.graphConfigBox.Controls.Add(this.chartModeComboBox);
            this.graphConfigBox.Controls.Add(this.label6);
            this.graphConfigBox.Controls.Add(this.label5);
            resources.ApplyResources(this.graphConfigBox, "graphConfigBox");
            this.graphConfigBox.Name = "graphConfigBox";
            this.graphConfigBox.TabStop = false;
            // 
            // secondarySerieGroupBox
            // 
            this.secondarySerieGroupBox.Controls.Add(this.label11);
            this.secondarySerieGroupBox.Controls.Add(this.label10);
            this.secondarySerieGroupBox.Controls.Add(this.secondaryThicknessComboBox);
            this.secondarySerieGroupBox.Controls.Add(this.secondaryColorPanel);
            resources.ApplyResources(this.secondarySerieGroupBox, "secondarySerieGroupBox");
            this.secondarySerieGroupBox.Name = "secondarySerieGroupBox";
            this.secondarySerieGroupBox.TabStop = false;
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // secondaryThicknessComboBox
            // 
            this.secondaryThicknessComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.secondaryThicknessComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.secondaryThicknessComboBox, "secondaryThicknessComboBox");
            this.secondaryThicknessComboBox.FormattingEnabled = true;
            this.secondaryThicknessComboBox.Name = "secondaryThicknessComboBox";
            this.secondaryThicknessComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.thicknessComboBox_DrawItem);
            this.secondaryThicknessComboBox.SelectedIndexChanged += new System.EventHandler(this.secondaryThicknessComboBox_SelectedIndexChanged);
            // 
            // secondaryColorPanel
            // 
            this.secondaryColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.secondaryColorPanel, "secondaryColorPanel");
            this.secondaryColorPanel.Name = "secondaryColorPanel";
            this.secondaryColorPanel.Click += new System.EventHandler(this.secondaryColorPanel_Click);
            // 
            // gridColorPanel
            // 
            this.gridColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.gridColorPanel, "gridColorPanel");
            this.gridColorPanel.Name = "gridColorPanel";
            this.gridColorPanel.Click += new System.EventHandler(this.gridColorPanel_Click);
            // 
            // textBackgroundColorPanel
            // 
            this.textBackgroundColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.textBackgroundColorPanel, "textBackgroundColorPanel");
            this.textBackgroundColorPanel.Name = "textBackgroundColorPanel";
            this.textBackgroundColorPanel.Click += new System.EventHandler(this.textBackgroundColorPanel_Click);
            // 
            // backgroundColorPanel
            // 
            this.backgroundColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.backgroundColorPanel, "backgroundColorPanel");
            this.backgroundColorPanel.Name = "backgroundColorPanel";
            this.backgroundColorPanel.Click += new System.EventHandler(this.backgroundColorPanel_Click);
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // previewLabel
            // 
            resources.ApplyResources(this.previewLabel, "previewLabel");
            this.previewLabel.Name = "previewLabel";
            // 
            // graphPreviewPanel
            // 
            resources.ApplyResources(this.graphPreviewPanel, "graphPreviewPanel");
            this.graphPreviewPanel.Name = "graphPreviewPanel";
            this.graphPreviewPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.graphPreviewPanel_Paint);
            // 
            // showGridCheckBox
            // 
            resources.ApplyResources(this.showGridCheckBox, "showGridCheckBox");
            this.showGridCheckBox.Name = "showGridCheckBox";
            this.showGridCheckBox.UseVisualStyleBackColor = true;
            this.showGridCheckBox.CheckedChanged += new System.EventHandler(this.showGridCheckBox_CheckedChanged);
            // 
            // chartModeComboBox
            // 
            this.chartModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.chartModeComboBox, "chartModeComboBox");
            this.chartModeComboBox.FormattingEnabled = true;
            this.chartModeComboBox.Name = "chartModeComboBox";
            this.chartModeComboBox.SelectedIndexChanged += new System.EventHandler(this.chartModeComboBox_SelectedIndexChanged);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // applyToAllButton
            // 
            resources.ApplyResources(this.applyToAllButton, "applyToAllButton");
            this.applyToAllButton.Name = "applyToAllButton";
            this.applyToAllButton.UseVisualStyleBackColor = true;
            this.applyToAllButton.Click += new System.EventHandler(this.applyToAllButton_Click);
            // 
            // lineConfigBox
            // 
            this.lineConfigBox.Controls.Add(this.lineColorPanel);
            this.lineConfigBox.Controls.Add(this.lineValueTextBox);
            this.lineConfigBox.Controls.Add(this.label9);
            this.lineConfigBox.Controls.Add(this.label3);
            this.lineConfigBox.Controls.Add(this.label2);
            this.lineConfigBox.Controls.Add(this.label1);
            this.lineConfigBox.Controls.Add(this.thicknessComboBox);
            this.lineConfigBox.Controls.Add(this.lineTypeComboBox);
            resources.ApplyResources(this.lineConfigBox, "lineConfigBox");
            this.lineConfigBox.Name = "lineConfigBox";
            this.lineConfigBox.TabStop = false;
            this.lineConfigBox.Validating += new System.ComponentModel.CancelEventHandler(this.lineConfigBox_Validating);
            // 
            // lineColorPanel
            // 
            this.lineColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.lineColorPanel, "lineColorPanel");
            this.lineColorPanel.Name = "lineColorPanel";
            this.lineColorPanel.Click += new System.EventHandler(this.lineColorPanel_Click);
            // 
            // lineValueTextBox
            // 
            resources.ApplyResources(this.lineValueTextBox, "lineValueTextBox");
            this.lineValueTextBox.Name = "lineValueTextBox";
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // thicknessComboBox
            // 
            this.thicknessComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.thicknessComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.thicknessComboBox, "thicknessComboBox");
            this.thicknessComboBox.FormattingEnabled = true;
            this.thicknessComboBox.Name = "thicknessComboBox";
            this.thicknessComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.thicknessComboBox_DrawItem);
            this.thicknessComboBox.SelectedIndexChanged += new System.EventHandler(this.thicknessComboBox_SelectedIndexChanged);
            // 
            // opacityTrackBar
            // 
            resources.ApplyResources(this.opacityTrackBar, "opacityTrackBar");
            this.opacityTrackBar.Minimum = 0;
            this.opacityTrackBar.Maximum = 255;
            this.opacityTrackBar.TickFrequency = 25;
            this.opacityTrackBar.Name = "opacityTrackBar";
            this.opacityTrackBar.ValueChanged += new System.EventHandler(this.opacityTrackBar_ValueChanged);
            // 
            // lineTypeComboBox
            // 
            this.lineTypeComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lineTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.lineTypeComboBox, "lineTypeComboBox");
            this.lineTypeComboBox.FormattingEnabled = true;
            this.lineTypeComboBox.Name = "lineTypeComboBox";
            this.lineTypeComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lineTypeComboBox_DrawItem);
            this.lineTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.lineTypeComboBox_SelectedIndexChanged);
            // 
            // curveConfigBox
            // 
            this.curveConfigBox.Controls.Add(this.visibleCheckBox);
            this.curveConfigBox.Controls.Add(this.curvePreviewLabel);
            this.curveConfigBox.Controls.Add(this.previewPanel);
            this.curveConfigBox.Controls.Add(this.opacityTrackBar);
            resources.ApplyResources(this.curveConfigBox, "curveConfigBox");
            this.curveConfigBox.Name = "curveConfigBox";
            this.curveConfigBox.TabStop = false;
            // 
            // visibleCheckBox
            // 
            resources.ApplyResources(this.visibleCheckBox, "visibleCheckBox");
            this.visibleCheckBox.Name = "visibleCheckBox";
            this.visibleCheckBox.UseVisualStyleBackColor = true;
            this.visibleCheckBox.CheckedChanged += new System.EventHandler(this.visibleCheckBox_CheckedChanged);
            // 
            // curvePreviewLabel
            // 
            resources.ApplyResources(this.curvePreviewLabel, "curvePreviewLabel");
            this.curvePreviewLabel.Name = "curvePreviewLabel";
            // 
            // previewPanel
            // 
            resources.ApplyResources(this.previewPanel, "previewPanel");
            this.previewPanel.Name = "previewPanel";
            this.previewPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.previewPanel_Paint);
            // 
            // paintBarGroupBox
            // 
            resources.ApplyResources(this.paintBarGroupBox, "paintBarGroupBox");
            this.paintBarGroupBox.Name = "paintBarGroupBox";
            this.paintBarGroupBox.TabStop = false;
            // 
            // trailStopGroupBox
            // 
            resources.ApplyResources(this.trailStopGroupBox, "trailStopGroupBox");
            this.trailStopGroupBox.Name = "trailStopGroupBox";
            this.trailStopGroupBox.TabStop = false;
            // 
            // graphMenuStrip
            // 
            this.graphMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addIndicatorToolStripMenuItem,
            this.addCloudToolStripMenuItem,
            this.addHorizontalLineToolStripMenuItem,
            this.addPaintBarsToolStripMenuItem,
            this.addTrailStopsToolStripMenuItem,
            addAutoDrawingsToolStripMenuItem});
            this.graphMenuStrip.Name = "graphMenuStrip";
            resources.ApplyResources(this.graphMenuStrip, "graphMenuStrip");
            // 
            // addIndicatorToolStripMenuItem
            // 
            this.addIndicatorToolStripMenuItem.Name = "addIndicatorToolStripMenuItem";
            resources.ApplyResources(this.addIndicatorToolStripMenuItem, "addIndicatorToolStripMenuItem");
            this.addIndicatorToolStripMenuItem.Click += new System.EventHandler(this.addIndicatorToolStripMenuItem_Click);
            // 
            // addCloudToolStripMenuItem
            // 
            this.addCloudToolStripMenuItem.Name = "addCloudToolStripMenuItem";
            resources.ApplyResources(this.addCloudToolStripMenuItem, "addCloudToolStripMenuItem");
            this.addCloudToolStripMenuItem.Click += new System.EventHandler(this.addCloudToolStripMenuItem_Click);
            // 
            // addHorizontalLineToolStripMenuItem
            // 
            this.addHorizontalLineToolStripMenuItem.Name = "addHorizontalLineToolStripMenuItem";
            resources.ApplyResources(this.addHorizontalLineToolStripMenuItem, "addHorizontalLineToolStripMenuItem");
            this.addHorizontalLineToolStripMenuItem.Click += new System.EventHandler(this.addHorizontalLineToolStripMenuItem_Click);
            // 
            // addPaintBarsToolStripMenuItem
            // 
            this.addPaintBarsToolStripMenuItem.Name = "addPaintBarsToolStripMenuItem";
            resources.ApplyResources(this.addPaintBarsToolStripMenuItem, "addPaintBarsToolStripMenuItem");
            this.addPaintBarsToolStripMenuItem.Click += new System.EventHandler(this.addPaintBarsToolStripMenuItem_Click);
            // 
            // addAutoDrawingsToolStripMenuItem
            // 
            this.addAutoDrawingsToolStripMenuItem.Name = "addAutoDrawingsToolStripMenuItem";
            resources.ApplyResources(this.addAutoDrawingsToolStripMenuItem, "addAutoDrawingsToolStripMenuItem");
            this.addAutoDrawingsToolStripMenuItem.Click += new System.EventHandler(this.addAutoDrawingsToolStripMenuItem_Click);
            // 
            // addTrailStopsToolStripMenuItem
            // 
            this.addTrailStopsToolStripMenuItem.Name = "addTrailStopsToolStripMenuItem";
            resources.ApplyResources(this.addTrailStopsToolStripMenuItem, "addTrailStopsToolStripMenuItem");
            this.addTrailStopsToolStripMenuItem.Click += new System.EventHandler(this.addTrailStopsToolStripMenuItem_Click);
            // 
            // addDecoratorToolStripMenuItem
            // 
            this.addDecoratorToolStripMenuItem.Name = "addDecoratorToolStripMenuItem";
            resources.ApplyResources(this.addDecoratorToolStripMenuItem, "addDecoratorToolStripMenuItem");
            this.addDecoratorToolStripMenuItem.Click += new System.EventHandler(this.addDecoratorToolStripMenuItem_Click);
            // 
            // addTrailToolStripMenuItem
            // 
            this.addTrailToolStripMenuItem.Name = "addTrailToolStripMenuItem";
            resources.ApplyResources(this.addTrailToolStripMenuItem, "addTrailToolStripMenuItem");
            this.addTrailToolStripMenuItem.Click += new System.EventHandler(this.addTrailToolStripMenuItem_Click);
            // 
            // indicatorMenuStrip
            // 
            this.indicatorMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeStripMenuItem,
            this.addDecoratorToolStripMenuItem,
            this.addTrailToolStripMenuItem,
            this.copyStripMenuItem});
            this.indicatorMenuStrip.Name = "indicatorMenuStrip";
            resources.ApplyResources(this.indicatorMenuStrip, "indicatorMenuStrip");
            // 
            // removeStripMenuItem
            // 
            this.removeStripMenuItem.Name = "removeStripMenuItem";
            resources.ApplyResources(this.removeStripMenuItem, "removeStripMenuItem");
            this.removeStripMenuItem.Click += new System.EventHandler(this.removeStripMenuItem_Click);
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // applyButton
            // 
            resources.ApplyResources(this.applyButton, "applyButton");
            this.applyButton.Name = "applyButton";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // someTextLabel
            // 
            resources.ApplyResources(this.someTextLabel, "someTextLabel");
            this.someTextLabel.Name = "someTextLabel";
            // 
            // copyStripMenuItem
            // 
            this.copyStripMenuItem.Name = "copyStripMenuItem";
            resources.ApplyResources(this.copyStripMenuItem, "copyStripMenuItem");
            this.copyStripMenuItem.Click += new System.EventHandler(this.copyStripMenuItem_Click);
            // 
            // StockIndicatorSelectorDlg
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.graphConfigBox);
            this.Controls.Add(this.curveConfigBox);
            this.Controls.Add(this.someTextLabel);
            this.Controls.Add(this.applyToAllButton);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.trailStopGroupBox);
            this.Controls.Add(this.lineConfigBox);
            this.Controls.Add(this.indicatorConfigBox);
            this.Controls.Add(this.paintBarGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StockIndicatorSelectorDlg";
            this.Load += new System.EventHandler(this.StockIndicatorSelectorDlg_Load);
            this.indicatorConfigBox.ResumeLayout(false);
            this.graphConfigBox.ResumeLayout(false);
            this.graphConfigBox.PerformLayout();
            this.secondarySerieGroupBox.ResumeLayout(false);
            this.secondarySerieGroupBox.PerformLayout();
            this.lineConfigBox.ResumeLayout(false);
            this.lineConfigBox.PerformLayout();
            this.curveConfigBox.ResumeLayout(false);
            this.curveConfigBox.PerformLayout();
            this.graphMenuStrip.ResumeLayout(false);
            this.indicatorMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.GroupBox graphConfigBox;
        private System.Windows.Forms.GroupBox indicatorConfigBox;
        private System.Windows.Forms.GroupBox curveConfigBox;
        private System.Windows.Forms.GroupBox lineConfigBox;
        private System.Windows.Forms.GroupBox paintBarGroupBox;
        private System.Windows.Forms.GroupBox trailStopGroupBox;
        private System.Windows.Forms.ContextMenuStrip graphMenuStrip;
        private System.Windows.Forms.ContextMenuStrip indicatorMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem addIndicatorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addCloudToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addDecoratorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addTrailToolStripMenuItem;
        private System.Windows.Forms.TrackBar opacityTrackBar;
        private System.Windows.Forms.ComboBox thicknessComboBox;
        private System.Windows.Forms.ComboBox lineTypeComboBox;
        private System.Windows.Forms.Label curvePreviewLabel;
        private System.Windows.Forms.Panel previewPanel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView paramListView;
        private System.Windows.Forms.ColumnHeader nameColumnHeader;
        private System.Windows.Forms.ColumnHeader typeColumnHeader;
        private System.Windows.Forms.ColumnHeader valueColumnHeader;
        private System.Windows.Forms.CheckBox visibleCheckBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button applyToAllButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox showGridCheckBox;
        private System.Windows.Forms.Label previewLabel;
        private System.Windows.Forms.Panel graphPreviewPanel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox chartModeComboBox;
        private System.Windows.Forms.ToolStripMenuItem addHorizontalLineToolStripMenuItem;
        private System.Windows.Forms.TextBox lineValueTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Panel lineColorPanel;
        private System.Windows.Forms.Panel textBackgroundColorPanel;
        private System.Windows.Forms.Panel backgroundColorPanel;
        private System.Windows.Forms.Panel gridColorPanel;
        private System.Windows.Forms.ToolStripMenuItem addPaintBarsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addAutoDrawingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addTrailStopsToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader minColumnHeader;
        private System.Windows.Forms.ColumnHeader maxColumnHeader;
        private GroupBox secondarySerieGroupBox;
        private Panel secondaryColorPanel;
        private Label label11;
        private Label label10;
        private ComboBox secondaryThicknessComboBox;
        private Label someTextLabel;
        private ToolStripMenuItem copyStripMenuItem;
    }
}