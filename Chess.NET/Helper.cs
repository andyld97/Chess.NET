using Chess.NET.Model;
using System.Windows.Media.Imaging;

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