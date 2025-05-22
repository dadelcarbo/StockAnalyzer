using Saxo.OpenAPI.Models;
using System;

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
