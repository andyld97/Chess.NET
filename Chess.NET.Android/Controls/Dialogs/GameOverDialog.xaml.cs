using Chess.NET.Shared.Model;
using Color = Chess.NET.Shared.Model.Color;

namespace Chess.NET.Android.Controls.Dialogs;

public partial class GameOverDialog : ContentPage
{
    public GameOverDialog(GameResult gameResult, Color? colorWon, string playerWon)
    {
        InitializeComponent();

        if (gameResult == GameResult.Checkmate || gameResult == GameResult.Resign || gameResult == GameResult.Disconnected || gameResult == GameResult.Timeout)
        {
            GridWin.IsVisible = true;
            GridDraw.IsVisible = false;

            switch (gameResult)
            {
                case GameResult.Checkmate: TextWinReason.Text = Chess.NET.Shared.Resources.strCheckmate; break;
                case GameResult.Resign: TextWinReason.Text = Chess.NET.Shared.Resources.strResignation; break;
                case GameResult.Disconnected: TextWinReason.Text = Chess.NET.Shared.Resources.strDisconnect; break;
                case GameResult.Timeout: TextWinReason.Text = Chess.NET.Shared.Resources.strTimeout; break;
            }

            TextColor.Text = colorWon == Color.Black ? Chess.NET.Shared.Resources.strBlack : Chess.NET.Shared.Resources.strWhite;
            if (!System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("de", StringComparison.CurrentCultureIgnoreCase))
                TextColor.Text = TextColor.Text.ToLower();

            ImgKing.Source = Helper.GetImage(Settings.Instance.Theme, PieceType.King, colorWon!.Value);
            TextPlayer.Text = playerWon;
        }
        else
        {
            GridWin.IsVisible = false;
            GridDraw.IsVisible = true;

            ImgKingWhite.Source = Helper.GetImage(Settings.Instance.Theme, PieceType.King, Color.White);
            ImgKingBlack.Source = Helper.GetImage(Settings.Instance.Theme, PieceType.King, Color.Black);

            switch (gameResult)
            {
                case GameResult.Stalemate: TextDrawReason.Text = Chess.NET.Shared.Resources.strStalemate; break;
                case GameResult.ThreefoldReptition: TextDrawReason.Text = Chess.NET.Shared.Resources.strThreefoldRepition; break;
                case GameResult.FiftyMoveRule: TextDrawReason.Text = Chess.NET.Shared.Resources.strFiftyMoveRule; break;
                case GameResult.InsufficentCheckmatingMaterial: TextDrawReason.Text = Chess.NET.Shared.Resources.strInsufficientCheckmatingMaterial; break;
            }
        }
    }

    private async void DialogHeader_CloseRequested(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
