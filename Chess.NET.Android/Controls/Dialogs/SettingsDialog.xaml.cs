namespace Chess.NET.Android.Controls.Dialogs;

public partial class SettingsDialog : ContentPage
{
    public delegate void onRefresh();
    public event onRefresh OnRefrsh;

    public SettingsDialog()
    {
        InitializeComponent();
        Loaded += SettingsDialog_Loaded;

        ThemePicker.ItemsSource = new List<string> { Shared.Resources.strTheme_Default, Shared.Resources.strTheme_Comic };

        if (Settings.Instance.Theme == Chess.NET.Shared.Model.Theme.Default)
            ThemePicker.SelectedItem = Shared.Resources.strTheme_Default;
        else if (Settings.Instance.Theme == Chess.NET.Shared.Model.Theme.Comic)
            ThemePicker.SelectedItem = Shared.Resources.strTheme_Comic;

        CmbBackground.ItemsSource = new List<string> { Shared.Resources.strBackground_None, Shared.Resources.strBackground_Sand, Shared.Resources.strBackground_Abstract, Shared.Resources.strBackground_Abstract_Purple };

        if (Settings.Instance.Background == Shared.Model.Background.None)
            CmbBackground.SelectedItem = Shared.Resources.strBackground_None;
        else if (Settings.Instance.Background == Shared.Model.Background.Abstract)
            CmbBackground.SelectedItem = Shared.Resources.strBackground_Abstract;
        else if (Settings.Instance.Background == Shared.Model.Background.AbstractPurple)
            CmbBackground.SelectedItem = Shared.Resources.strBackground_Abstract_Purple;
        else
            CmbBackground.SelectedItem = Shared.Resources.strBackground_Sand;
    }

    private void SettingsDialog_Loaded(object? sender, EventArgs e)
    {
        TextPlayer1.Text = Preferences.Get("Player1", Chess.NET.Android.Properties.Resources.strPlayer1);
        TextPlayer2.Text = Preferences.Get("Player2", Chess.NET.Android.Properties.Resources.strPlayer2);
        ChkAutoPromote.IsChecked = Settings.Instance.AutoPromoteToQueen;
        ChkPlaySounds.IsChecked = Settings.Instance.PlaySounds;
    }

    private async void ButtonOK_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("Player1", TextPlayer1.Text);
        Preferences.Set("Player2", TextPlayer2.Text);

        if (ThemePicker.SelectedItem is string theme)        
        {
            if (theme == Shared.Resources.strTheme_Default)
                Preferences.Set("Theme", Chess.NET.Shared.Model.Theme.Default.ToString());
            else if (theme == Shared.Resources.strTheme_Comic)
                Preferences.Set("Theme", Chess.NET.Shared.Model.Theme.Comic.ToString());
        }

        if (CmbBackground.SelectedItem is string background)
        {
            if (background == Shared.Resources.strBackground_None)
                Preferences.Set("Background", Chess.NET.Shared.Model.Background.None.ToString());
            else if (background == Shared.Resources.strBackground_Abstract)
                Preferences.Set("Background", Chess.NET.Shared.Model.Background.Abstract.ToString());
            else if (background == Shared.Resources.strBackground_Abstract_Purple)
                Preferences.Set("Background", Chess.NET.Shared.Model.Background.AbstractPurple.ToString());
            else if (background == Shared.Resources.strBackground_Sand)
                Preferences.Set("Background", Chess.NET.Shared.Model.Background.Sand.ToString());
        }

        Preferences.Set("AutoPromoteToQueen", ChkAutoPromote.IsChecked);
        Preferences.Set("PlaySounds", ChkPlaySounds.IsChecked);

        OnRefrsh?.Invoke();

        await Navigation.PopModalAsync();
    }

    private async void DialogHeader_CloseRequested(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}