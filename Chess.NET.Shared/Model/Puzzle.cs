using Chess.NET.Shared.Model.Pieces;

namespace Chess.NET.Shared.Model
{
    public class Puzzle
    {
        #region Puzzles
        public static List<Puzzle> Puzzles { get; set; } =
        [
            new Puzzle()
            {
                ColorToMove = Color.White,
                Moves = ["Qxg7#"],
                Name = "Silent Square",
                Pieces = [new Pawn(Position.Parse("a2"), Color.White),
                          new Pawn(Position.Parse("b2"), Color.White),
                          new Pawn(Position.Parse("c2"), Color.White),
                          new Pawn(Position.Parse("d3"), Color.White),
                          new Pawn(Position.Parse("e4"), Color.White),
                          new Pawn(Position.Parse("f2"), Color.White),
                          new Pawn(Position.Parse("g2"), Color.White),
                          new Pawn(Position.Parse("h2"), Color.White),
                          new Rook(Position.Parse("a1"), Color.White),
                          new Rook(Position.Parse("h1"), Color.White),
                          new Knight(Position.Parse("b1"), Color.White),
                          new Knight(Position.Parse("g1"), Color.White),
                          new King(Position.Parse("e1"), Color.White),
                          new Bishop(Position.Parse("c4"), Color.White),
                          new Bishop(Position.Parse("h6"), Color.White),
                          new Queen(Position.Parse("g3"), Color.White),
                          new Pawn(Position.Parse("a7"), Color.Black),
                          new Pawn(Position.Parse("b7"), Color.Black),
                          new Pawn(Position.Parse("c7"), Color.Black),
                          new Pawn(Position.Parse("d6"), Color.Black),
                          new Pawn(Position.Parse("e5"), Color.Black),
                          new Pawn(Position.Parse("f7"), Color.Black),
                          new Pawn(Position.Parse("g7"), Color.Black),
                          new Pawn(Position.Parse("h7"), Color.Black),
                          new Rook(Position.Parse("a8"), Color.Black),
                          new Knight(Position.Parse("c6"), Color.Black),
                          new Knight(Position.Parse("f6"), Color.Black),
                          new Bishop(Position.Parse("c8"), Color.Black),
                          new Bishop(Position.Parse("c5"), Color.Black),
                          new Queen(Position.Parse("d8"), Color.Black),
                          new King(Position.Parse("g8"), Color.Black),
                          new Rook(Position.Parse("f8"), Color.Black)
                         ],
            },
            new Puzzle()
            {
                ColorToMove = Color.White,
                Name = "Last Light",
                Moves = ["Bh6+", "Kxh6", "Qf8#"],
                Pieces = [
                    new Rook(Position.Parse("a8"), Color.White),
                    new Bishop(Position.Parse("f1"), Color.Black),
                    new King(Position.Parse("g7"), Color.Black),
                    new Pawn(Position.Parse("h7"), Color.Black),
                    new Pawn(Position.Parse("g6"), Color.Black),
                    new Pawn(Position.Parse("f7"), Color.Black),
                    new King(Position.Parse("a2"), Color.White),
                    new Pawn(Position.Parse("b2"), Color.White),
                    new Pawn(Position.Parse("b3"), Color.White),
                    new Bishop(Position.Parse("c1"), Color.White),
                    new Queen(Position.Parse("d6"), Color.White),
                    new Pawn(Position.Parse("g4"), Color.White),
                    new Pawn(Position.Parse("h4"), Color.White)
                ]
            }
        ];

        #endregion

        public Color ColorToMove { get; set; }

        public List<string> Moves { get; set; } = [];

        public string Name { get; set; } = string.Empty;

        public List<Piece> Pieces { get; set; } = [];

        public PuzzleSolved SolveType { get; set; } = PuzzleSolved.Checkmate;
    }

    public enum PuzzleSolved
    {
        Checkmate,
        Stalemate
    }
}