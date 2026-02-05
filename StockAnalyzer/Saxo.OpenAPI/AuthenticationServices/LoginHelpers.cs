using Saxo.OpenAPI.Models;
using StockAnalyzer.StockLogging;
using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Saxo.OpenAPI.AuthenticationServices
{
    public static class LoginHelpers
    {
        public static Token GoLogin(App app)
        {
            using MethodLogger ml = new MethodLogger(typeof(LoginHelpers), true);
            try
            {
                Token token = null;
                if (string.IsNullOrEmpty(app._24hToken))
                {
                    // Open Listener for Redirect
                    var listener = LoginHelpers.BeginListening(app);
                    var authService = new PkceAuthService();
                    var authUrl = authService.GetAuthenticationRequest(app);

                    if (Environment.UserName == "r395930")
                    {
                        var chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
                        var psi = new ProcessStartInfo
                        {
                            FileName = chromePath,
                            Arguments = authUrl,
                            UseShellExecute = false
                        };
                        Process.Start(psi);
                    }
                    else
                    {
                        authUrl = authUrl.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {authUrl}") { CreateNoWindow = false });
                    }

                    var authCode = GetAuthCode(app, listener);
                    if (authCode == null)
                    {
                        return null;
                    }

                    // Get Token
                    token = authService.GetToken(app, authCode);
                }
                else
                {
                    // Reuse 24h Token
                    token = new Token { AccessToken = app._24hToken, TokenType = "Bearer" };
                }
                return token;
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
                throw new LoginException("Exception occured while loging in", ex);
            }
        }

        public static Token RefreshToken(LoginSession session)
        {
            Token token = null;
            try
            {
                using MethodLogger ml = new MethodLogger(typeof(LoginHelpers), true);
                var authService = new PkceAuthService();
                if (string.IsNullOrEmpty(session.Token.RefreshToken))
                    throw new ArgumentException("Invalid refresh token");

                token = authService.RefreshToken(session.App, session.Token.RefreshToken);
                token.CreationDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
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
            using MethodLogger ml = new MethodLogger(typeof(LoginHelpers), true);
            // Listening
            HttpListenerContext httpContext = null;
            try
            {
                int timeout = 60000; // Milliseconds
                var task = listener.GetContextAsync();
                if (Task.WhenAny(task, Task.Delay(timeout)).Result == task)
                {
                    // task completed within timeout
                    httpContext = task.Result;
                }
                else
                {
                    // timeout logic
                    return null;
                }

                foreach (var item in httpContext.Request.QueryString)
                {
                    StockLog.Write($"Key: {item} Value:{httpContext.Request.QueryString[item.ToString()]}");
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
                StockLog.Write(ex);
                throw new Exception("Failed to get the authCode from URL", ex);
            }
            finally
            {
                httpContext?.Response.Close();
            }
        }
    }
}
