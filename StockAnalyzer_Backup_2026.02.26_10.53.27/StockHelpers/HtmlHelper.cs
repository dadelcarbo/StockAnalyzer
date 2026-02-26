using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace StockAnalyzer.StockHelpers
{

    public static class HtmlHelper
    {
        static public string GenerateHtmlPage(string header, string htmlBody, string htmlTitle = null)
        {
            if (string.IsNullOrEmpty(htmlTitle))
            {
                htmlTitle = header;
            }
            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{header}</title>
    <style>

    body {{
        font-family: Arial, sans-serif;
        margin: 20px;
        background-color: #f9f9f9;
    }}
    h1 {{
        color: #333;
    }}
    table {{
        border-collapse: collapse;
        margin-top: 20px;
        font-size: 13px;
        table-layout: auto;
    }}
    th, td {{
        border: 1px solid #ddd;
        padding: 12px;
        text-align: left;
        font-size: 13px;
    }}
    th {{
        background-color: #007BFF;
        color: white;
        font-size: 15px;
    }}
    tr:nth-child(even) {{
        background-color: #f2f2f2;
    }}
    </style>
</head>
<body>
    <h2 class=""title"">{htmlTitle}</h2>       
    {htmlBody}
</body>
</html>";
        }

        public static string GenerateHtmlTable<T>(IEnumerable<T> data, IEnumerable<Expression<Func<T, object>>> propertySelectors)
        {
            var htmlTable = new StringBuilder();
            htmlTable.AppendLine("<table border='1'>");

            // Generate table header
            htmlTable.AppendLine("<tr>");
            foreach (var selector in propertySelectors)
            {
                // Assuming property names can be extracted from the selector
                var propertyName = GetPropertyName(selector).CapitalizeFirstCharacter();
                htmlTable.AppendLine($"<th>{System.Net.WebUtility.HtmlEncode(propertyName)}</th>");
            }
            htmlTable.AppendLine("</tr>");

            // Generate table rows
            foreach (var item in data)
            {
                htmlTable.AppendLine("<tr>");
                foreach (var selector in propertySelectors)
                {
                    var compiledSelector = selector.Compile();
                    var value = compiledSelector(item);
                    htmlTable.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(value?.ToString() ?? string.Empty)}</td>");
                }
                htmlTable.AppendLine("</tr>");
            }

            htmlTable.AppendLine("</table>");
            return htmlTable.ToString();
        }

        private static string GetPropertyName<T>(Expression<Func<T, object>> selector)
        {
            if (selector.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            else if (selector.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
            {
                return operand.Member.Name;
            }
            throw new InvalidOperationException("Selector must be a member expression");
        }

        public static string CapitalizeFirstCharacter(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Convert the first character to uppercase and concatenate it with the rest of the string
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}
