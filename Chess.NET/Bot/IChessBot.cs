using Chess.NET.Model;
using Chess.NET.Model.Pieces;

namespace Chess.NET.Bot
{
    public interface IChessBot
    {
        NextMove? Move(Game game);

        int Elo { get; }

        string Name { get; }
    }

    public class NextMove
    {
        public Piece Piece { get; set; }

        public Position To { get; set; }

        public PieceType? PromotionType { get; set; } = null;

        public NextMove(Piece piece, Position to, PieceType? promotionType = null)
        {
            Piece = piece;
            To = to;
            PromotionType = promotionType;
        }

        public static NextMove Parse(string move, Board board, PieceColor pieceColor)
        {
            // Remove Check / Checkmate
            move = move.TrimEnd('+', '#');

            // Promotion?
            PieceType? promotionType = null;
            int promoIndex = move.IndexOf('=');
            if (promoIndex >= 0)
            {
                char promoChar = move[promoIndex + 1];
                promotionType = promoChar switch
                {
                    'Q' => PieceType.Queen,
                    'R' => PieceType.Rook,
                    'B' => PieceType.Bishop,
                    'N' => PieceType.Knight,
                    _ => null
                };

                move = move[..promoIndex]; // "=Q" cut off
            }

            // Last two chars are the destionation square
            string toStr = move[^2..];
            int toFile = toStr[0] - 'a' + 1;
            int toRank = toStr[1] - '0';
            var to = new Position(toFile, toRank);

            string core = move[..^2]; // alles vor dem Ziel

            bool isCapture = core.Contains('x');
            core = core.Replace("x", "");

            // 1) Figur bestimmen
            PieceType pieceType;
            int index = 0;

            char first = core.Length > 0 ? core[0] : '\0';

            if (char.IsUpper(first) && first is not ('O'))
            {
                pieceType = first switch
                {
                    'N' => PieceType.Knight,
                    'B' => PieceType.Bishop,
                    'R' => PieceType.Rook,
                    'Q' => PieceType.Queen,
                    'K' => PieceType.King,
                    _ => PieceType.Pawn
                };

                index = 1; // Figurenbuchstabe verbraucht
            }
            else
            {
                pieceType = PieceType.Pawn;
            }

            // 2) Disambiguation auslesen (0–2 Zeichen)
            char? disFile = null;
            int? disRank = null;

            while (index < core.Length)
            {
                char c = core[index];

                if (c >= 'a' && c <= 'h')
                    disFile = c;
                else if (c >= '1' && c <= '8')
                    disRank = c - '0';

                index++;
            }

            // 3) Kandidaten suchen
            var candidates = board.Pieces
                .Where(p => p.Color == pieceColor && p.Type == pieceType)
                .Where(p => p.GetPossibleMoves(board).Any(m => m == to)).ToList();

            // 4) Disambiguation anwenden
            if (disFile.HasValue)
            {
                int f = disFile.Value - 'a' + 1;
                candidates = candidates.Where(p => p.Position.File == f).ToList();
            }

            if (disRank.HasValue)
                candidates = candidates.Where(p => p.Position.Rank == disRank.Value).ToList();

            if (candidates.Count != 1)
                return null; // illegal oder mehrdeutig

            var piece = candidates[0];

            return new NextMove(piece, to) { PromotionType = promotionType };
        }
    }
}