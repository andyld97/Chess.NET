using Chess.NET.Model;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Chess.NET
{
    public static class Helper
    {
        public static PieceColor InvertPieceColor(PieceColor pieceColor)
        {
            if (pieceColor == PieceColor.White)
                return PieceColor.Black;

            return PieceColor.White;
        }

        public static char ToUciChar(this PieceType type) => type switch
        {
            PieceType.Queen => 'q',
            PieceType.Rook => 'r',
            PieceType.Bishop => 'b',
            PieceType.Knight => 'n',
            _ => throw new InvalidOperationException("Invalid promotion piece")
        };

        public static string ToEmoji(this Piece piece)
        {
            return piece.Color switch
            {
                PieceColor.White => piece.Type switch
                {
                    PieceType.King => "♔",
                    PieceType.Queen => "♕",
                    PieceType.Rook => "♖",
                    PieceType.Bishop => "♗",
                    PieceType.Knight => "♘",
                    PieceType.Pawn => "♙",
                    _ => "?"
                },
                PieceColor.Black => piece.Type switch
                {
                    PieceType.King => "♚",
                    PieceType.Queen => "♛",
                    PieceType.Rook => "♜",
                    PieceType.Bishop => "♝",
                    PieceType.Knight => "♞",
                    PieceType.Pawn => "♟",
                    _ => "?"
                },
                _ => "?"
            };
        }
        public static string GetPlayerName(int player)
        {
            if (player == 1)
            {
                if (!string.IsNullOrEmpty(Settings.Instance.Player1Name))
                    return Settings.Instance.Player1Name;

                return Properties.Resources.strPlayer1;
            }
            else if (player == 2)
            {
                if (!string.IsNullOrEmpty(Settings.Instance.Player2Name))
                    return Settings.Instance.Player2Name;

                return Properties.Resources.strPlayer2;
            }

            return Properties.Resources.strPlayer1;
        }

        public static void OpenHyperlink(string url)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", $"\"{url}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.strFailedToOpenHyperlink, url, ex.Message), Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static Dictionary<PieceType, BitmapImage> bitmapCacheWhite = [];
        private static Dictionary<PieceType, BitmapImage> bitmapCacheBlack = [];

        public static BitmapImage ToBitmap(this PieceType pieceType, PieceColor color)
        {
            if (color == PieceColor.White && bitmapCacheWhite.TryGetValue(pieceType, out BitmapImage? value))
                return value;
            else if (color == PieceColor.Black && bitmapCacheBlack.TryGetValue(pieceType, out BitmapImage? value1))
                return value1;

            string col = (color == PieceColor.White ? "white" : "black");

            BitmapImage bi = new BitmapImage { CacheOption = BitmapCacheOption.OnLoad };
            bi.BeginInit();
            bi.UriSource = new Uri($"pack://application:,,,/Chess.NET;component/resources/icons/{col}/{pieceType}.png");
            bi.EndInit();
            bi.Freeze();

            // Add to cache
            if (color == PieceColor.White)
                bitmapCacheWhite.Add(pieceType, bi);
            else
                bitmapCacheBlack.Add(pieceType, bi);

            return bi;
        }
    }
}