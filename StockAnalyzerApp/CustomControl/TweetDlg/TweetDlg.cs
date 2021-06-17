using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzerApp.CustomControl.GraphControls;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.TweetDlg
{
    public partial class TweetDlg : Form
    {
        public TweetDlg()
        {
            this.fullGraphUserControl = new FullGraphUserControl(StockBarDuration.Monthly);
            InitializeComponent();
        }

        public void Initialize(StockSerie.Groups group, StockSerie stockSerie)
        {
            this.CurrentStockSerie = stockSerie;
        }

        public void ApplyTheme()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                this.fullGraphUserControl.CurrentStockSerie = currentStockSerie;
                this.fullGraphUserControl.ApplyTheme();
            }
        }

        private StockSerie currentStockSerie;
        public StockSerie CurrentStockSerie
        {
            get { return currentStockSerie; }
            set
            {
                if (currentStockSerie != value)
                {
                    currentStockSerie = value;
                    this.ApplyTheme();
                }
            }
        }
    }
}
