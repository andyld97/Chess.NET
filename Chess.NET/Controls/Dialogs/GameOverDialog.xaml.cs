using Chess.NET.Model;
using Chess.NET.Shared.Model;
using System.Windows;

namespace Chess.NET.Controls.Dialogs
{
    /// <summary>
    /// Interaktionslogik für GameOverDialog.xaml
    /// </summary>
    public partial class GameOverDialog : Window
    {
        public GameOverDialog(GameResult gameResult, Color? colorWon, string playerName)
        {
            InitializeComponent();

            if (gameResult == GameResult.Checkmate || gameResult == GameResult.Resign || gameResult == GameResult.Disconnected ||  gameResult == GameResult.Timeout)
            {
                GridWin.Visibility = Visibility.Visible;
                GridDraw.Visibility = Visibility.Collapsed;

                switch (gameResult)
                {
                    case GameResult.Checkmate:      TextWinReason.Text = Chess.NET.Shared.Resources.strCheckmate; break;
                    case GameResult.Resign:         TextWinReason.Text = Chess.NET.Shared.Resources.strResignation; break;
                    case GameResult.Disconnected:   TextWinReason.Text = Chess.NET.Shared.Resources.strDisconnect; break;
                    case GameResult.Timeout:        TextWinReason.Text = Chess.NET.Shared.Resources.strTimeout; break;
                }

                TextColor.Text = colorWon == Color.Black ? Chess.NET.Shared.Resources.strBlack : Chess.NET.Shared.Resources.strWhite;
                if (!System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("de", StringComparison.CurrentCultureIgnoreCase))
                    TextColor.Text = TextColor.Text.ToLower();

                ImgKing.Source = PieceType.King.ToBitmap(colorWon!.Value, Settings.Instance.Theme);
                TextPlayer.Text = playerName;
            }
            else
            {
                GridWin.Visibility = Visibility.Collapsed;
                GridDraw.Visibility = Visibility.Visible;
                Width = 630;

                ImgKingWhite.Source = PieceType.King.ToBitmap(Color.White, Settings.Instance.Theme);
                ImgKingBlack.Source = PieceType.King.ToBitmap(Color.Black, Settings.Instance.Theme);

                switch (gameResult)
                {
                    case GameResult.Stalemate:                          TextDrawReason.Text = Chess.NET.Shared.Resources.strStalemate; break;
                    case GameResult.ThreefoldReptition:                 TextDrawReason.Text = Chess.NET.Shared.Resources.strThreefoldRepition; break;
                    case GameResult.FiftyMoveRule:                      TextDrawReason.Text = Chess.NET.Shared.Resources.strFiftyMoveRule; break;
                    case GameResult.InsufficentCheckmatingMaterial:     TextDrawReason.Text = Chess.NET.Shared.Resources.strInsufficientCheckmatingMaterial; break;
                }
            }
        }
    }
}