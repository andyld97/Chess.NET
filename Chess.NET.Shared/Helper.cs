using Chess.NET.Shared.Model;

namespace Chess.NET.Shared
{
    public static class Helper
    {
        public static Color InvertColor(this Color pieceColor)
        {
            if (pieceColor == Color.White)
                return Color.Black;

            return Color.White;
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
                Color.White => piece.Type switch
                {
                    PieceType.King => "♔",
                    PieceType.Queen => "♕",
                    PieceType.Rook => "♖",
                    PieceType.Bishop => "♗",
                    PieceType.Knight => "♘",
                    PieceType.Pawn => "♙",
                    _ => "?"
                },
                Color.Black => piece.Type switch
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
    }
}