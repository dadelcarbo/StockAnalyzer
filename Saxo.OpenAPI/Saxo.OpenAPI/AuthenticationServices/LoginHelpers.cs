using Saxo.OpenAPI.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Saxo.OpenAPI.AuthenticationServices
{
    public static class LoginHelpers
    {
        private static Token token;
        public static Token Token => token;

        private static App app;
        public static App App => app;

        public static Token GoLogin(App app, HttpListener listener)
        {
            LoginHelpers.app = app;
            var authService = new PkceAuthService();
            var authUrl = authService.GetAuthenticationRequest(app);

            //System.Diagnostics.Process.Start(authUrl);
            authUrl = authUrl.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {authUrl}") { CreateNoWindow = true });

            var authCode = GetAuthCode(app, listener);
            if (authCode == null)
            {
                return null;
            }

            // Get Token
            LoginHelpers.token = authService.GetToken(app, authCode);
            return token;
        }
        public static Token GoLogin(App app, string _24hToken)
        {
            LoginHelpers.app = app;
            // Get Token
            LoginHelpers.token = new Token { AccessToken = _24hToken, TokenType = "Bearer"};
            return token;
        }

        public static Token RefreshToken(App app, string refreshToken, HttpListener listener)
        {
            var authService = new PkceAuthService();
            if (string.IsNullOrEmpty(refreshToken))
                throw new ArgumentException("Invalid refresh token");

            var token = authService.RefreshToken(app, refreshToken);

            return token;
        }

        private static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public static HttpListener BeginListening(App app)
        {
            HttpListener listener;
            try
            {
                var port = GetRandomUnusedPort();
                var uri = new Uri(app.RedirectUrls[0]);
                listener = new HttpListener();
                listener.Prefixes.Add($"{uri.Scheme}://{uri.Host}:{port}/");
                listener.Start();

                app.RedirectUrls[0] = uri.AbsoluteUri.Replace(uri.Host, uri.Host + ":" + port);
                return listener;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to start the listener for the redirect URL", ex);
            }
        }

        private static string GetAuthCode(App app, HttpListener listener)
        {
            // Listening
            HttpListenerContext httpContext = null;
            try
            {
                httpContext = listener.GetContext();
                foreach (var item in httpContext.Request.QueryString)
                {
                    Console.WriteLine($"Key: {item} Value:{httpContext.Request.QueryString[item.ToString()]}");
                }
                var authCode = httpContext.Request.QueryString["code"];
                using (var writer = new StreamWriter(httpContext.Response.OutputStream))
                {
                    if (authCode == null)
                    {
                        writer.WriteLine("Authentication failure, retry later...");
                    }
                    else
                    {
                        writer.WriteLine("Authentication success. Please close the browser.");
                    }
                    writer.Close();
                }

                return authCode;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get the authCode from URL", ex);
            }
            finally
            {
                if (httpContext != null)
                    httpContext.Response.Close();
            }
        }
    }
}
