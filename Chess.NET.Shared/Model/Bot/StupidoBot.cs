using Chess.NET.Shared.Model.Pieces;

namespace Chess.NET.Shared.Model.Bot
{
    public class StupidoBot : IChessBot
    {
        public string Name => "Stupido";

        public int Elo => 42;

        public Color Color => Color.Black;

        public PendingMove Move(Game game)
        {
            List<SortedMove> sortedMoves = [];
            bool canCapture = false;

            // Man könnte noch überlegen:
            // - Bot should move hanging pieces!
            // - Bot could also be white, currently it is only considered as black!
            // - Bot could check if the move can be re-captured and check the resulting material balance

            // Bot sollte auf IsCheck prüfen
            if (game.IsCheck(Color))
            {
                // Was können wir tuen? Aus dem Schach gehen?
                // Es gibt 3 Möglichkeiten:

                // 1. Den König bewegen (ungerne)
                // 2. Eine andere Figur bewegen, um den Angriff zu blockieren
                // 3. Eine Figur opfern, um den angreifenden Gegner zu schlagen

                List<SortedMove> checkOutMoves = [];

                foreach (var p in game.Board.Pieces)
                {
                    if (p.Color != Color)
                        continue;

                    var possibleMoves = p.GetPossibleMoves(game);

                    foreach (var mv in possibleMoves)
                    {
                        if (!game.IsCheck(Color, p, mv))
                        {
                            var sm = new SortedMove() { Piece = p, TargetPosition = mv, Score = p.MaterialValue };

                            if (p is King)
                                sm.Score = -1000; // König bewegen ist die letzte Option

                            checkOutMoves.Add(sm);
                        }
                    }
                }

                if (checkOutMoves.Count > 0)
                {
                    var bestMove = checkOutMoves.OrderByDescending(m => m.Score).First();
                    return new PendingMove(bestMove.Piece, bestMove.TargetPosition);
                }
            }
            else
            {
                if (Color == Color.Black)
                {
                    if (game.CanCastle(Color.Black, new Position(3, 8)))
                    {
                        var king = game.Board.GetPiece(new Position(5, 8));
                        return new PendingMove(king!, new Position(3, 8));
                    }
                    else if (game.CanCastle(Color.Black, new Position(7, 8)))
                    {
                        var king = game.Board.GetPiece(new Position(5, 8));
                        return new PendingMove(king!, new Position(7, 8));
                    }
                }
                else
                {
                    if (game.CanCastle(Color.White, new Position(3, 1)))
                    {
                        var king = game.Board.GetPiece(new Position(5, 1));
                        return new PendingMove(king!, new Position(3, 1));
                    }
                    else if (game.CanCastle(Color.White, new Position(7, 1)))
                    {
                        var king = game.Board.GetPiece(new Position(5, 1));
                        return new PendingMove(king!, new Position(7, 1));
                    }
                }
            }           

            foreach (var p in game.Board.Pieces)
            {
                if (p.Color != Color)
                    continue;

                var possibleMoves = p.GetPossibleMoves(game);
                foreach (var mv in possibleMoves)
                {
                    var capturePiece = game.Board.GetPiece(mv);
                    if (capturePiece == null)
                    {
                        var sm = new SortedMove
                        {
                            Piece = p,
                            TargetPosition = mv,
                            Score = 0
                        };

                        if (game.IsCheck(Color.InvertColor(), p, mv))
                            sm.Score += 500;

                        if (p.Type == PieceType.King)
                            sm.Score *= -1;

                        sortedMoves.Add(sm);
                    }
                    else
                    {
                        if (capturePiece is King)
                            continue;

                        // Cannot caputre into check
                        if (game.IsCheck(Color, p, mv))
                            continue;

                        canCapture = true;
                        var sm = new SortedMove
                        {
                            Piece = p,
                            TargetPosition = mv,
                            Score = capturePiece.MaterialValue
                        };

                        if (game.IsCheck(Color.InvertColor(), p, mv))
                            sm.Score += 500;

                        sortedMoves.Add(sm);
                    }
                }
            }

            if (canCapture)
            {
                var result = sortedMoves.OrderByDescending(p => p.Score).FirstOrDefault();
                return new PendingMove(result!.Piece, result.TargetPosition);
            }
            else
            {
                var piece = GetRandom(game.Board.Pieces.Where(p => p.Color == Color.Black), p => p.GetPossibleMoves(game).Count > 0);
                var moves = piece.GetPossibleMoves(game);

                return new PendingMove(piece, moves[Random.Shared.Next(moves.Count)]);
            }
        }

        public static T GetRandom<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            var filtered = source.Where(predicate).ToList();

            if (filtered.Count == 0)
                throw new InvalidOperationException("No matching elements.");

            int index = Random.Shared.Next(filtered.Count);
            return filtered[index];
        }
    }

    class SortedMove
    {
        public Piece Piece { get; set; } = null!;

        public Position TargetPosition { get; set; } = null!;

        public int Score { get; set; }
    }
}
