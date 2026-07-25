using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzerApp.CustomControl.GraphControls;
using StockAnalyzer.StockData;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl
{
    public partial class MultiTimeFrameChartDlg : Form
    {
        private Groups selectedGroup;
        public MultiTimeFrameChartDlg()
        {
            this.fullGraphUserControl1 = new FullGraphUserControl(BarDuration.Monthly);
            this.fullGraphUserControl2 = new FullGraphUserControl(BarDuration.Weekly);
            this.fullGraphUserControl3 = new FullGraphUserControl(BarDuration.Daily);
            InitializeComponent();

            this.fullGraphUserControl1.OnMouseDateChanged += fullGraphUserControl2.MouseDateChanged;
            this.fullGraphUserControl1.OnMouseDateChanged += fullGraphUserControl3.MouseDateChanged;

            this.fullGraphUserControl2.OnMouseDateChanged += fullGraphUserControl1.MouseDateChanged;
            this.fullGraphUserControl2.OnMouseDateChanged += fullGraphUserControl3.MouseDateChanged;

            this.fullGraphUserControl3.OnMouseDateChanged += fullGraphUserControl1.MouseDateChanged;
            this.fullGraphUserControl3.OnMouseDateChanged += fullGraphUserControl2.MouseDateChanged;
        }

        public void Initialize(StockInstrument instrument)
        {
            this.selectedGroup = instrument.Group;
            this.Instrument = instrument;

            switch (this.selectedGroup)
            {
                case Groups.TURBO:
                    fullGraphUserControl3.SetDuration(BarDuration.Daily);
                    break;
            }

            InitialiseStockCombo();
        }

        public void ApplyTheme(string theme)
        {
            using MethodLogger ml = new MethodLogger(this);
            this.fullGraphUserControl1.Instrument = instrument;
            this.fullGraphUserControl1.ApplyTheme(theme);
            this.fullGraphUserControl2.Instrument = instrument;
            this.fullGraphUserControl2.ApplyTheme(theme);
            this.fullGraphUserControl3.Instrument = instrument;
            this.fullGraphUserControl3.ApplyTheme(theme);
        }

        private void StockNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Instrument = stockNameComboBox.SelectedItem as StockInstrument;
        }

        private void InitialiseStockCombo()
        {
            // Initialise Combo values
            stockNameComboBox.Items.Clear();

            var instruments = StockDictionary.Instruments.Values.Where(s => s.BelongsToGroup(this.selectedGroup));
            foreach (var instrument in instruments)
            {
                stockNameComboBox.Items.Add(instrument);
            }
            stockNameComboBox.SelectedItem = this.instrument;
        }

        private StockInstrument instrument;
        public StockInstrument Instrument
        {
            get { return instrument; }
            set
            {
                if (instrument != value)
                {
                    instrument = value;
                    this.ApplyTheme(null);
                }
            }
        }

        public void SetSerieAndTheme(StockInstrument instrument, string theme)
        {
            this.instrument = instrument;
            this.ApplyTheme(theme);
        }
    }
}
