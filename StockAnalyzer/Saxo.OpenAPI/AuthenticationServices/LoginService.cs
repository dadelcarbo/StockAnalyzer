using Saxo.OpenAPI.Models;
using Saxo.OpenAPI.TradingServices;
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

        public static LoginSession Login(string clientId, string appFolder, bool isSimu)
        {
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

                // Establish Session
                Clipboard.SetText(clientId);
                var token = LoginHelpers.GoLogin(session.App);
                if (token != null)
                {
                    token.Serialize(clientId);
                    session.Token = token;
                    CurrentSession = session;

                    // Verify clientId
                    var actualClientId = new ClientService().GetClient().ClientId;
                    if (actualClientId != clientId)
                    {
                        MessageBox.Show("Client ID mistmach, check portfolio file configuration", "Saxo Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                return session;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Saxo connection exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoginService.CurrentSession = null;
            return CurrentSession;
        }
    }
}
