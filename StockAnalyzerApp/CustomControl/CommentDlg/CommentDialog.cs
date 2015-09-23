using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl
{
    public partial class CommentDialog : Form
    {
        private StockSerie stockSerie;
        public CommentDialog()
        {
            InitializeComponent();
        }
        public CommentDialog(StockSerie stockSerie)
        {
            InitializeComponent();

            StockSerie.StockBarDuration currentBarDuration = stockSerie.BarDuration;
            this.stockSerie = stockSerie;
            stockSerie.BarDuration = StockSerie.StockBarDuration.Daily;

            this.Text += ": " + stockSerie.StockName;

            if (stockSerie.StockAnalysis.Comments.ContainsKey(stockSerie.Keys.Last().Date))
            {
                this.commentBox.Text = stockSerie.StockAnalysis.Comments[stockSerie.Keys.Last().Date];
            }

            this.InitDateCombo();

            stockSerie.BarDuration = currentBarDuration;
        }

        private void InitDateCombo()
        {
            this.dateComboBox.Items.Clear();
            List<String> dateStrings = new List<string>();
            foreach (DateTime date in stockSerie.Keys)
            {
                if (stockSerie.StockAnalysis.Comments.ContainsKey(date))
                {
                    dateStrings.Insert(0, "* " + date.ToShortDateString());
                }
                else
                {
                    dateStrings.Insert(0, date.ToShortDateString());
                }
            }
            this.dateComboBox.Items.AddRange(dateStrings.ToArray());
            this.dateComboBox.SelectedItem = this.dateComboBox.Items[0];
        }
        private void okBtn_Click(object sender, EventArgs e)
        {

        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void commentBox_Validated(object sender, EventArgs e)
        {
            string selectedDate = ((string)this.dateComboBox.SelectedItem).Replace("* ", "");
            DateTime date = DateTime.Parse(selectedDate);
            if (string.IsNullOrWhiteSpace(this.commentBox.Text))
            {
                if (stockSerie.StockAnalysis.Comments.ContainsKey(date))
                {
                    stockSerie.StockAnalysis.Comments.Remove(date);

                    this.dateComboBox.Items[this.dateComboBox.SelectedIndex] = selectedDate;
                }
            }
            else
            {
                if (stockSerie.StockAnalysis.Comments.ContainsKey(date))
                {
                    stockSerie.StockAnalysis.Comments[date] = this.commentBox.Text;
                }
                else
                {
                    stockSerie.StockAnalysis.Comments.Add(date, this.commentBox.Text);
                }
                this.dateComboBox.Items[this.dateComboBox.SelectedIndex] = "* " + selectedDate;
            }
        }

        private void dateComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedDate = ((string)this.dateComboBox.SelectedItem).Replace("* ", "");
            DateTime date = DateTime.Parse(selectedDate);

            if (stockSerie.StockAnalysis.Comments.ContainsKey(date))
            {
                this.commentBox.Text = stockSerie.StockAnalysis.Comments[date];
            }
            else
            {
                this.commentBox.Text = string.Empty;
            }
        }

        private void clearAllBtn_Click(object sender, EventArgs e)
        {
            this.stockSerie.StockAnalysis.Comments.Clear();
            this.commentBox.Text = string.Empty;
            StockSerie.StockBarDuration currentBarDuration = this.stockSerie.BarDuration;
            this.stockSerie.BarDuration = StockSerie.StockBarDuration.Daily;

            this.InitDateCombo();

            this.stockSerie.BarDuration = currentBarDuration;
        }
    }
}
