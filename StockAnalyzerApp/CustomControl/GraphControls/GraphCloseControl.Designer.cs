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
            int index = 0;
            this.SuspendLayout();
            base.InitializeComponent();

            this.contextMenu = new System.Windows.Forms.ContextMenu();
            this.addAlertMenu = new System.Windows.Forms.MenuItem();
            this.buyMenu = new System.Windows.Forms.MenuItem();
            this.sellMenu = new System.Windows.Forms.MenuItem();
            this.cancelMenu = new System.Windows.Forms.MenuItem();
            this.agendaMenu = new MenuItem();
            this.openInPEAPerfMenu = new MenuItem();
            this.openInZBMenu = new MenuItem();
            this.openInDataProvider = new MenuItem();
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
            this.openInPEAPerfMenu,
            this.openInZBMenu,
            this.openInDataProvider});
            // 
            // buyMenu
            // 
            this.buyMenu.Index = index++;
            this.buyMenu.Text = "Buy";
            this.buyMenu.Click += new System.EventHandler(this.buyMenu_Click);
            // 
            // sellMenu
            // 
            this.sellMenu.Index = index++;
            this.sellMenu.Text = "Sell";
            this.sellMenu.Click += new System.EventHandler(this.sellMenu_Click);
            // 
            // cancelMenu
            // 
            this.cancelMenu.Index = index++;
            this.cancelMenu.Text = "Cancel Order";
            this.cancelMenu.Click += new System.EventHandler(this.cancelMenu_Click);
            // 
            // separator1
            // 
            this.separator1.Index = index++;
            this.separator1.Text = "-";
            // 
            // addAlertMenu
            // 
            this.addAlertMenu.Index = index++;
            this.addAlertMenu.Text = "Add Alert";
            this.addAlertMenu.Click += new System.EventHandler(this.addAlertMenu_Click);
            // 
            // separator2
            // 
            this.separator2.Index = index++;
            this.separator2.Text = "-";
            // 
            // agendaMenu
            // 
            this.agendaMenu.Index = index++;
            this.agendaMenu.Text = "Agenda";
            this.agendaMenu.Click += new System.EventHandler(this.agendaMenu_Click);
            // 
            // separator3
            // 
            this.separator3.Index = index++;
            this.separator3.Text = "-";
            // 
            // openInPEAPerf
            // 
            this.openInPEAPerfMenu.Index = index++;
            this.openInPEAPerfMenu.Text = "Open in PEAPerformance";
            this.openInPEAPerfMenu.Click += new System.EventHandler(this.openInPEAPerf_Click);
            // 
            // openInZBMenu
            // 
            this.openInZBMenu.Index = index++;
            this.openInZBMenu.Text = "Open in Zone Bourse";
            this.openInZBMenu.Click += new System.EventHandler(this.openInZBMenu_Click);
            // 
            // openInSocGenMenu
            // 
            this.openInDataProvider.Index = index++;
            this.openInDataProvider.Text = "Open in Data Provider";
            this.openInDataProvider.Click += new System.EventHandler(this.openInDataProvider_Click);
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
        private MenuItem openInPEAPerfMenu;
        private MenuItem openInZBMenu;
        private MenuItem openInDataProvider;
        #endregion
    }
}
