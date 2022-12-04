using Saxo.OpenAPI.Models;
using Saxo.OpenAPI.TradingServices;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace Saxo.OpenAPI.AuthenticationServices
{
    class SaxoAPIException : Exception
    {
        public SaxoAPIException(string msg, Exception ex = null) : base(msg, ex)
        {
        }
    }
    public static class LoginHelpers
    {
        public static LoginSession CurrentSession { get; set; }

        public static App App
        {
            get
            {
                if (CurrentSession == null)
                {
                    throw new SaxoAPIException("Session now intialized, log in first");
                }
                return CurrentSession.App;
            }
        }
        public static Token Token
        {
            get
            {
                if (CurrentSession == null)
                {
                    throw new SaxoAPIException("Session now intialized, log in first");
                }
                return CurrentSession.Token;
            }
        }

        public static LoginSession GoLogin(App app, HttpListener listener)
        {
            try
            {
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
                var token = authService.GetToken(app, authCode);
                CurrentSession = new LoginSession
                {
                    App = app,
                    Token = token
                };
                CurrentSession.Client = new ClientService().GetClient();
                //Clipboard.SetText(token.AccessToken);
                return CurrentSession;
            }
            catch (Exception ex)
            {
                CurrentSession = null;
                throw new SaxoAPIException("Exception occured while loging in", ex);
            }
        }
        public static LoginSession GoLogin(App app, string _24hToken)
        {
            try
            {
                // Get Token
                var token = new Token { AccessToken = _24hToken, TokenType = "Bearer" };
                CurrentSession = new LoginSession
                {
                    App = app,
                    Token = token
                };
                CurrentSession.Client = new ClientService().GetClient();
                return CurrentSession;
            }
            catch (Exception ex)
            {
                CurrentSession = null;
                throw new SaxoAPIException("Exception occured while loging in", ex);
            }
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
