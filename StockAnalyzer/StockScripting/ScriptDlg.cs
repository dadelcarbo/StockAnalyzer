using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

// For live compilation
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace StockAnalyzerApp.StockScripting
{
    public partial class ScriptDlg : Form
    {
        public ScriptDlg()
        {
            InitializeComponent();
        }

        private void CompileBtn_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> providerOptions = new Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", "v3.5");
            CSharpCodeProvider codeProvider = new CSharpCodeProvider(providerOptions);

            resultTextBox.Text = "";
            System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
            //Make sure we generate an EXE, not a DLL
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;
            parameters.ReferencedAssemblies.Add(@"System.dll");
            parameters.ReferencedAssemblies.Add(@"System.Core.dll");
            parameters.ReferencedAssemblies.Add(@"System.Data.DataSetExtensions.dll");
            parameters.ReferencedAssemblies.Add(@"System.Xml.dll");
            parameters.ReferencedAssemblies.Add(@"System.Xml.Linq.dll");
            parameters.ReferencedAssemblies.Add(@"StockAnalyzer.exe");

            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, scriptTextBox.Text);

            if (results.Errors.Count > 0)
            {
                resultTextBox.ForeColor = Color.Red;
                foreach (CompilerError CompErr in results.Errors)
                {
                    resultTextBox.Text = resultTextBox.Text +
                                "Line number " + CompErr.Line +
                                ", Error Number: " + CompErr.ErrorNumber +
                                ", '" + CompErr.ErrorText + ";" +
                                Environment.NewLine + Environment.NewLine;
                }
            }
            else
            {
                //Successful Compile
                resultTextBox.ForeColor = Color.Blue;
                resultTextBox.Text = "Success!";
            }

            
        }
    }
}