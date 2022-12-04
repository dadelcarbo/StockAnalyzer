using Saxo.OpenAPI.Models;
using Saxo.OpenAPI.TradingServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saxo.OpenAPI.AuthenticationServices
{
    public class LoginSession
    {
        public App App { get; set; }
        public Token Token { get; set; }
        public Client Client { get; set; }
    }
}
