using System;
using System.Collections.Generic;
using StockAnalyzer.StockClasses;

using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace StockAnalyzer.StockScripting
{
    public interface IStockFilter
    {
        bool MatchFilter(StockSerie stockSerie);
    }

    public class StockFilterManager
    {
        public StockFilterManager()
        {
        }

        private static string filterClassTemplate = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockScripting
{
    public class <FILTER_NAME>StockFilterImpl : IStockFilter
    {
        public bool MatchFilter(StockSerie stockSerie)
        {
            return (<FILTER_CODE>);
        }
    }
}";

        public static IStockFilter CreateStockFilterInstance(string stockFilterName, string stockFilterScript)
        {
            List<string> stockNameList = new List<string>();

            // Compile the script to execute
            string filterSource = filterClassTemplate.Replace("<FILTER_NAME>", stockFilterName).Replace("<FILTER_CODE>", stockFilterScript);

            Dictionary<string, string> providerOptions = new Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", "v3.5");
            CSharpCodeProvider codeProvider = new CSharpCodeProvider(providerOptions);

            System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
            //Make sure we generate an EXE, not a DLL
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;
            parameters.IncludeDebugInformation = true;
            parameters.ReferencedAssemblies.Add(@"System.dll");
            parameters.ReferencedAssemblies.Add(@"System.Core.dll");
            parameters.ReferencedAssemblies.Add(@"System.Data.DataSetExtensions.dll");
            parameters.ReferencedAssemblies.Add(@"System.Xml.dll");
            parameters.ReferencedAssemblies.Add(@"System.Xml.Linq.dll");
            parameters.ReferencedAssemblies.Add(@"StockAnalyzer.dll");

            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, filterSource);

            if (results.Errors.Count > 0)
            {
                string resultText = string.Empty;
                foreach (CompilerError CompErr in results.Errors)
                {
                    resultText = resultText +
                                "Line number " + CompErr.Line +
                                ", Error Number: " + CompErr.ErrorNumber +
                                ", '" + CompErr.ErrorText + ";" +
                                Environment.NewLine + Environment.NewLine;
                }
                throw new System.ArgumentException(resultText, "Compilation Error");
            }

            // Get the instance of the newly compiled code
            return (IStockFilter)results.CompiledAssembly.CreateInstance("StockAnalyzer.StockScripting." + stockFilterName + "StockFilterImpl");
        }
    }
}
