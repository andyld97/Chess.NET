namespace Chess.NET.Shared.Model.Pieces
{
    public class Pawn : Piece
    {
        public override PieceType Type => PieceType.Pawn;

        public override int MaterialValue => 1;

        public Pawn(Position position, Color color) : base(position, color)
        {
        }   

        public override List<Position> GetPossibleMoves(Game game)
        {
            var board = game.Board;

            List<Position> positions = [];

            if (Position.Rank == 8)
                return [];

            if (Color == Color.White)
            {
                bool mayHopTwice = false;   
                Position newPos = new Position(Position.File, Position.Rank + 1);
                if (game.Board.GetPiece(newPos) == null)
                {
                    positions.Add(newPos);
                    mayHopTwice = true;
                }

                if (Position.Rank == 2 && mayHopTwice)
                {
                    Position doubleStepPos = new Position(Position.File, Position.Rank + 2);

                    if (board.GetPiece(doubleStepPos) == null)
                        positions.Add(doubleStepPos);
                }

                // Capture diagonals
                // If we have A and H pawns, they can capture only one side
                if (Position.File != 1 && Position.File != 8)
                {
                    Position captureLeft = new Position(Position.File - 1, Position.Rank + 1);
                    Position captureRight = new Position(Position.File + 1, Position.Rank + 1);

                    if (board.GetPiece(captureLeft) != null && board.GetPiece(captureLeft)?.Color != Color)
                        positions.Add(captureLeft);

                    if (board.GetPiece(captureRight) != null && board.GetPiece(captureRight)?.Color != Color)
                        positions.Add(captureRight);
                }
                else if (Position.File == 1)
                {
                    Position captureRight = new Position(Position.File + 1, Position.Rank + 1);

                    if (board.GetPiece(captureRight) != null && board.GetPiece(captureRight)?.Color != Color)
                        positions.Add(captureRight);
                }
                else if (Position.File == 8)
                {
                    Position captureLeft = new Position(Position.File - 1, Position.Rank + 1);

                    if (board.GetPiece(captureLeft) != null && board.GetPiece(captureLeft)?.Color != Color)
                        positions.Add(captureLeft);
                }
            }
            else if (Color == Color.Black)
            {
                Position newPos = new Position(Position.File, Position.Rank - 1);
                bool mayHopTwice = false;
                if (board.GetPiece(newPos) == null)
                {
                    positions.Add(newPos);
                    mayHopTwice = true;
                }

                if (Position.Rank == 7 && mayHopTwice)
                {
                    Position doubleStepPos = new Position(Position.File, Position.Rank - 2);
                    if (board.GetPiece(doubleStepPos) == null)
                        positions.Add(doubleStepPos);
                }

                // Capture diagonals
                // If we have A and H pawns, they can capture only one side
                if (Position.File != 1 && Position.File != 8)
                {
                    Position captureLeft = new Position(Position.File - 1, Position.Rank - 1);
                    Position captureRight = new Position(Position.File + 1, Position.Rank - 1);

                    if (board.GetPiece(captureLeft) != null && board.GetPiece(captureLeft)?.Color != Color)
                        positions.Add(captureLeft);

                    if (board.GetPiece(captureRight) != null && board.GetPiece(captureRight)?.Color != Color)
                        positions.Add(captureRight);
                }
                else if (Position.File == 1)
                {
                    Position captureRight = new Position(Position.File + 1, Position.Rank - 1);

                    if (board.GetPiece(captureRight) != null && board.GetPiece(captureRight)?.Color != Color)
                        positions.Add(captureRight);
                }
                else if (Position.File == 8)
                {
                    Position captureLeft = new Position(Position.File - 1, Position.Rank - 1);

                    if (board.GetPiece(captureLeft) != null && board.GetPiece(captureLeft)?.Color != Color)
                        positions.Add(captureLeft);
                }
            }

            // Also add EN Passant captures if available
            // A and H pawns can only on passant right and left
            List<Position> enPassantPositions = []; 

            if (Color == Color.White)
            {
                int rank = Position.Rank + 1;
                if (rank < 1)
                    return positions;

                int fileLeft = Position.File - 1;
                int fileRight = Position.File + 1;

                if (fileLeft > 1) 
                    enPassantPositions.Add(new Position(fileLeft, rank));   

                if (fileRight < 9)
                    enPassantPositions.Add(new Position(fileRight, rank));  
            }
            else
            {
                int rank = Position.Rank - 1;
                if (rank > 8)
                    return positions;

                int fileLeft = Position.File - 1;
                int fileRight = Position.File + 1;

                if (fileLeft > 1)
                    enPassantPositions.Add(new Position(fileLeft, rank));

                if (fileRight < 9)
                    enPassantPositions.Add(new Position(fileRight, rank));
            }

            foreach (var enPassant in enPassantPositions)
            {
                if (game.IsEnPassant(Color, this, enPassant))
                    positions.Add(enPassant);
            }

            return positions;
        }

        public override object Clone()
        {
            return new Pawn((Position)Position.Clone(), Color);
        }
    }
}