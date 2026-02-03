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
                    case GameResult.Checkmate:      TextWinReason.Text = Properties.Resources.strCheckmate; break;
                    case GameResult.Resign:         TextWinReason.Text = Properties.Resources.strResignation; break;
                    case GameResult.Disconnected:   TextWinReason.Text = Properties.Resources.strDisconnect; break;
                    case GameResult.Timeout:        TextWinReason.Text = Properties.Resources.strTimeout; break;
                }

                TextColor.Text = colorWon == Color.Black ? Properties.Resources.strBlack : Properties.Resources.strWhite;
                if (!System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("de", StringComparison.CurrentCultureIgnoreCase))
                    TextColor.Text = TextColor.Text.ToLower();

                ImgKing.Source = PieceType.King.ToBitmap(colorWon!.Value);
                TextPlayer.Text = playerName;
            }
            else
            {
                GridWin.Visibility = Visibility.Collapsed;
                GridDraw.Visibility = Visibility.Visible;
                Width = 430;

                switch (gameResult)
                {
                    case GameResult.Stalemate:                          TextDrawReason.Text = Properties.Resources.strStalemate; break;
                    case GameResult.ThreefoldReptition:                 TextDrawReason.Text = Properties.Resources.strThreefoldRepition; break;
                    case GameResult.FiftyMoveRule:                      TextDrawReason.Text = Properties.Resources.strFiftyMoveRule; break;
                    case GameResult.InsufficentCheckmatingMaterial:     TextDrawReason.Text = Properties.Resources.strInsufficientCheckmatingMaterial; break;
                }
            }
        }
    }
}