using Newtonsoft.Json;
using System.IO;

namespace Saxo.OpenAPI.Models
{
    public class App
    {
        public string AppName { get; set; }

        public string AppKey { get; set; }

        public string CodeVerifier { get; set; }

        public string AuthorizationEndpoint { get; set; }

        public string TokenEndpoint { get; set; }

        public string GrantType { get; set; }

        public string OpenApiBaseUrl { get; set; }

        public string[] RedirectUrls { get; set; }

        public string _24hToken { get; set; }

        public static App GetApp(string path)
        {
            var content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<App>(content);
        }
    }
}
