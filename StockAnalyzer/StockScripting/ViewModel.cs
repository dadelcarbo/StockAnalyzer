using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace StockAnalyzer.StockScripting
{
    public class ViewModel : NotifyPropertyChangedBase
    {
        public ObservableCollection<StockScript> Scripts => StockScriptManager.Instance.StockScripts;

        private const string BUILD_SUCCEEDED = "Build succeeded";
        private StockScript script;
        public StockScript Script
        {
            get => script; set
            {
                SetProperty(ref script, value);
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(Code));
                this.Errors = string.Empty;
            }
        }


        private string errors;
        public string Errors { get => errors; set => SetProperty(ref errors, value); }

        public string Description { get => script?.Description; set { if (value != script.Description) { script.Description = value; OnPropertyChanged(); } } }

        public string Name { get => script?.Name; set { if (value != script.Name) { script.Name = value; OnPropertyChanged(); } } }
        public string Code { get => script?.Code; set { if (value != script.Code) { script.Code = value; OnPropertyChanged(); } } }


        private CommandBase addCommand;
        public ICommand AddCommand => addCommand ??= new CommandBase(Add);

        private void Add()
        {
            this.Script = new StockScript() { Name = "New", Code = "return true;" };
        }
        private CommandBase deleteCommand;
        public ICommand DeleteCommand => deleteCommand ??= new CommandBase(Delete, CanDelete, this, new[] { nameof(Script) });

        private void Delete()
        {
            StockScriptManager.Instance.Delete(this.script.Name);
            this.Script = this.Scripts.FirstOrDefault();
        }
        private bool CanDelete()
        {
            return this.script != null && this.Scripts.Contains(script);
        }

        private CommandBase saveCommand;
        public ICommand SaveCommand => saveCommand ??= new CommandBase(Save, CanSave, this, new[] { nameof(Script), nameof(Errors) });


        private void Save()
        {
            StockScriptManager.Instance.Save(this.script);
            this.Errors = string.Empty;
        }
        private bool CanSave()
        {
            return this.Script != null && this.Errors == BUILD_SUCCEEDED;
        }


        private CommandBase compileCommand;
        public ICommand CompileCommand => compileCommand ??= new CommandBase(Compile, CanCompile, this, new[] { nameof(Script) });

        private bool CanCompile()
        {
            return !string.IsNullOrEmpty(this.Script?.Code);
        }

        private void Compile()
        {
            this.Errors = "Build Started";
            var filter = StockScriptManager.Instance.CreateStockFilterInstance(this.Script);
            string output = string.Empty;
            if (filter == null)
            {
                foreach (CompilerError compErr in StockScriptManager.Instance.CompilerResults.Errors)
                {
                    if (compErr.IsWarning)
                    {
                        output += "Line number " + compErr.Line +
                                    ", Warning Number: " + compErr.ErrorNumber +
                                    ", '" + compErr.ErrorText + ";" +
                                    Environment.NewLine;
                    }
                    else
                    {
                        output += "Line number " + compErr.Line +
                                    ", Error Number: " + compErr.ErrorNumber +
                                    ", '" + compErr.ErrorText + ";" +
                                    Environment.NewLine;
                    }
                }
            }
            else
            {
                output += BUILD_SUCCEEDED;
            }
            this.Errors = output;
        }
    }
}
