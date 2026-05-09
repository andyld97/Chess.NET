namespace Chess.NET.Android.Controls.Dialogs;

public partial class NewGameDialog : ContentPage
{
    private readonly TaskCompletionSource<int> _tcs = new();

    public Task<int> WaitForResultAsync() => _tcs.Task;


    public int Result { get; private set; }

    public NewGameDialog()
	{
		InitializeComponent();
        Loaded += NewGameDialog_Loaded;
	}

    private void NewGameDialog_Loaded(object? sender, EventArgs e)
    {
        OpponentPicker.SelectedIndex = 0; //TODO später aus den Settings laden (lastSelectedOpponent)
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();

        _tcs.SetResult(OpponentPicker.SelectedIndex);
    }
}