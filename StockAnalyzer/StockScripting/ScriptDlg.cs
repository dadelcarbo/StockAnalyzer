using StockAnalyzer.StockClasses;
using StockAnalyzer.StockScripting;
using System;
using System.CodeDom.Compiler;
using System.Drawing;
using System.Windows.Forms;

namespace StockAnalyzerApp.StockScripting
{
    public partial class ScriptDlg : Form
    {
        public ScriptDlg()
        {
            InitializeComponent();
        }

        private void CompileBtn_Click(object sender, EventArgs e)
        {
            var filter = StockScriptManager.Instance.CreateStockFilterInstance("Test", scriptTextBox.Text);

            if (filter == null)
            {
                resultTextBox.ForeColor = Color.Red;
                foreach (CompilerError CompErr in StockScriptManager.Instance.CompilerResults.Errors)
                {
                    resultTextBox.Text = resultTextBox.Text +
                                "Line number " + CompErr.Line +
                                ", Error Number: " + CompErr.ErrorNumber +
                                ", '" + CompErr.ErrorText + ";" +
                                Environment.NewLine + Environment.NewLine;
                }
            }
            else
            {
                //Successful Compile
                resultTextBox.ForeColor = Color.Blue;
                resultTextBox.Text = "Success!" + Environment.NewLine;

                var stockSerie = StockDictionary.Instance["CAC40"];
                if (filter.MatchFilter(stockSerie, BarDuration.Daily))
                {
                    resultTextBox.Text += "Match: OK" + Environment.NewLine;
                }
                else {
                    resultTextBox.Text += "Match: KO" + Environment.NewLine;
                }


            }


        }
    }
}