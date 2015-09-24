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
         // 
         // GraphCloseControl
         // 
         this.Name = "GraphCloseControl";
         this.ResumeLayout(false);
         this.PerformLayout();

         this.contextMenu = new ContextMenu();

         buyMenu = new MenuItem("Buy");
         buyMenu.Click += new System.EventHandler(buyMenu_Click);
         contextMenu.MenuItems.Add(buyMenu);

         sellMenu = new MenuItem("Sell");
         sellMenu.Click += new System.EventHandler(sellMenu_Click);
         contextMenu.MenuItems.Add(sellMenu);

         shortMenu = new MenuItem("Short");
         shortMenu.Click += new System.EventHandler(shortMenu_Click);
         contextMenu.MenuItems.Add(shortMenu);

         coverMenu = new MenuItem("Cover");
         coverMenu.Click += new System.EventHandler(coverMenu_Click);
         contextMenu.MenuItems.Add(coverMenu);
      }

      private ContextMenu contextMenu;
      private MenuItem buyMenu;
      private MenuItem sellMenu;
      private MenuItem shortMenu;
      private MenuItem coverMenu;

      #endregion

   }
}
