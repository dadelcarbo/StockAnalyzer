using System.Windows.Forms;
namespace StockAnalyzerApp.CustomControl.GraphControls
{
    partial class GraphCloseControl
    {
        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        protected override void InitializeComponent()
        {
            this.SuspendLayout();
            base.InitializeComponent();

            this.contextMenu = new System.Windows.Forms.ContextMenu();
            this.buyMenu = new System.Windows.Forms.MenuItem();
            this.sellMenu = new System.Windows.Forms.MenuItem();
            this.shortMenu = new System.Windows.Forms.MenuItem();
            this.coverMenu = new System.Windows.Forms.MenuItem();
            this.financialMenu = new System.Windows.Forms.MenuItem();
            this.agendaMenu = new MenuItem();
            this.openInABCMenu = new MenuItem();
            this.statMenu = new MenuItem();
            this.separator1 = new MenuItem();
            this.separator2 = new MenuItem();
            // 
            // contextMenu
            // 
            this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.buyMenu,
            this.sellMenu,
            this.shortMenu,
            this.coverMenu,
            this.separator1,
            this.agendaMenu,
            this.financialMenu,
            this.separator2,
            this.openInABCMenu,
            this.statMenu});
            // 
            // buyMenu
            // 
            this.buyMenu.Index = 0;
            this.buyMenu.Text = "Buy";
            this.buyMenu.Click += new System.EventHandler(this.buyMenu_Click);
            // 
            // sellMenu
            // 
            this.sellMenu.Index = 1;
            this.sellMenu.Text = "Sell";
            this.sellMenu.Click += new System.EventHandler(this.sellMenu_Click);
            // 
            // shortMenu
            // 
            this.shortMenu.Index = 2;
            this.shortMenu.Text = "Short";
            this.shortMenu.Click += new System.EventHandler(this.shortMenu_Click);
            // 
            // coverMenu
            // 
            this.coverMenu.Index = 3;
            this.coverMenu.Text = "Cover";
            this.coverMenu.Click += new System.EventHandler(this.coverMenu_Click);
            // 
            // separator1
            // 
            this.separator1.Index = 4;
            this.separator1.Text = "-";
            // 
            // agendaMenu
            // 
            this.agendaMenu.Index = 5;
            this.agendaMenu.Text = "Agenda";
            this.agendaMenu.Click += new System.EventHandler(this.agendaMenu_Click);
            // 
            // financialMenu
            // 
            this.financialMenu.Index = 6;
            this.financialMenu.Text = "Financials";
            this.financialMenu.Click += new System.EventHandler(this.financialMenu_Click);
            // 
            // separator2
            // 
            this.separator2.Index = 7;
            this.separator2.Text = "-";
            // 
            // openInABCMenu
            // 
            this.openInABCMenu.Index = 8;
            this.openInABCMenu.Text = "Open in ABCBourse";
            this.openInABCMenu.Click += new System.EventHandler(this.openInABCMenu_Click);
            // 
            // statMenu
            // 
            this.statMenu.Index = 9;
            this.statMenu.Text = "Make Stats";
            this.statMenu.Click += new System.EventHandler(this.statMenu_Click);
            // 
            // GraphCloseControl
            // 
            this.Name = "GraphCloseControl";
            this.ResumeLayout(false);

        }

        private ContextMenu contextMenu;
        private MenuItem buyMenu;
        private MenuItem sellMenu;
        private MenuItem shortMenu;
        private MenuItem coverMenu;
        private MenuItem separator1;
        private MenuItem separator2;
        private MenuItem financialMenu;
        private MenuItem agendaMenu;
        private MenuItem openInABCMenu;
        private MenuItem statMenu;

        #endregion

    }
}
