using Newtonsoft.Json;
using StockAnalyzerSettings;
using System;
using System.IO;

namespace Saxo.OpenAPI.Models
{
    public class Token
    {
        [JsonProperty("creation_date")]
        public DateTime CreationDate { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("refresh_token_expires_in")]
        public int RefreshTokenExpiresIn { get; set; }

        public void Serialize(string clientId)
        {
            var tokenFileName = Path.Combine(Folders.Saxo, $"{clientId}.json");
            File.WriteAllText(tokenFileName, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        static public Token Deserialize(string clientId)
        {
            var tokenFileName = Path.Combine(Folders.Saxo, $"{clientId}.json");
            if (File.Exists(tokenFileName))
            {
                return JsonConvert.DeserializeObject<Token>(File.ReadAllText(tokenFileName));
            }
            return null;
        }

    }
}
