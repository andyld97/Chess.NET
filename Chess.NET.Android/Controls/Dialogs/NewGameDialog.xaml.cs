namespace Chess.NET.Android.Controls.Dialogs;

public partial class NewGameDialog : ContentPage
{
    private readonly TaskCompletionSource<int> _tcs = new();

    public Task<int> WaitForResultAsync() => _tcs.Task;


    public int Result { get; private set; }

    public NewGameDialog()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();

        _tcs.SetResult(OpponentPicker.SelectedIndex);
    }
}