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

        public bool IsCheck { get; set; }

        public bool IsCheckmate { get; set; }

        public bool IsStalemate { get; set; }

        public PieceType? PromotionType { get; set; }

        public int Count { get; set; } = 1;

        public string ToUci()
        {
            if (IsPromotion && PromotionType.HasValue)
                return $"{From}{To}{PromotionType.Value.ToUciChar()}".ToLower();

            return $"{From}{To}".ToLower();
        }

        public override string ToString()
        {
            // 1) Rochade
            if (IsCastleKingSide)
                return IsCheckmate ? "O-O#" : IsCheck ? "O-O+" : "O-O";

            if (IsCastleQueenSide)
                return IsCheckmate ? "O-O-O#" : IsCheck ? "O-O-O+" : "O-O-O";

            // 2️) Figurenbuchstabe
            string piece = PieceType switch
            {
                PieceType.Pawn => "",
                PieceType.Knight => "N",
                PieceType.Bishop => "B",
                PieceType.Rook => "R",
                PieceType.Queen => "Q",
                PieceType.King => "K",
                _ => ""
            };

            // 3️) Bauernschlag braucht das File
            string pawnFile = PieceType == PieceType.Pawn && IsCapture ? From.ToString().Substring(0,1) : "";

            // 4️) Schlag
            string capture = IsCapture ? "x" : "";

            // 5️) Grundzug
            string result = $"{piece}{pawnFile}{capture}{To}";

            // 6️) Promotion
            if (IsPromotion)
                result += $"={PromotionType?.ToUciChar().ToString().ToUpper()}"; // später auswählbar

            // 7️ Schach / Matt / Patt
            if (IsCheckmate)
                result += "#";
            else if (IsCheck)
                result += "+";
            else if (IsStalemate)
                result += "$";

            return result;
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
