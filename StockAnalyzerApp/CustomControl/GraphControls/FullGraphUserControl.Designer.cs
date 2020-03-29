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
            this.graphCloseControl = new GraphCloseControl();
            this.graphVolumeControl = new GraphVolumeControl();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.durationComboBox = new System.Windows.Forms.ComboBox();
            this.graphScrollerControl = new GraphScrollerControl();
            this.graphIndicator1Control = new GraphRangedControl();
            this.graphIndicator2Control = new GraphRangedControl();
            this.graphIndicator3Control = new GraphRangedControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
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
            this.graphCloseControl.ScaleInvisible = false;
            this.graphCloseControl.SecondaryFloatSerie = null;
            this.graphCloseControl.SecondaryPen = null;
            this.graphCloseControl.ShowGrid = false;
            this.graphCloseControl.ShowVariation = false;
            this.graphCloseControl.Size = new System.Drawing.Size(590, 136);
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
            this.graphVolumeControl.Size = new System.Drawing.Size(590, 32);
            this.graphVolumeControl.StartIndex = 0;
            this.graphVolumeControl.TabIndex = 0;
            this.graphVolumeControl.TextBackgroundColor = System.Drawing.Color.Empty;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.durationComboBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.graphScrollerControl, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.graphCloseControl, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.graphIndicator1Control, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.graphIndicator2Control, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.graphIndicator3Control, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.graphVolumeControl, 0, 6);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize, 22F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(600, 300);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // durationComboBox
            // 
            this.durationComboBox.FormattingEnabled = true;
            this.durationComboBox.Location = new System.Drawing.Point(5, 5);
            this.durationComboBox.Name = "durationComboBox";
            this.durationComboBox.Size = new System.Drawing.Size(209, 21);
            this.durationComboBox.TabIndex = 2;
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
            this.graphScrollerControl.Location = new System.Drawing.Point(5, 29);
            this.graphScrollerControl.Name = "graphScrollerControl";
            this.graphScrollerControl.ScaleInvisible = false;
            this.graphScrollerControl.ShowGrid = false;
            this.graphScrollerControl.ShowVariation = false;
            this.graphScrollerControl.Size = new System.Drawing.Size(590, 19);
            this.graphScrollerControl.StartIndex = 0;
            this.graphScrollerControl.TabIndex = 1;
            this.graphScrollerControl.TextBackgroundColor = System.Drawing.Color.Empty;
            // 
            // graphIndicator1Control
            // 
            this.graphIndicator1Control.BackgroundColor = System.Drawing.Color.White;
            this.graphIndicator1Control.ChartMode = GraphChartMode.Line;
            this.graphIndicator1Control.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphIndicator1Control.CurveList = null;
            this.graphIndicator1Control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphIndicator1Control.DrawingMode = GraphDrawMode.Normal;
            this.graphIndicator1Control.DrawingPen = null;
            this.graphIndicator1Control.DrawingStep = GraphDrawingStep.SelectItem;
            this.graphIndicator1Control.EndIndex = 0;
            this.graphIndicator1Control.GridColor = System.Drawing.Color.Empty;
            this.graphIndicator1Control.horizontalLines = null;
            this.graphIndicator1Control.IsLogScale = false;
            this.graphIndicator1Control.Location = new System.Drawing.Point(5, 236);
            this.graphIndicator1Control.Name = "graphIndicator1Control";
            this.graphIndicator1Control.RangeMax = 0F;
            this.graphIndicator1Control.RangeMin = 0F;
            this.graphIndicator1Control.ScaleInvisible = false;
            this.graphIndicator1Control.ShowGrid = false;
            this.graphIndicator1Control.ShowVariation = false;
            this.graphIndicator1Control.Size = new System.Drawing.Size(200, 14);
            this.graphIndicator1Control.StartIndex = 0;
            this.graphIndicator1Control.TabIndex = 3;
            this.graphIndicator1Control.TextBackgroundColor = System.Drawing.Color.Empty;
            // 
            // graphIndicator2Control
            // 
            this.graphIndicator2Control.BackgroundColor = System.Drawing.Color.White;
            this.graphIndicator2Control.ChartMode = GraphChartMode.Line;
            this.graphIndicator2Control.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphIndicator2Control.CurveList = null;
            this.graphIndicator2Control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphIndicator2Control.DrawingMode = GraphDrawMode.Normal;
            this.graphIndicator2Control.DrawingPen = null;
            this.graphIndicator2Control.DrawingStep = GraphDrawingStep.SelectItem;
            this.graphIndicator2Control.EndIndex = 0;
            this.graphIndicator2Control.GridColor = System.Drawing.Color.Empty;
            this.graphIndicator2Control.horizontalLines = null;
            this.graphIndicator2Control.IsLogScale = false;
            this.graphIndicator2Control.Location = new System.Drawing.Point(5, 258);
            this.graphIndicator2Control.Name = "graphIndicator2Control";
            this.graphIndicator2Control.RangeMax = 0F;
            this.graphIndicator2Control.RangeMin = 0F;
            this.graphIndicator2Control.ScaleInvisible = false;
            this.graphIndicator2Control.ShowGrid = false;
            this.graphIndicator2Control.ShowVariation = false;
            this.graphIndicator2Control.Size = new System.Drawing.Size(200, 14);
            this.graphIndicator2Control.StartIndex = 0;
            this.graphIndicator2Control.TabIndex = 4;
            this.graphIndicator2Control.TextBackgroundColor = System.Drawing.Color.Empty;
            // 
            // graphIndicator3Control
            // 
            this.graphIndicator3Control.BackgroundColor = System.Drawing.Color.White;
            this.graphIndicator3Control.ChartMode = GraphChartMode.Line;
            this.graphIndicator3Control.Cursor = System.Windows.Forms.Cursors.Cross;
            this.graphIndicator3Control.CurveList = null;
            this.graphIndicator3Control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphIndicator3Control.DrawingMode = GraphDrawMode.Normal;
            this.graphIndicator3Control.DrawingPen = null;
            this.graphIndicator3Control.DrawingStep = GraphDrawingStep.SelectItem;
            this.graphIndicator3Control.EndIndex = 0;
            this.graphIndicator3Control.GridColor = System.Drawing.Color.Empty;
            this.graphIndicator3Control.horizontalLines = null;
            this.graphIndicator3Control.IsLogScale = false;
            this.graphIndicator3Control.Location = new System.Drawing.Point(5, 280);
            this.graphIndicator3Control.Name = "graphIndicator3Control";
            this.graphIndicator3Control.RangeMax = 0F;
            this.graphIndicator3Control.RangeMin = 0F;
            this.graphIndicator3Control.ScaleInvisible = false;
            this.graphIndicator3Control.ShowGrid = false;
            this.graphIndicator3Control.ShowVariation = false;
            this.graphIndicator3Control.Size = new System.Drawing.Size(200, 15);
            this.graphIndicator3Control.StartIndex = 0;
            this.graphIndicator3Control.TabIndex = 5;
            this.graphIndicator3Control.TextBackgroundColor = System.Drawing.Color.Empty;
            // 
            // FullGraphUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FullGraphUserControl";
            this.Size = new System.Drawing.Size(600, 300);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox durationComboBox;
        private GraphCloseControl graphCloseControl;
        private GraphVolumeControl graphVolumeControl;
        private GraphScrollerControl graphScrollerControl;
        private GraphRangedControl graphIndicator1Control;
        private GraphRangedControl graphIndicator2Control;
        private GraphRangedControl graphIndicator3Control;
    }
}
