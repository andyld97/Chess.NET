using Chess.NET.Model;

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
    }
}