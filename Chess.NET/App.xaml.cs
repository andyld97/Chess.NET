using System.Globalization;
using System.Windows;

namespace Chess.NET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if DEBUG
            // For english screenshots:

            //var cultureInfoTest = new CultureInfo("en-US");
            //Thread.CurrentThread.CurrentCulture =
            //Thread.CurrentThread.CurrentUICulture = cultureInfoTest;
#endif
        }
    }
}
