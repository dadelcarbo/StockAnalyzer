using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweetApp
{
    public class ViewModel : INotifyPropertyChanged
    {
        const string NOT_CONNECTED = "Not Connected";
        const string CONNECTED = "Connected";
        private string status = NOT_CONNECTED;
        public string Status
        {
            get { return status; }
            set { if (status != value) { status = value; NotifyPropertyChanged("Status"); } }
        }

        public void Connect()
        {
            this.Status = this.status == CONNECTED ? NOT_CONNECTED : CONNECTED;
        }
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
