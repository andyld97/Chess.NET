using System.Globalization;
using System.Windows;
using System.Windows.Threading;

namespace Chess.NET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Dispatcher UiDispatcher { get; private set; } = null!;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            UiDispatcher = Dispatcher.CurrentDispatcher;
#if DEBUG
            // For english screenshots:

            var cultureInfoTest = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture = cultureInfoTest;
#endif
        }
    }
}
