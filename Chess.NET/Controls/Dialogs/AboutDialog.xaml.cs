using System.Reflection;
using System.Windows;

namespace Chess.NET.Controls.Dialogs
{
    /// <summary>
    /// Interaktionslogik für AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
            TextService.Text = $"Powered by {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";
            TextCopyright.Text = typeof(AboutDialog).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
            TextVersion.Text = $"Version {typeof(AboutDialog).Assembly.GetName().Version!.ToString(3)}";
        }

        private void LnkHomepage_Click(object sender, RoutedEventArgs e)
        {
            Helper.OpenHyperlink(LnkHomepage.NavigateUri.ToString());
        }
    }
}
