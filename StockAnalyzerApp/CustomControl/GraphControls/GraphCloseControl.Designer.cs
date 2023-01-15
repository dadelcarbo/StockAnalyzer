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
            this.addAlertMenu = new System.Windows.Forms.MenuItem();
            this.buyMenu = new System.Windows.Forms.MenuItem();
            this.sellMenu = new System.Windows.Forms.MenuItem();
            this.cancelMenu = new System.Windows.Forms.MenuItem();
            this.agendaMenu = new MenuItem();
            this.openInABCMenu = new MenuItem();
            this.openInTradingViewMenu = new MenuItem();
            this.openInPEAPerfMenu = new MenuItem();
            this.openInZBMenu = new MenuItem();
            this.openInSocGenMenu = new MenuItem();
            this.separator1 = new MenuItem();
            this.separator2 = new MenuItem();
            this.separator3 = new MenuItem();
            // 
            // contextMenu
            // 
            this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.buyMenu,
            this.sellMenu,
            this.cancelMenu,
            this.separator1,
            this.addAlertMenu,
            this.separator2,
            this.agendaMenu,
            this.separator3,
            this.openInABCMenu,
            this.openInTradingViewMenu,
            this.openInPEAPerfMenu,
            this.openInZBMenu,
            this.openInSocGenMenu});
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
            // cancelMenu
            // 
            this.cancelMenu.Index = 2;
            this.cancelMenu.Text = "Cancel Order";
            this.cancelMenu.Click += new System.EventHandler(this.cancelMenu_Click);
            // 
            // separator1
            // 
            this.separator1.Index = 3;
            this.separator1.Text = "-";
            // 
            // addAlertMenu
            // 
            this.addAlertMenu.Index = 4;
            this.addAlertMenu.Text = "Add Alert";
            this.addAlertMenu.Click += new System.EventHandler(this.addAlertMenu_Click);
            // 
            // separator2
            // 
            this.separator2.Index = 5;
            this.separator2.Text = "-";
            // 
            // agendaMenu
            // 
            this.agendaMenu.Index = 6;
            this.agendaMenu.Text = "Agenda";
            this.agendaMenu.Click += new System.EventHandler(this.agendaMenu_Click);
            // 
            // separator3
            // 
            this.separator3.Index = 7;
            this.separator3.Text = "-";
            // 
            // openInABCMenu
            // 
            this.openInABCMenu.Index = 8;
            this.openInABCMenu.Text = "Open in ABCBourse";
            this.openInABCMenu.Click += new System.EventHandler(this.openInABCMenu_Click);
            // 
            // openInTradingViewMenu
            // 
            this.openInTradingViewMenu.Index = 9;
            this.openInTradingViewMenu.Text = "Open in TradingView";
            this.openInTradingViewMenu.Click += new System.EventHandler(this.openInTradingView_Click);
            // 
            // openInPEAPerf
            // 
            this.openInPEAPerfMenu.Index = 10;
            this.openInPEAPerfMenu.Text = "Open in PEAPerformance";
            this.openInPEAPerfMenu.Click += new System.EventHandler(this.openInPEAPerf_Click);
            // 
            // openInZBMenu
            // 
            this.openInZBMenu.Index = 11;
            this.openInZBMenu.Text = "Open in Zone Bourse";
            this.openInZBMenu.Click += new System.EventHandler(this.openInZBMenu_Click);
            // 
            // openInSocGenMenu
            // 
            this.openInSocGenMenu.Index = 12;
            this.openInSocGenMenu.Text = "Open in Data Provider";
            this.openInSocGenMenu.Click += new System.EventHandler(this.openInSocGenMenu_Click);
            // 
            // GraphCloseControl
            // 
            this.Name = "GraphCloseControl";
            this.ResumeLayout(false);
        }

        private ContextMenu contextMenu;
        private MenuItem addAlertMenu;
        private MenuItem buyMenu;
        private MenuItem sellMenu;
        private MenuItem cancelMenu;
        private MenuItem separator1;
        private MenuItem separator2;
        private MenuItem separator3;
        private MenuItem agendaMenu;
        private MenuItem openInABCMenu;
        private MenuItem openInPEAPerfMenu;
        private MenuItem openInTradingViewMenu;
        private MenuItem openInZBMenu;
        private MenuItem openInSocGenMenu;
        #endregion
    }
}
