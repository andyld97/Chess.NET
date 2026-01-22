namespace Chess.NET.Model
{
    public class Move
    {
        public Piece Piece { get; set; } = null!;

        public Position From { get; set; } = null!;

        public Position To { get; set; } = null!;

        public bool IsCapture { get; set; }

        public bool IsCastleKingSide { get; set; }

        public bool IsCastleQueenSide { get; set; }

        public bool IsPromotion { get; set; }

        public bool IsCheck { get; set; }

        public bool IsCheckmate { get; set; }

        public bool IsStalemate { get; set; }

        public PieceType? PromotionType { get; set; }

        public int Count { get; set; } = 1;

        public string Disambiguation { get; set; } = string.Empty;

        public string ToUci()
        {
            if (IsPromotion && PromotionType.HasValue)
                return $"{From}{To}{PromotionType.Value.ToUciChar()}".ToLower();

            return $"{From}{To}".ToLower();
        }

        public string FormatMove(bool useEmojis)
        {
            // 1) Rochade
            if (IsCastleKingSide)
                return IsCheckmate ? "O-O#" : IsCheck ? "O-O+" : "O-O";

            if (IsCastleQueenSide)
                return IsCheckmate ? "O-O-O#" : IsCheck ? "O-O-O+" : "O-O-O";

            // 2️) Figurenbuchstabe
            string piece = Piece.Type switch
            {
                PieceType.Pawn => "",
                PieceType.Knight => "N",
                PieceType.Bishop => "B",
                PieceType.Rook => "R",
                PieceType.Queen => "Q",
                PieceType.King => "K",
                _ => ""
            };

            // oder emoji:
            if (useEmojis)
                piece = Helper.ToEmoji(Piece);

            // 3️) Bauernschlag braucht das File
            string pawnFile = Piece.Type == PieceType.Pawn && IsCapture ? From.ToString()[..1] : string.Empty;

            // 4️) Schlag
            string capture = IsCapture ? "x" : "";

            // 5️) Grundzug
            string result = $"{piece}{Disambiguation}{pawnFile}{capture}{To}";

            // 6️) Promotion
            if (IsPromotion)
                result += $"={PromotionType?.ToUciChar().ToString().ToUpper()}";

            // 7️ Schach / Matt / Patt
            if (IsCheckmate)
                result += "#";
            else if (IsCheck)
                result += "+";
            else if (IsStalemate)
                result += "$";

            return result;
        }

        public override string ToString()
        {
            return FormatMove(true);
        }
    }
}
