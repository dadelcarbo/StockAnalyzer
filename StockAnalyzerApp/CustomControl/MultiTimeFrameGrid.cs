using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl
{
   public partial class MultiTimeFrameGrid : Form
   {
      public MultiTimeFrameGrid()
      {
         InitializeComponent();

         dataGridView.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView_CellFormatting);
         dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.Raised;
      }

      private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
      {
         if (e.Value.Equals(StockSerie.Trend.UpTrend))
         {
            e.CellStyle.SelectionBackColor = e.CellStyle.BackColor = Color.LimeGreen;
            //Don't display 'True' or 'False'
            e.Value = string.Empty;
         }
         else if (e.Value.Equals(StockSerie.Trend.DownTrend))
         {
            e.CellStyle.SelectionBackColor = e.CellStyle.BackColor = Color.DarkRed;
            //Don't display 'True' or 'False'
            e.Value = string.Empty;
         }
      }

      public void LoadData(List<StockBarDuration> durations, List<string> indicators,
          List<StockSerie> stockSeries)
      {
         // add columns.
         dataGridView.ColumnCount = durations.Count * indicators.Count + 1;
         dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
         int i = 1;
         dataGridView.Columns[0].Name = "Name";
         foreach (string indicator in indicators)
         {
            foreach (StockBarDuration duration in durations)
            {
               dataGridView.Columns[i].Name = indicator + System.Environment.NewLine + duration.ToString();
               i++;
            }
         }

         object[] cells = new object[dataGridView.ColumnCount];
         foreach (StockSerie stockSerie in stockSeries)
         {
            cells[0] = stockSerie.StockName;
            stockSerie.GenerateMultiTimeFrameTrendSummary(indicators, durations).CopyTo(cells, 1);

            dataGridView.Rows.Add(cells);
         }

         dataGridView.Refresh();
      }
   }
}
