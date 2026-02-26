using Saxo.OpenAPI.Models;
using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Saxo.OpenAPI.AuthenticationServices
{
    public class LoginService
    {
        public static List<LoginSession> Sessions = new List<LoginSession>();

        public static LoginSession CurrentSession { get; set; }

        public static bool IsConnected => CurrentSession != null && !CurrentSession.HasTokenExpired();

        public static App App
        {
            get
            {
                if (CurrentSession == null)
                {
                    throw new LoginException("Session not intialized, log in first");
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
                    throw new LoginException("Session now intialized, log in first");
                }
                return CurrentSession.Token;
            }
        }

        public static void RefreshSessions()
        {
            using MethodLogger ml = new MethodLogger(typeof(LoginService), true);
            foreach (var session in Sessions.Where(s => s.HasTokenExpired()))
            {
                if (!session.HasRefreshTokenExpired())
                {
                    StockLog.Write($"Refreshing token for session {session.ClientId}");
                    var refreshToken = LoginHelpers.RefreshToken(session);
                    if (refreshToken != null)
                    {
                        refreshToken.Serialize(session.ClientId);
                        session.Token = refreshToken;
                    }
                }
                else
                {
                    StockLog.Write($"Refresh token for session {session.ClientId} expired !!!");
                }
            }
        }

        public static LoginSession Login(string clientId, string appFolder, bool isSimu)
        {
            using MethodLogger ml = new MethodLogger(typeof(LoginService), true, $"clientId:{clientId}");
            try
            {
                // Check if session already exists
                var session = Sessions.FirstOrDefault(s => s.ClientId == clientId);
                if (session == null)
                {
                    string appPath = Path.Combine(appFolder, isSimu ? "App_sim.json" : "App_live.json");
                    session = new LoginSession
                    {
                        App = App.GetApp(appPath),
                        ClientId = clientId,
                        Token = Token.Deserialize(clientId)
                    };
                    Sessions.Add(session);
                }
                if (!session.HasTokenExpired())
                {
                    CurrentSession = session;
                    return session;
                }
                else
                {
                    StockLog.Write($"ClientId:{clientId} Token has expired");
                }
                if (!session.HasRefreshTokenExpired())
                {
                    var refreshToken = LoginHelpers.RefreshToken(session);
                    if (refreshToken != null)
                    {
                        refreshToken.Serialize(clientId);
                        session.Token = refreshToken;
                        CurrentSession = session;
                        return session;
                    }
                }
                else
                {
                    StockLog.Write($"ClientId:{clientId} RefreshToken has expired");
                }

                // Establish Session
                try { Clipboard.SetText(clientId); } catch { }
                var token = LoginHelpers.GoLogin(session.App);
                if (token != null)
                {
                    CurrentSession = session;
                    session.Token = token;

                    // Verify clientId
                    var actualClientId = new ClientService().GetClient().ClientId;
                    token.Serialize(actualClientId);
                    if (actualClientId != clientId)
                    {
                        Sessions.Remove(session);
                        CurrentSession = null;

                        //var res = MessageBox.Show("Client ID mismatch, do you want to retry ?", "Saxo Connection Error", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                        var res = MessageBoxResult.OK;
                        if (res == MessageBoxResult.OK)
                        {
                            Login(clientId, appFolder, isSimu);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Saxo connection timeout", "Saxo Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    Sessions.Remove(session);
                    CurrentSession = null;
                }
                return CurrentSession;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Saxo connection exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoginService.CurrentSession = null;
            return CurrentSession;
        }

        public static LoginSession SilentLogin(string clientId, string appFolder, bool isSimu)
        {
            using MethodLogger ml = new MethodLogger(typeof(LoginService), true, $"clientId:{clientId}");
            try
            {
                // Check if session already exists
                var session = Sessions.FirstOrDefault(s => s.ClientId == clientId);
                if (session == null)
                {
                    string appPath = Path.Combine(appFolder, isSimu ? "App_sim.json" : "App_live.json");
                    session = new LoginSession
                    {
                        App = App.GetApp(appPath),
                        ClientId = clientId,
                        Token = Token.Deserialize(clientId)
                    };
                    if (!session.HasTokenExpired())
                    {
                        StockLog.Write($"ClientId:{clientId} Token is still valid");
                        Sessions.Add(session);
                        CurrentSession = session;
                        return session;
                    }
                    else
                    {
                        StockLog.Write($"ClientId:{clientId} Token has expired");
                    }
                    if (!session.HasRefreshTokenExpired())
                    {
                        StockLog.Write($"ClientId:{clientId} Refreshing token");
                        var refreshToken = LoginHelpers.RefreshToken(session);
                        if (refreshToken != null)
                        {
                            StockLog.Write($"ClientId:{clientId} token refreshed");
                            refreshToken.Serialize(clientId);
                            session.Token = refreshToken;
                            Sessions.Add(session);
                            CurrentSession = session;
                            return session;
                        }
                    }
                    else
                    {
                        StockLog.Write($"ClientId:{clientId} RefreshToken has expired");
                    }
                }
                else
                {
                    if (!session.HasTokenExpired())
                    {
                        CurrentSession = session;
                        return session;
                    }
                    else
                    {
                        StockLog.Write($"ClientId:{clientId} Token has expired");
                    }
                    if (!session.HasRefreshTokenExpired())
                    {
                        var refreshToken = LoginHelpers.RefreshToken(session);
                        if (refreshToken != null)
                        {
                            refreshToken.Serialize(clientId);
                            session.Token = refreshToken;
                            CurrentSession = session;
                            return session;
                        }
                    }
                    else
                    {
                        StockLog.Write($"ClientId:{clientId} RefreshToken has expired");
                    }
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return null;
        }
    }
}
