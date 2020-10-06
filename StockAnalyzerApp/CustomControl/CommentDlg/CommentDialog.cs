using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;
using StockAnalyzerApp.CustomControl.CommentDlg;

namespace StockAnalyzerApp.CustomControl
{
    public partial class CommentDialog : Form
    {
        private StockSerie stockSerie;

        public List<CommentEntry> CommentList { get; set; }
        public CommentDialog(StockSerie stockSerie)
        {
            InitializeComponent();
            this.stockSerie = stockSerie;

            this.commentControl1 = this.elementHost1.Child as CommentControl;
            this.commentControl1.DataContext = CommentList = stockSerie.StockAnalysis.Comments.Select(c => new CommentEntry { Date = c.Key, Comment = c.Value }).ToList();
        }
    }
}
