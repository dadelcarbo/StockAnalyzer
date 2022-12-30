using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.Saxo.OpenAPI.TradingServices
{
    public class SaxoApiException : Exception
    {
        public ErrorInfo ErrorInfo {get;set;}

        public override string Message => ErrorInfo == null ? base.Message : this.ErrorInfo.Message;
    }

    public class ErrorInfo
    {
        public string ErrorCode { get; set; }
        public string Message { get; set; }
    }
    public class SaxoErrorInfo
    {
        public ErrorInfo ErrorInfo { get; set; }
    }
}
