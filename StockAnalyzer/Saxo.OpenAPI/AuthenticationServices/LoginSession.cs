using Saxo.OpenAPI.Models;
using Saxo.OpenAPI.TradingServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Saxo.OpenAPI.AuthenticationServices
{
    public class LoginSession
    {
        public App App { get; set; }
        public Token Token { get; set; }
        public string ClientId { get; set; }

        public bool HasTokenExpired()
        {
            if (Token == null)
                return true;
            return Token.CreationDate.AddSeconds(Token.ExpiresIn) <= DateTime.Now;
        }
        public bool HasRefreshTokenExpired()
        {
            if (Token == null)
                return true;
            return Token.CreationDate.AddSeconds(Token.RefreshTokenExpiresIn) <= DateTime.Now;
        }
    }
}
