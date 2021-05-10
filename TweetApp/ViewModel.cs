using System;
using System.ComponentModel;
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
            var api = new TweeterAPIOld();
            api.SendTweet("Test message");

            this.Status = this.status == CONNECTED ? NOT_CONNECTED : CONNECTED;
        }
        public async Task SendTweetAsync(string text, string filePath)
        {
            var api = new TweeterAPI();
            try
            {
                var tweet = await api.SendTweetAsync(text, filePath);

                this.Status = "Publish successful: Id " + tweet.Id;
            }
            catch (Exception e)
            {
                this.Status = e.Message;
            }
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
