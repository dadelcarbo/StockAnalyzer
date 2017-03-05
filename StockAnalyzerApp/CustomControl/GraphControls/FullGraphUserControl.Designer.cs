namespace StockAnalyzerApp.CustomControl.GraphControls
{
    partial class FullGraphUserControl
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.graphCloseControl = new GraphCloseControl();
            this.graphVolumeControl = new GraphVolumeControl();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.graphScrollerControl = new GraphScrollerControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(3, 41);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.graphCloseControl);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.graphVolumeControl);
            this.splitContainer.Size = new System.Drawing.Size(594, 256);
            this.splitContainer.SplitterDistance = 204;
            this.splitContainer.TabIndex = 0;
            // 
            // graphCloseControl
            // 
            this.graphCloseControl.Agenda = null;
            this.graphCloseControl.BackgroundColor = System.Drawing.Color.White;
            this.graphCloseControl.ChartMode = GraphChartMode.Line;
            this.graphCloseControl.Comments = null;
            this.graphCloseControl.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphCloseControl.CurveList = null;
            this.graphCloseControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphCloseControl.DrawingMode = GraphDrawMode.Normal;
            this.graphCloseControl.DrawingPen = null;
            this.graphCloseControl.DrawingStep = GraphDrawingStep.SelectItem;
            this.graphCloseControl.EndIndex = 0;
            this.graphCloseControl.GridColor = System.Drawing.Color.Empty;
            this.graphCloseControl.HideIndicators = false;
            this.graphCloseControl.horizontalLines = null;
            this.graphCloseControl.IsLogScale = false;
            this.graphCloseControl.Location = new System.Drawing.Point(0, 0);
            this.graphCloseControl.Magnetism = false;
            this.graphCloseControl.Name = "graphCloseControl";
            this.graphCloseControl.Portofolio = null;
            this.graphCloseControl.ScaleInvisible = false;
            this.graphCloseControl.SecondaryFloatSerie = null;
            this.graphCloseControl.SecondaryPen = null;
            this.graphCloseControl.ShowGrid = false;
            this.graphCloseControl.ShowVariation = false;
            this.graphCloseControl.Size = new System.Drawing.Size(594, 204);
            this.graphCloseControl.StartIndex = 0;
            this.graphCloseControl.TabIndex = 0;
            this.graphCloseControl.TextBackgroundColor = System.Drawing.Color.Empty;
            // 
            // graphVolumeControl
            // 
            this.graphVolumeControl.BackgroundColor = System.Drawing.Color.White;
            this.graphVolumeControl.ChartMode = GraphChartMode.Line;
            this.graphVolumeControl.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphVolumeControl.CurveList = null;
            this.graphVolumeControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphVolumeControl.DrawingMode = GraphDrawMode.Normal;
            this.graphVolumeControl.DrawingPen = null;
            this.graphVolumeControl.DrawingStep = GraphDrawingStep.SelectItem;
            this.graphVolumeControl.EndIndex = 0;
            this.graphVolumeControl.GridColor = System.Drawing.Color.Empty;
            this.graphVolumeControl.horizontalLines = null;
            this.graphVolumeControl.IsLogScale = false;
            this.graphVolumeControl.Location = new System.Drawing.Point(0, 0);
            this.graphVolumeControl.Name = "graphVolumeControl";
            this.graphVolumeControl.ScaleInvisible = false;
            this.graphVolumeControl.ShowGrid = false;
            this.graphVolumeControl.ShowVariation = false;
            this.graphVolumeControl.Size = new System.Drawing.Size(594, 48);
            this.graphVolumeControl.StartIndex = 0;
            this.graphVolumeControl.TabIndex = 0;
            this.graphVolumeControl.TextBackgroundColor = System.Drawing.Color.Empty;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.splitContainer, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.graphScrollerControl, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 87.33334F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(600, 300);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // graphScrollerControl
            // 
            this.graphScrollerControl.BackgroundColor = System.Drawing.Color.White;
            this.graphScrollerControl.ChartMode = GraphChartMode.Line;
            this.graphScrollerControl.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphScrollerControl.CurveList = null;
            this.graphScrollerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphScrollerControl.DrawingMode = GraphDrawMode.Normal;
            this.graphScrollerControl.DrawingPen = null;
            this.graphScrollerControl.DrawingStep = GraphDrawingStep.SelectItem;
            this.graphScrollerControl.EndIndex = 0;
            this.graphScrollerControl.GridColor = System.Drawing.Color.Empty;
            this.graphScrollerControl.horizontalLines = null;
            this.graphScrollerControl.IsLogScale = false;
            this.graphScrollerControl.Location = new System.Drawing.Point(3, 3);
            this.graphScrollerControl.Name = "graphScrollerControl";
            this.graphScrollerControl.ScaleInvisible = false;
            this.graphScrollerControl.ShowGrid = false;
            this.graphScrollerControl.ShowVariation = false;
            this.graphScrollerControl.Size = new System.Drawing.Size(594, 32);
            this.graphScrollerControl.StartIndex = 0;
            this.graphScrollerControl.TabIndex = 1;
            this.graphScrollerControl.TextBackgroundColor = System.Drawing.Color.Empty;
            // 
            // FullGraphUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FullGraphUserControl";
            this.Size = new System.Drawing.Size(600, 300);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private GraphCloseControl graphCloseControl;
        private GraphVolumeControl graphVolumeControl;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private GraphScrollerControl graphScrollerControl;

    }
}
