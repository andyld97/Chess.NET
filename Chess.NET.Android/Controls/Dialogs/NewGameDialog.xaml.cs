namespace Chess.NET.Android.Controls.Dialogs;

public partial class NewGameDialog : ContentPage
{
    private readonly TaskCompletionSource<(int, int)> _tcs = new();

    public Task<(int, int)> WaitForResultAsync() => _tcs.Task;

    public (int, int) Result { get; private set; }

    public NewGameDialog()
	{
		InitializeComponent();
        Loaded += NewGameDialog_Loaded;
	}

    private void NewGameDialog_Loaded(object? sender, EventArgs e)
    {
        OpponentPicker.SelectedIndex = Settings.Instance.LastSelectedGameMode;
        CmbTimeControl.SelectedIndex = Settings.Instance.LastSelectedTimeControl;

        OpponentPicker.Items.Add(Properties.Resources.strOnline);
        OpponentPicker.Items.Add(Properties.Resources.strBot);
        OpponentPicker.Items.Add(Properties.Resources.strPlayer2);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();

        int selectedIndex = OpponentPicker.SelectedIndex;
        Preferences.Set("LastSelectedGameMode", selectedIndex);
        Preferences.Set("LastSelectedTimeControl", CmbTimeControl.SelectedIndex);
        _tcs.SetResult((selectedIndex, CmbTimeControl.SelectedIndex));
    }

    private async void DialogHeader_CloseRequested(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}