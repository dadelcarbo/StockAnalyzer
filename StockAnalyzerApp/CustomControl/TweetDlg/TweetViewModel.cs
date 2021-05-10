using StockAnalyzer;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace StockAnalyzerApp.CustomControl.TweetDlg
{
    public delegate void TweetSentHandler();
    public class TweetViewModel : NotifyPropertyChangedBase
    {
        public TweetViewModel()
        {
        }

        string text;
        public string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                    this.OnPropertyChanged("Text");
                }
            }
        }

        string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    this.OnPropertyChanged("FileName");
                }
            }
        }

        public async Task SendTweetAsync()
        {
            try
            {
                var _credentials = new TwitterCredentials(oAuthConsumerKey, oAuthConsumerSecret, accessToken, accessTokenSecret);

                var fileBytes = File.ReadAllBytes(this.fileName);
                var client = new TwitterClient(_credentials);

                var publishTweetParameters = new PublishTweetParameters(this.text);

                if (fileBytes != null)
                {
                    var media = await client.Upload.UploadBinaryAsync(fileBytes);
                    publishTweetParameters.Medias.Add(media);
                }

                var tweet = await client.Tweets.PublishTweetAsync(publishTweetParameters);
                if (tweet != null)
                {
                    this.TweetSent?.Invoke();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
            }
        }

        public event TweetSentHandler TweetSent;


        private string oAuthConsumerKey = "vTMGphEDjwBCflVYANUt5sbMP";
        private string oAuthConsumerSecret = "8h40YUVy4RhcHtB2H4289sdStoZTXLRXe9zOPQpxqf3kpAdyOi";
        private string accessToken = "165906306-dHAtCCfd4khKBlzegtJhTr4NtCnY6vTLrcZ1TgTv";
        private string accessTokenSecret = "3WuVUBnLH7O3HnpSune1lomha1Syyl4kKY50l89H8DD2r";

    }
}
