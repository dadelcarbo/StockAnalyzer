using Saxo.OpenAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Saxo.OpenAPI.AuthenticationServices
{
    public class LoginService
    {
        public static Token Login(string appPath)
        {
            var app = App.GetApp(appPath);
            if (string.IsNullOrEmpty(app._24hToken))
            {
                // Open Listener for Redirect
                var listener = LoginHelpers.BeginListening(app);

                // Get token and call API
                Console.WriteLine("Getting Token... ");
                return LoginHelpers.GoLogin(app, listener);
            }
            else
            {
                return LoginHelpers.GoLogin(app, app._24hToken);
            }
        }
    }
}
