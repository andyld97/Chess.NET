using Chess.NET.Model.Pieces;

namespace Chess.NET.Model
{
    public class Move
    {
        public PieceType PieceType { get; set; }

        public Position From { get; set; } = null!;

        public Position To { get; set; } = null!;

        public bool IsCapture { get; set; }

        public PieceColor PieceColor { get; set; }

        public bool IsCastleKingSide { get; set; }

        public bool IsCastleQueenSide { get; set; }

        public bool IsPromotion { get; set; }

        public PieceType? PromotionType { get; set; }

        public string ToUci()
        {
            if (IsPromotion && PromotionType.HasValue)
                return $"{From}{To}{PromotionType.Value.ToUciChar()}".ToLower();

            return $"{From}{To}".ToLower();
        }

        public override string ToString()
        {
            string chessNotation = PieceType switch
            {
                PieceType.Pawn => "",
                PieceType.Knight => "N",
                PieceType.Bishop => "B",
                PieceType.Rook => "R",
                PieceType.Queen => "Q",
                PieceType.King => "K",
                _ => ""
            };

            string fromNotation = From.ToString();
            string toNotation = To.ToString();  

            string captureNotation = IsCapture ? "x" : "";

            if (IsCastleKingSide)
                return "O-O";
            else if (IsCastleQueenSide)
                return "O-O-O";

            return $"{chessNotation}{fromNotation}{captureNotation}{toNotation}";

        }

    }

    public static class PieceTypeExtensions
    {
        public static char ToUciChar(this PieceType type) => type switch
        {
            PieceType.Queen => 'q',
            PieceType.Rook => 'r',
            PieceType.Bishop => 'b',
            PieceType.Knight => 'n',
            _ => throw new InvalidOperationException("Invalid promotion piece")
        };
    }
}
