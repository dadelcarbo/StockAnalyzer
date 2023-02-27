using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace UltimateChartist;

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

    protected override void OnStartup(StartupEventArgs e)
    {
        var culture = new CultureInfo("en-UK");
        culture.NumberFormat.CurrencySymbol = "€";
        culture.DateTimeFormat = new CultureInfo("fr-FR").DateTimeFormat;

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        base.OnStartup(e);
    }
}
