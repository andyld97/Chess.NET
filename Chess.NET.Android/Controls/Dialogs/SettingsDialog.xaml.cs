namespace Chess.NET.Android.Controls.Dialogs;

public partial class SettingsDialog : ContentPage
{
    public SettingsDialog()
    {
        InitializeComponent();
        Loaded += SettingsDialog_Loaded;
    }

    private void SettingsDialog_Loaded(object? sender, EventArgs e)
    {
        TextPlayer1.Text = Preferences.Get("Player1", "Player 1");
        TextPlayer2.Text = Preferences.Get("Player2", "Player 2");
    }

    private async void ButtonOK_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("Player1", TextPlayer1.Text);
        Preferences.Set("Player2", TextPlayer2.Text);
        await Navigation.PopModalAsync();
    }
}