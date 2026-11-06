using System.Reflection;

namespace Chess.NET.Android.Controls.Dialogs;

public partial class AboutDialog : ContentPage
{
	public AboutDialog()
	{
		InitializeComponent();
        TextService.Text = $"Powered by {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";
        TextCopyright.Text = typeof(AboutDialog).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
        TextVersion.Text = $"Version {typeof(AboutDialog).Assembly.GetName().Version!.ToString(3)}";
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Launcher.OpenAsync("https://ca-soft.net");
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Launcher.OpenAsync("https://github.com/andyld97/Chess.NET");
    }

    private async void TapGestureRecognizer_Tapped_2(object sender, TappedEventArgs e)
    {
        await Launcher.OpenAsync("https://linktr.ee/lisydra");
    }

    private async void DialogHeader_CloseRequested(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}