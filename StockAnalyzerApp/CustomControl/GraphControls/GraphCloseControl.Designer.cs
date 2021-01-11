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
            this.shortMenu = new System.Windows.Forms.MenuItem();
            this.coverMenu = new System.Windows.Forms.MenuItem();
            this.deleteOperationMenu = new System.Windows.Forms.MenuItem();
            this.commentMenu = new System.Windows.Forms.MenuItem();
            this.agendaMenu = new MenuItem();
            this.openInABCMenu = new MenuItem();
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
            this.addAlertMenu,
            this.separator1,
            this.buyMenu,
            this.sellMenu,
            this.shortMenu,
            this.coverMenu,
            this.deleteOperationMenu,
            this.separator2,
            this.agendaMenu,
            this.commentMenu,
            this.separator3,
            this.openInABCMenu,
            this.openInPEAPerfMenu,
            this.openInZBMenu,
            this.openInSocGenMenu});
            // 
            // addAlertMenu
            // 
            this.addAlertMenu.Index = 0;
            this.addAlertMenu.Text = "Add Alert";
            this.addAlertMenu.Click += new System.EventHandler(this.addAlertMenu_Click);
            // 
            // separator1
            // 
            this.separator1.Index = 1;
            this.separator1.Text = "-";
            // 
            // buyMenu
            // 
            this.buyMenu.Index = 2;
            this.buyMenu.Text = "Buy";
            this.buyMenu.Click += new System.EventHandler(this.buyMenu_Click);
            // 
            // sellMenu
            // 
            this.sellMenu.Index = 3;
            this.sellMenu.Text = "Sell";
            this.sellMenu.Click += new System.EventHandler(this.sellMenu_Click);
            // 
            // shortMenu
            // 
            this.shortMenu.Index = 4;
            this.shortMenu.Text = "Short";
            this.shortMenu.Click += new System.EventHandler(this.shortMenu_Click);
            // 
            // coverMenu
            // 
            this.coverMenu.Index = 5;
            this.coverMenu.Text = "Cover";
            this.coverMenu.Click += new System.EventHandler(this.coverMenu_Click);
            // 
            // deleteOperationMenu
            // 
            this.deleteOperationMenu.Index = 6;
            this.deleteOperationMenu.Text = "Delete";
            this.deleteOperationMenu.Click += new System.EventHandler(this.deleteOperationMenu_Click);
            // 
            // separator2
            // 
            this.separator2.Index = 7;
            this.separator2.Text = "-";
            // 
            // commentMenu
            // 
            this.commentMenu.Index = 8;
            this.commentMenu.Text = "Add Comment";
            this.commentMenu.Click += new System.EventHandler(this.commentMenu_Click);
            // 
            // agendaMenu
            // 
            this.agendaMenu.Index = 9;
            this.agendaMenu.Text = "Agenda";
            this.agendaMenu.Click += new System.EventHandler(this.agendaMenu_Click);
            // 
            // separator3
            // 
            this.separator3.Index = 10;
            this.separator3.Text = "-";
            // 
            // openInABCMenu
            // 
            this.openInABCMenu.Index = 11;
            this.openInABCMenu.Text = "Open in ABCBourse";
            this.openInABCMenu.Click += new System.EventHandler(this.openInABCMenu_Click);
            // 
            // openInPEAPerf
            // 
            this.openInPEAPerfMenu.Index = 12;
            this.openInPEAPerfMenu.Text = "Open in PEAPerformance";
            this.openInPEAPerfMenu.Click += new System.EventHandler(this.openInPEAPerf_Click);
            // 
            // openInZBMenu
            // 
            this.openInZBMenu.Index = 13;
            this.openInZBMenu.Text = "Open in Zone Bourse";
            this.openInZBMenu.Click += new System.EventHandler(this.openInZBMenu_Click);
            // 
            // openInSocGenMenu
            // 
            this.openInSocGenMenu.Index = 14;
            this.openInSocGenMenu.Text = "Open in SogGen Products";
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
        private MenuItem shortMenu;
        private MenuItem coverMenu;
        private MenuItem deleteOperationMenu;
        private MenuItem separator1;
        private MenuItem separator2;
        private MenuItem separator3;
        private MenuItem commentMenu;
        private MenuItem agendaMenu;
        private MenuItem openInABCMenu;
        private MenuItem openInPEAPerfMenu;
        private MenuItem openInZBMenu;
        private MenuItem openInSocGenMenu;
        #endregion
    }
}
