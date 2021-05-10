using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace TweetApp
{
    public class TweeterAPI
    {
        private string oAuthConsumerKey = "vTMGphEDjwBCflVYANUt5sbMP";
        private string oAuthConsumerSecret = "8h40YUVy4RhcHtB2H4289sdStoZTXLRXe9zOPQpxqf3kpAdyOi";
        private string accessToken = "165906306-dHAtCCfd4khKBlzegtJhTr4NtCnY6vTLrcZ1TgTv";
        private string accessTokenSecret = "3WuVUBnLH7O3HnpSune1lomha1Syyl4kKY50l89H8DD2r";

        public async Task<ITweet> SendTweetAsync(string tweet, string filePath)
        {
            var _credentials = new TwitterCredentials(oAuthConsumerKey, oAuthConsumerSecret, accessToken, accessTokenSecret);

            var fileBytes = File.ReadAllBytes(filePath);
            var client = new TwitterClient(_credentials);

            var publishTweetParameters = new PublishTweetParameters(tweet);

            if (fileBytes != null)
            {
                var media = await client.Upload.UploadBinaryAsync(fileBytes);
                publishTweetParameters.Medias.Add(media);
            }

            return await client.Tweets.PublishTweetAsync(publishTweetParameters);
        }
    }

}
