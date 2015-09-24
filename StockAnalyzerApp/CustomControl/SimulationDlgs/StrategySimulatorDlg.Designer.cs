using StockAnalyzerApp.CustomControl.SimulationDlgs;

namespace StockAnalyzerApp.CustomControl
{
   partial class StrategySimulatorDlg
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
         this.Portofolio = new System.Windows.Forms.Label();
         this.portofolioComboBox = new System.Windows.Forms.ComboBox();
         this.label3 = new System.Windows.Forms.Label();
         this.stockComboBox = new System.Windows.Forms.ComboBox();
         this.simulateTradingBtn = new System.Windows.Forms.Button();
         this.simulationParameterControl = new SimulationParameterControl();
         this.parametrizableControl = new CustomControl.IndicatorDlgs.ParametrizableControl();
         this.SuspendLayout();
         // 
         // Portofolio
         // 
         this.Portofolio.AutoSize = true;
         this.Portofolio.Location = new System.Drawing.Point(12, 15);
         this.Portofolio.Name = "Portofolio";
         this.Portofolio.Size = new System.Drawing.Size(51, 13);
         this.Portofolio.TabIndex = 0;
         this.Portofolio.Text = "Portofolio";
         // 
         // portofolioComboBox
         // 
         this.portofolioComboBox.FormattingEnabled = true;
         this.portofolioComboBox.Location = new System.Drawing.Point(69, 12);
         this.portofolioComboBox.Name = "portofolioComboBox";
         this.portofolioComboBox.Size = new System.Drawing.Size(238, 21);
         this.portofolioComboBox.TabIndex = 2;
         this.portofolioComboBox.SelectedIndexChanged += new System.EventHandler(this.portofolioComboBox_SelectedIndexChanged);
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(12, 42);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(35, 13);
         this.label3.TabIndex = 0;
         this.label3.Text = "Stock";
         // 
         // stockComboBox
         // 
         this.stockComboBox.FormattingEnabled = true;
         this.stockComboBox.Location = new System.Drawing.Point(69, 39);
         this.stockComboBox.Name = "stockComboBox";
         this.stockComboBox.Size = new System.Drawing.Size(238, 21);
         this.stockComboBox.TabIndex = 2;
         this.stockComboBox.SelectedIndexChanged += new System.EventHandler(this.stockComboBox_SelectedIndexChanged);
         // 
         // simulateTradingBtn
         // 
         this.simulateTradingBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.simulateTradingBtn.Location = new System.Drawing.Point(282, 532);
         this.simulateTradingBtn.Name = "simulateTradingBtn";
         this.simulateTradingBtn.Size = new System.Drawing.Size(99, 23);
         this.simulateTradingBtn.TabIndex = 4;
         this.simulateTradingBtn.Text = "Simulate Trading";
         this.simulateTradingBtn.UseVisualStyleBackColor = true;
         this.simulateTradingBtn.Click += new System.EventHandler(this.simulateTradingBtn_Click);
         // 
         // simulationParameterControl
         // 
         this.simulationParameterControl.EndDate = new System.DateTime(((long)(0)));
         this.simulationParameterControl.Location = new System.Drawing.Point(-2, 56);
         this.simulationParameterControl.Name = "simulationParameterControl";
         this.simulationParameterControl.Size = new System.Drawing.Size(390, 275);
         this.simulationParameterControl.StartDate = new System.DateTime(((long)(0)));
         this.simulationParameterControl.TabIndex = 5;
         this.simulationParameterControl.SelectedStrategyChanged += new SimulationParameterControl.SelectedStrategyHandler(this.simulationParameterControl_SelectedStrategyChanged);
         // 
         // parametrizableControl
         // 
         this.parametrizableControl.Location = new System.Drawing.Point(13, 338);
         this.parametrizableControl.Name = "parametrizableControl";
         this.parametrizableControl.Size = new System.Drawing.Size(294, 181);
         this.parametrizableControl.TabIndex = 6;
         this.parametrizableControl.ViewableItem = null;
         // 
         // StrategySimulatorDlg
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(393, 567);
         this.Controls.Add(this.parametrizableControl);
         this.Controls.Add(this.simulateTradingBtn);
         this.Controls.Add(this.stockComboBox);
         this.Controls.Add(this.portofolioComboBox);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.Portofolio);
         this.Controls.Add(this.simulationParameterControl);
         this.Name = "StrategySimulatorDlg";
         this.Text = "Trailing Order Simulation";
         this.ResumeLayout(false);
         this.PerformLayout();

      }
      #endregion

      private System.Windows.Forms.Label Portofolio;
      private System.Windows.Forms.ComboBox portofolioComboBox;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.ComboBox stockComboBox;
      private System.Windows.Forms.Button simulateTradingBtn;
      public SimulationParameterControl simulationParameterControl;
      private IndicatorDlgs.ParametrizableControl parametrizableControl;
   }
}