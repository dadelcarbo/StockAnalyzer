using Saxo.OpenAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Saxo.OpenAPI.AuthenticationServices
{
    public class LoginService
    {
        public static List<LoginSession> Sessions = new List<LoginSession>();

        public static LoginSession Login(string clientId, string appPath)
        {
            try
            {
                // Check if session already exists
                if (LoginHelpers.CurrentSession != null && LoginHelpers.CurrentSession.Client.ClientId == clientId)
                    return LoginHelpers.CurrentSession;

                var session = Sessions.FirstOrDefault(s => s.Client?.ClientId == clientId);
                if (session != null)
                {
                    LoginHelpers.CurrentSession = session;
                    // TODO check token renewal
                    return session;
                }

                // Establish Session
                var app = App.GetApp(appPath);
                if (string.IsNullOrEmpty(app._24hToken))
                {
                    // Open Listener for Redirect
                    var listener = LoginHelpers.BeginListening(app);

                    // Get token and call API
                    session = LoginHelpers.GoLogin(app, listener);
                }
                else
                {
                    session = LoginHelpers.GoLogin(app, app._24hToken);
                }
                if (session != null)
                {
                    if (session.Client.ClientId != clientId)
                    {
                        throw new SaxoAPIException("Client ID mistmach");
                    }
                    Sessions.Add(session);
                }
                return session;
            }
            catch(Exception ex)
            {
                throw new SaxoAPIException("Login exception", ex);
            }
        }
    }
}
