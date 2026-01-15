namespace Chess.NET.Model.Pieces
{
    public class Pawn : Piece
    {
        public override PieceType Type => PieceType.Pawn;

        public override int MaterialValue => 1;

        public Pawn(Position position, PieceColor color) : base(position, color)
        {
        }   

        public override List<Position> GetPossibleMoves(IBoard board)
        {
            List<Position> positions = [];

            if (Position.Rank == 8)
            {
                return []; // TODO von 7-8 ist Umwandlung
            }


            if (Color == PieceColor.White)
            {
                bool mayHopTwice = false;   
                Position newPos = new Position(Position.File, Position.Rank + 1);
                if (board.GetPiece(newPos) == null)
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
            else if (Color == PieceColor.Black)
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

            return positions;
        }
    }
}