﻿using Microsoft.CSharp;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockHelpers;
using StockAnalyzerSettings;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace StockAnalyzer.StockScripting
{
    public class StockScriptManager
    {
        private static readonly string filterClassTemplate = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;

namespace StockAnalyzer.StockScripting
{
    public class <FILTER_NAME>StockFilterImpl : StockFilterBase
    {
        protected override bool MatchFilter(StockSerie stockSerie, StockDailyValue bar, int index)
        {
            <FILTER_CODE>
        }
    }
}";

        private bool MatchFilter(StockSerie stockSerie, StockDailyValue bar, int index)
        {
            return stockSerie.GetTrailStop("TRAILATR(75,35,1.25,0.25,EMA)").GetEvents("Consolidation")[index];

            if (bar.VARIATION < 0)
                return false;

            int period = 35;

            var maxRor = stockSerie.GetIndicator("MAX(ROR(" + period + ")," + period + ")").Series[0][index];
            if (maxRor < 0.3)
                return false;

            var stoch = stockSerie.CalculateLastFastOscillator(period * 2, InputType.Close);
            if (stoch < 61)
                return false;

            var highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            var highPivot = highSerie.FindMax(stockSerie.LastIndex - period, stockSerie.LastIndex);

            var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            var lowPivot = lowSerie.FindMin(highPivot.Index, stockSerie.LastIndex);

            var longRange = highPivot.Value - lowPivot.Value;
            var shortRange = stockSerie.CalculateLastRange(2, InputType.Body);

            if (longRange / shortRange < 10.0f)
                return false;

            return true;
        }


        public CompilerResults CompilerResults { get; private set; }
        public IStockFilter CreateStockFilterInstance(StockScript stockScript)
        {
            // Set the culture to English (United States)
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");


            // Compile the script to execute
            string filterSource = filterClassTemplate.Replace("<FILTER_NAME>", stockScript.Name).Replace("<FILTER_CODE>", stockScript.Code);

            Dictionary<string, string> providerOptions = new Dictionary<string, string>
            {
            };
            CSharpCodeProvider codeProvider = new CSharpCodeProvider(providerOptions);

            CompilerParameters parameters = new CompilerParameters();
            //Make sure we generate an EXE, not a DLL
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;
            parameters.IncludeDebugInformation = false;
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
            return (IStockFilter)CompilerResults.CompiledAssembly.CreateInstance("StockAnalyzer.StockScripting." + stockScript.Name + "StockFilterImpl");
        }

        static private SortedDictionary<string, IStockFilter> compilerCache = new SortedDictionary<string, IStockFilter>();
        public IStockFilter CreateStockFilterInstance(string name)
        {
            if (compilerCache.ContainsKey(name))
                return compilerCache[name];

            var stockFilter = StockScripts?.FirstOrDefault(s => s.Name == name);
            if (stockFilter == null)
                return null;

            var filter = CreateStockFilterInstance(stockFilter);
            if (filter != null)
                compilerCache.Add(name, filter);

            return filter;
        }

        static StockScriptManager instance;
        public static StockScriptManager Instance => instance ??= new StockScriptManager();
        private StockScriptManager()
        {
            Persister<StockScript>.Instance.Initialize(Path.Combine(Folders.PersonalFolder, "Scripts"), "script");
            StockScripts = Persister<StockScript>.Instance.Items;
        }

        public void Delete(string name)
        {
            Persister<StockScript>.Instance.Delete(name);
        }

        internal void Save(StockScript script)
        {
            Persister<StockScript>.Instance.Save(script);
        }

        public ObservableCollection<StockScript> StockScripts { get; }
    }
}
