using Microsoft.CSharp;
using StockAnalyzer.StockHelpers;
using StockAnalyzerSettings;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace StockAnalyzer.StockScripting
{
    public class StockScriptManager
    {
        private static readonly string filterClassTemplate = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockScripting
{
    public class <FILTER_NAME>StockFilterImpl : StockFilterBase
    {
        protected override bool MatchFilter(StockSerie stockSerie, StockDailyValue bar)
        {
            <FILTER_CODE>
        }
    }
}";
        public CompilerResults CompilerResults;
        public IStockFilter CreateStockFilterInstance(string stockFilterName, string stockFilterScript)
        {
            // Set the culture to English (United States)
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");


            // Compile the script to execute
            string filterSource = filterClassTemplate.Replace("<FILTER_NAME>", stockFilterName).Replace("<FILTER_CODE>", stockFilterScript);

            Dictionary<string, string> providerOptions = new Dictionary<string, string>
            {
                { "CompilerVersion", "v3.5" }
            };
            CSharpCodeProvider codeProvider = new CSharpCodeProvider(providerOptions);

            CompilerParameters parameters = new CompilerParameters();
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

            CompilerResults = codeProvider.CompileAssemblyFromSource(parameters, filterSource);

            if (CompilerResults.Errors.Count > 0)
            {
                return null;
            }

            // Get the instance of the newly compiled code
            return (IStockFilter)CompilerResults.CompiledAssembly.CreateInstance("StockAnalyzer.StockScripting." + stockFilterName + "StockFilterImpl");
        }

        static StockScriptManager instance;
        public static StockScriptManager Instance => instance ??= new StockScriptManager();
        private StockScriptManager()
        {
            Persister<StockScript>.Instance.Initialize(Path.Combine(Folders.PersonalFolder, "Scripts"), "script");
            StockScripts = Persister<StockScript>.Instance.Items;
        }

        public ObservableCollection<StockScript> StockScripts { get; }
    }
}
