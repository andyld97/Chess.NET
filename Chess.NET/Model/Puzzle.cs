using Chess.NET.Model.Pieces;

namespace Chess.NET.Model
{
    public class Puzzle
    {
        #region Puzzles
        public static List<Puzzle> Puzzles { get; set; } = 
        [
            new Puzzle()
            {
                ColorToMove = PieceColor.White,
                Moves = ["Qxg7#"],
                Name = "Silent Square",
                Pieces = [new Pawn(Position.Parse("a2"), PieceColor.White),
                          new Pawn(Position.Parse("b2"), PieceColor.White),
                          new Pawn(Position.Parse("c2"), PieceColor.White),
                          new Pawn(Position.Parse("d3"), PieceColor.White),
                          new Pawn(Position.Parse("e4"), PieceColor.White),
                          new Pawn(Position.Parse("f2"), PieceColor.White),
                          new Pawn(Position.Parse("g2"), PieceColor.White),
                          new Pawn(Position.Parse("h2"), PieceColor.White),
                          new Rook(Position.Parse("a1"), PieceColor.White),
                          new Rook(Position.Parse("h1"), PieceColor.White),
                          new Knight(Position.Parse("b1"), PieceColor.White),
                          new Knight(Position.Parse("g1"), PieceColor.White),
                          new King(Position.Parse("e1"), PieceColor.White),
                          new Bishop(Position.Parse("c4"), PieceColor.White),
                          new Bishop(Position.Parse("h6"), PieceColor.White),
                          new Queen(Position.Parse("g3"), PieceColor.White),
                          new Pawn(Position.Parse("a7"), PieceColor.Black),
                          new Pawn(Position.Parse("b7"), PieceColor.Black),
                          new Pawn(Position.Parse("c7"), PieceColor.Black),
                          new Pawn(Position.Parse("d6"), PieceColor.Black),
                          new Pawn(Position.Parse("e5"), PieceColor.Black),
                          new Pawn(Position.Parse("f7"), PieceColor.Black),
                          new Pawn(Position.Parse("g7"), PieceColor.Black),
                          new Pawn(Position.Parse("h7"), PieceColor.Black),
                          new Rook(Position.Parse("a8"), PieceColor.Black),
                          new Knight(Position.Parse("c6"), PieceColor.Black),
                          new Knight(Position.Parse("f6"), PieceColor.Black),
                          new Bishop(Position.Parse("c8"), PieceColor.Black),
                          new Bishop(Position.Parse("c5"), PieceColor.Black),
                          new Queen(Position.Parse("d8"), PieceColor.Black),
                          new King(Position.Parse("g8"), PieceColor.Black),
                          new Rook(Position.Parse("f8"), PieceColor.Black)
                         ],             
            },
            new Puzzle()
            {
                ColorToMove = PieceColor.White,
                Name = "Last Light",
                Moves = ["Bh6+", "Kxh6", "Qf8#"],
                Pieces = [
                    new Rook(Position.Parse("a8"), PieceColor.White),
                    new Bishop(Position.Parse("f1"), PieceColor.Black),
                    new King(Position.Parse("g7"), PieceColor.Black),
                    new Pawn(Position.Parse("h7"), PieceColor.Black),
                    new Pawn(Position.Parse("g6"), PieceColor.Black),
                    new Pawn(Position.Parse("f7"), PieceColor.Black),
                    new King(Position.Parse("a2"), PieceColor.White),
                    new Pawn(Position.Parse("b2"), PieceColor.White),
                    new Pawn(Position.Parse("b3"), PieceColor.White),
                    new Bishop(Position.Parse("c1"), PieceColor.White),
                    new Queen(Position.Parse("d6"), PieceColor.White),
                    new Pawn(Position.Parse("g4"), PieceColor.White),
                    new Pawn(Position.Parse("h4"), PieceColor.White)
                ]
            }
        ];

        #endregion

        public PieceColor ColorToMove { get; set; }

        public List<string> Moves { get; set; } = [];

        public string Name { get; set; } = string.Empty;

        public List<Piece> Pieces { get; set; } = [];

    }
}