using System;

namespace Saxo.OpenAPI.AuthenticationServices
{
    class LoginException : Exception
    {
        public LoginException(string msg, Exception ex = null) : base(msg, ex)
        {
        }
    }
}
