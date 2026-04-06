namespace Chess.NET.Android
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void ButtonRestart_Clicked(object sender, EventArgs e)
        {
            Chessboard.Restart();
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            Chessboard.Mirror();
        }
    }
}
