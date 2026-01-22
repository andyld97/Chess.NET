namespace Chess.NET.Model
{
    public class PendingMove
    {
        public Piece Piece { get; set; }

        public Position To { get; set; }

        public PieceType? PromotionType { get; set; } = null;

        public PendingMove(Piece piece, Position to, PieceType? promotionType = null)
        {
            Piece = piece;
            To = to;
            PromotionType = promotionType;
        }

        public static PendingMove? Parse(string san, Board board, PieceColor color)
        {
            // 1) Remove check / checkmate markers
            san = san.TrimEnd('+', '#');

            // 2) Promotion handling (e8=Q)
            PieceType? promotionType = null;
            int promoIndex = san.IndexOf('=');
            if (promoIndex >= 0)
            {
                promotionType = san[promoIndex + 1] switch
                {
                    'Q' => PieceType.Queen,
                    'R' => PieceType.Rook,
                    'B' => PieceType.Bishop,
                    'N' => PieceType.Knight,
                    _ => null
                };

                san = san[..promoIndex]; // cut off "=Q"
            }

            // 3) Destination square (last two chars)
            if (san.Length < 2)
                return null;

            string toStr = san[^2..];
            if (toStr[0] < 'a' || toStr[0] > 'h' || toStr[1] < '1' || toStr[1] > '8')
                return null;

            int toFile = toStr[0] - 'a' + 1;
            int toRank = toStr[1] - '0';
            var to = new Position(toFile, toRank);

            string core = san[..^2]; // everything before destination

            // 4) Capture?
            bool isCapture = core.Contains('x');
            core = core.Replace("x", "");

            // 5) Determine piece type
            PieceType pieceType = PieceType.Pawn;
            int index = 0;

            if (core.Length > 0 && char.IsUpper(core[0]))
            {
                pieceType = core[0] switch
                {
                    'N' => PieceType.Knight,
                    'B' => PieceType.Bishop,
                    'R' => PieceType.Rook,
                    'Q' => PieceType.Queen,
                    'K' => PieceType.King,
                    _ => PieceType.Pawn
                };

                index = 1; // skip piece char
            }

            // 6) Read single-character disambiguation (file or rank)
            char? disambiguation = null;

            if (index < core.Length)
                disambiguation = core[index];

            // 7) Find all candidate pieces of that type that can move to the target
            var candidates = board.Pieces
                .Where(p => p.Color == color && p.Type == pieceType)
                .Where(p => p.GetPossibleMoves(board).Any(m => m == to))
                .ToList();

            // 8) Apply disambiguation if present
            if (disambiguation.HasValue)
            {
                char d = disambiguation.Value;

                if (d >= 'a' && d <= 'h')
                {
                    int file = d - 'a' + 1;
                    candidates = candidates.Where(p => p.Position.File == file).ToList();
                }
                else if (d >= '1' && d <= '8')
                {
                    int rank = d - '0';
                    candidates = candidates.Where(p => p.Position.Rank == rank).ToList();
                }
            }

            // 9) Must resolve to exactly one piece
            if (candidates.Count != 1)
                return null;

            var piece = candidates[0];

            return new PendingMove(piece, to) { PromotionType = promotionType };
        }
    }
}