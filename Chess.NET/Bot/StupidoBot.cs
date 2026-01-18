using Chess.NET.Model;
using Chess.NET.Model.Pieces;

namespace Chess.NET.Bot
{
    public class StupidoBot : IChessBot
    {
        public string Name => "Stupido";

        public int Elo => 42;

        public NextMove? Move(Game game)
        {
            List<SortedMove> sortedMoves = [];
            bool canCapture = false;

            // Man könnte noch überlegen:
            // - Bot soll Schach geben, wenn möglich
            // - Bot soll Figuren nur setzten, wenn er nicht direkt geschlagen wird oder tauschen kann
            // - Bot soll rochieren, wenn es geht!

            // Bot sollte auf IsCheck zugreifen
            if (game.IsCheck(PieceColor.Black))
            {
                // Was können wir tuen? Aus dem Schach gehen?
                // Es gibt 3 Möglichkeiten:

                // 1. Den König bewegen (ungerne)
                // 2. Eine andere Figur bewegen, um den Angriff zu blockieren
                // 3. Eine Figur opfern, um den angreifenden Gegner zu schlagen

                List<SortedMove> checkOutMoves = [];

                foreach (var p in game.Board.Pieces)
                {
                    if (p.Color != PieceColor.Black)
                        continue;

                    var possibleMoves = p.GetPossibleMoves(game.Board);

                    foreach (var mv in possibleMoves)
                    {
                        if (!game.IsCheck(PieceColor.Black, p, mv))
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
                    return new NextMove(bestMove.Piece, bestMove.TargetPosition);
                }
            }
            else
            {
                if (game.CanCastle(PieceColor.Black, new Position(3, 8)))
                {
                    var king = game.Board.GetPiece(new Position(5, 8));
                    return new NextMove(king!, new Position(3, 8));
                }
                else if (game.CanCastle(PieceColor.Black, new Position(7, 8)))
                {
                    var king = game.Board.GetPiece(new Position(5, 8));
                    return new NextMove(king!, new Position(7, 8));
                }
            }

            foreach (var p in game.Board.Pieces)
            {
                if (p.Color != PieceColor.Black)
                    continue;

                var possibleMoves = p.GetPossibleMoves(game.Board);
                foreach (var mv in possibleMoves)
                {
                    var capturePiece = game.Board.GetPiece(mv);
                    if (capturePiece == null)
                    {
                        sortedMoves.Add(new SortedMove
                        {
                            Piece = p,
                            TargetPosition = mv,
                            Score = 0
                        });
                    }
                    else
                    {
                        if (capturePiece is King)
                            continue;

                        // Cannot caputre into check
                        if (game.IsCheck(PieceColor.Black, p, mv))
                            continue;

                        canCapture = true;
                        sortedMoves.Add(new SortedMove
                        {
                            Piece = p,
                            TargetPosition = mv,
                            Score = capturePiece.MaterialValue
                        });
                    }
                }
            }

            if (canCapture)
            {
                var result = sortedMoves.OrderByDescending(p => p.Score).FirstOrDefault();
                return new NextMove(result!.Piece, result.TargetPosition);
            }
            else
            {
                var piece = GetRandom(game.Board.Pieces.Where(p => p.Color == PieceColor.Black), p => p.GetPossibleMoves(game.Board).Count > 0);
                var moves = piece.GetPossibleMoves(game.Board);

                return new NextMove(piece, moves[Random.Shared.Next(moves.Count)]);
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
