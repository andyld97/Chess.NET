using Chess.NET.Android.Controls.Dialogs;

namespace Chess.NET.Android
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void ButtonRestart_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NewGameDialog());
            Chessboard.Restart();
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            Chessboard.Mirror();
        }
    }
}
