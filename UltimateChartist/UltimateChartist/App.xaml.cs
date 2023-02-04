using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace UltimateChartist
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static App AppInstance { get; private set; }
        public App()
        {
            AppInstance = this;
            this.InitializeComponent();
        }
    }
}
