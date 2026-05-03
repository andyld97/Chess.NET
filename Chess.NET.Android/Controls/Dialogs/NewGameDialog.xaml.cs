namespace Chess.NET.Android.Controls.Dialogs;

public partial class NewGameDialog : ContentPage
{
	public NewGameDialog()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		int selectedOption = OpponentPicker.SelectedIndex;


		if (selectedOption == 0)
		{
			// Search for an opponent online



		}



		await Navigation.PopAsync();

    }

}