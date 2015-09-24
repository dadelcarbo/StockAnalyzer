using System.Windows.Forms;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl
{
   public partial class StockEventSelectorDlg : Form
   {
      public string SelectedEvents { get; set; }
      public StockEvent.EventFilterMode EventFilterMode { get; set; }

      public StockEventSelectorDlg(string selectedEventMask, StockEvent.EventFilterMode filterMode)
      {
         InitializeComponent();

         //
         this.SelectedEvents = selectedEventMask;
         this.EventFilterMode = filterMode;

         // Initialise event values
         this.eventSelectorCheckedListBox.Items.AddRange(StockEvent.GetSupportedEventTypes());

         // Retrieve last selected settings
         foreach (StockEvent.EventType checkedEvent in StockEvent.EventTypesFromString(SelectedEvents))
         {
            this.eventSelectorCheckedListBox.SetItemChecked(this.eventSelectorCheckedListBox.Items.IndexOf(checkedEvent.ToString()), true);
         }

         // Retrieve filter mode
         if (EventFilterMode.ToString() == "EventAll")
         {
            this.allEventsBtn.Checked = true;
            this.oneEventBtn.Checked = false;
         }
         else
         {
            this.allEventsBtn.Checked = false;
            this.oneEventBtn.Checked = true;
         }
      }

      private void OKButton_Click(object sender, System.EventArgs e)
      {
         SelectedEvents = string.Empty;

         // Save Selected filters
         foreach (string checkedEvent in this.eventSelectorCheckedListBox.CheckedItems)
         {
            if (!string.IsNullOrEmpty(SelectedEvents))
            {
               SelectedEvents += ";";
            }
            SelectedEvents += checkedEvent.ToString();
         }

         // Save filter mode
         if (this.allEventsBtn.Checked == true)
         {
            EventFilterMode = StockEvent.EventFilterMode.EventAll;
         }
         else
         {
            EventFilterMode = StockEvent.EventFilterMode.EventOne;
         }
      }
   }
}
