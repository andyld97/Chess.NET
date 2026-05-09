namespace Chess.NET.Android.Controls.Dialogs;

public partial class WaitingQueueDialog : ContentPage
{
    public delegate void onWaitingQueueExited();
    public event onWaitingQueueExited? OnWaitingQueueExited; 

	public WaitingQueueDialog()
	{
		InitializeComponent();
	}

    protected override bool OnBackButtonPressed()
    {
        OnWaitingQueueExited?.Invoke();
        return base.OnBackButtonPressed();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        OnWaitingQueueExited?.Invoke();
        await Navigation.PopModalAsync();
    }
}