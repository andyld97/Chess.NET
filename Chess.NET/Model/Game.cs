using Chess.NET.Bot;
using Chess.NET.Model.Pieces;
using System.Collections.ObjectModel;

namespace Chess.NET.Model
{
    public class Game
    {
        private readonly Board board = new Board();
        private bool hasWhiteCastled = false;
        private bool hasBlackCastled = false;

        public delegate void onMove(Move move);
        public event onMove? MovedPiece;

        public IBoard Board => board;

        public List<Move> Moves { get; set; } = [];

        public PieceColor PlayersTurn { get; private set; } = PieceColor.White;

        public bool IsGameOver { get; private set; }

        public void StartNewGame()
        {
            board.Reset();
            IsGameOver = false;
            PlayersTurn = PieceColor.White;
            Moves.Clear();            
            hasWhiteCastled = false;
            hasBlackCastled = false;    
        }

        public PlayerInfo GetPlayerInformation()
        {
            var whiteCapturedPieces = board.CapturedPieces.Where(p => p.Color == PieceColor.Black);
            var blackCapturedPieces = board.CapturedPieces.Where(p => p.Color == PieceColor.White);

            int whiteCapturePiecesMaterialValue = whiteCapturedPieces.Sum(p => p.MaterialValue);
            int blackCapturePiecesMaterialValue = blackCapturedPieces.Sum(p => p.MaterialValue);

            return new PlayerInfo(whiteCapturePiecesMaterialValue, blackCapturePiecesMaterialValue, [.. whiteCapturedPieces], [.. blackCapturedPieces]);
        }

        #region Castle

        private bool CastleShort(PieceColor color)
        {
            // Move the king and the rook (short castle)   
            var king = board.Pieces.FirstOrDefault(p => p.Type == PieceType.King && p.Color == color);
            var rook = board.Pieces.FirstOrDefault(p => p.Type == PieceType.Rook && p.Color == color &&
            (
                (p.Position == new Position(8, 1) && color == PieceColor.White) ||
                (p.Position == new Position(8, 8) && color == PieceColor.Black)
            ));

            if (king == null || rook == null)
                return false;

            king.Position = new Position(7, color == PieceColor.White ? 1 : 8);
            rook.Position = new Position(6, color == PieceColor.White ? 1 : 8);

            return true;
        }

        private bool CastleLong(PieceColor color)
        {
            // Move the king and the rook (long castle) 
            var king = board.Pieces.FirstOrDefault(p => p.Type == PieceType.King && p.Color == color);
            var rook = board.Pieces.FirstOrDefault(p => p.Type == PieceType.Rook && p.Color == color &&
            (
                (p.Position == new Position(1, 1) && color == PieceColor.White) ||
                (p.Position == new Position(1, 8) && color == PieceColor.Black)
            ));

            if (king == null || rook == null)
                return false;

            king.Position = new Position(3, color == PieceColor.White ? 1 : 8);
            rook.Position = new Position(4, color == PieceColor.White ? 1 : 8);

            return true;
        }

        private bool Castle(PieceColor color, Position position)
        {
            bool result;
            if (position.File == 7)
                result = CastleShort(color);
            else
                result = CastleLong(color);

            if (result)
            {
                if (color == PieceColor.White)
                    hasWhiteCastled = true;
                else
                    hasBlackCastled = true;
            }

            return result;
        }

        private static bool IsCastlePosition(Position position)
        {
            return (position.File == 7 || position.File == 3) && (position.Rank == 1 || position.Rank == 8);
        }

        public bool CanCastle(PieceColor color, Position position)
        {
            if (IsCheck(color))
                return false;
            else if (color == PieceColor.White && hasWhiteCastled)
                return false;
            else if (color == PieceColor.Black && hasBlackCastled)
                return false;

            bool isLongCastle = position.File == 3;

            // Check if the king and rook are in their starting positions
            bool hasLeftRookMoved = Moves.Any(m => m.Piece.Type == PieceType.Rook && m.Piece.Color == color &&
            (
                (m.From == new Position(1, 1) && color == PieceColor.White) ||
                (m.From == new Position(1, 8) && color == PieceColor.Black)
            ));
            
            bool hasRightRookMoved = Moves.Any(m => m.Piece.Type == PieceType.Rook && m.Piece.Color == color &&
            (
                (m.From == new Position(8, 1) && color == PieceColor.White) ||
                (m.From == new Position(8, 8) && color == PieceColor.Black)
            ));

            bool hasKingMoved = Moves.Any(m => m.Piece.Type == PieceType.King && m.Piece.Color == color);

            //  Weiter muss berücksichtigt werden, dass zwischen dem König und dem Turm keine Figuren stehen dürfen
            if (isLongCastle)
            {
                if (color == PieceColor.White)
                {
                    if (!(board.GetPiece(new Position(2, 1)) == null && board.GetPiece(new Position(3, 1)) == null && board.GetPiece(new Position(4, 1)) == null))
                        return false;
                }
                else
                {
                    if (!(board.GetPiece(new Position(2, 8)) == null && board.GetPiece(new Position(3, 8)) == null && board.GetPiece(new Position(4, 8)) == null))
                        return false;
                }
            }
            else
            {
                if (color == PieceColor.White)
                {
                    if (!(board.GetPiece(new Position(6, 1)) == null && board.GetPiece(new Position(7, 1)) == null))
                        return false;
                }
                else
                {
                    if (!(board.GetPiece(new Position(6, 8)) == null && board.GetPiece(new Position(7, 8)) == null))
                        return false;
                }
            }

            // Weiter darf der König nicht im Schach stehen, durch ein Schach ziehen oder in ein Schach ziehen
            // Hier wäre jetzt die Idee mit IsCheck zu arbeiten und zwar:
            // Spielbrett klonen ohne den zu rochierenden König
            // Den Weg des Königs durchgehen und bei jedem Feld prüfen ob IsCheck(move) true ist, falls ja not possible
            var testCastleBoard = (Board)board.Clone();
            var king = testCastleBoard.Pieces.First(p => p.Type == PieceType.King && p.Color == color);
            if (king == null)
                return false;

            if (isLongCastle)
            {               
                var position1 = new Position(king.Position.File - 1, king.Position.Rank);
                var position2 = new Position(king.Position.File - 2, king.Position.Rank);

                if (IsCheck(color, king, position1) || IsCheck(color, king, position2))
                    return false;
            }
            else
            {
                // Short castle
                var position1 = new Position(king.Position.File + 1, king.Position.Rank);
                var position2 = new Position(king.Position.File + 2, king.Position.Rank);

                if (IsCheck(color, king, position1) || IsCheck(color, king, position2))
                    return false;
            }

            if (hasKingMoved)
                return false;
            if (isLongCastle && !hasLeftRookMoved)
                return true;
            if (!isLongCastle && !hasRightRookMoved)
                return true;

            return false;
        }

        #endregion

        #region Check, Checkmate & Stalemate

        public bool IsCheckmate(PieceColor color)
        {
            // 1) Check if there is a check by any of the opposite pieces (except the king cant give a check)
            if (!IsCheck(color))
                return false;

            foreach (var piece in board.Pieces.Where(p => p.Color == color))
            {
                foreach (var mv in piece.GetPossibleMoves(board))
                    if (!IsCheck(color, piece, mv))
                        return false;
            }

            // No legal move found it's over.
            return true;
        }

        public bool IsStalemate(PieceColor color)
        {
            // The same as checkmate but without the initial check
            foreach (var piece in board.Pieces.Where(p => p.Color == color))
            {
                foreach (var mv in piece.GetPossibleMoves(board))
                    if (!IsCheck(color, piece, mv))
                        return false;
            }

            // No legal move found it's over.
            return true;
        }

        public bool IsCheck(PieceColor pieceColor)
        {
            return board.IsCheck(pieceColor);
        }

        public bool IsCheck(PieceColor color, Piece piece, Position target)
        {
            var clone = (Board)board.Clone();

            // passende Figur im Clone finden
            var clonePiece = clone.Pieces.First(p => p.Color == piece.Color && p.Type == piece.Type && p.Position.Equals(piece.Position));

            // evtl. Capture entfernen
            var captured = clone.GetPiece(target);
            if (captured != null)
                clone.Pieces.Remove(captured);

            // Zug ausführen
            clonePiece.Position = target;

            return clone.IsCheck(color);
        }

        #endregion

        #region Move

        private bool IsMoveValid(Piece piece, Position position)
        {
            if (piece.Color != PlayersTurn)
                return false;

            // Check the target square
            var pieceOnTargetSquare = board.GetPiece(position);
            if (pieceOnTargetSquare == piece)
                return false; // the own piece cannot move to its own place

            if (pieceOnTargetSquare != null && pieceOnTargetSquare.Color == piece.Color)
                return false; // cannot move or capture pieces with the same color

            if (pieceOnTargetSquare is King)
                return false; // cannot capture king

            if (IsCastlePosition(position) && CanCastle(PlayersTurn, position) && piece.Type == PieceType.King)
                return true;

            if (!piece.GetPossibleMoves(board).Contains(position))
                return false;

            if (IsCheck(PlayersTurn, piece, position))
                return false; // have to leave check first or cannot go into or capture into check

            return true;
        }

        public bool Move(NextMove nxtMove, bool playSound = true)
        {
            var piece = nxtMove.Piece;
            var position = nxtMove.To;  
            var promotionType = nxtMove.PromotionType;  

            if (IsGameOver)
                return false;

            if (!IsMoveValid(piece, position))
                return false;

            var currentPiece = board.GetPiece(position);
            bool isCapture = false;
            bool isCastle = false;
            bool isPromotion = false;
            Position oldPosition = (Position)piece.Position.Clone();
            bool willBeCheck = IsCheck(Helper.InvertPieceColor(piece.Color), piece, position);

            if (piece.Type == PieceType.Pawn && (position.Rank == 1 || position.Rank == 8))
            {
                // Remove the pawn 
                board.Pieces.Remove(piece);

                Piece newPiece = null!;
                switch (promotionType)
                {
                    case PieceType.Rook:
                        newPiece = new Rook(position, piece.Color);
                        break;
                    case PieceType.Bishop:
                        newPiece = new Bishop(position, piece.Color);
                        break;
                    case PieceType.Knight:
                        newPiece = new Knight(position, piece.Color);
                        break;
                }

                // Fallback if there is a non valid promotion type
                newPiece ??= new Queen(position, piece.Color);
                board.Pieces.Add(newPiece); 
            }

            if (IsCastlePosition(position) && CanCastle(piece.Color, position) && piece.Type == PieceType.King && Castle(piece.Color, position))
            {
                isCastle = true;
                if (playSound)
                    Sound.Play(Sound.SoundType.Castle);
            }
            else
            {
                if (currentPiece != null)
                {
                    // Capture!!
                    isCapture = true;
                    board.CapturePiece(currentPiece);

                    if (!willBeCheck && playSound)
                        Sound.Play(Sound.SoundType.Capture);
                }
                else if (!willBeCheck && playSound)
                    Sound.Play(Sound.SoundType.Move);
            }

            var move = new Move()
            {
                From = oldPosition,
                To = position,
                IsCapture = isCapture,
                Piece = piece,
                IsPromotion = isPromotion,
                PromotionType = promotionType,
                IsCheck = willBeCheck
            };

            if (Moves.Count > 0)
            {
                move.Count = Moves[^1].Count;
                if (piece.Color == PieceColor.White)
                    move.Count++;
            }

            if (isCastle)
            {
                if (position.File == 7)
                    move.IsCastleKingSide = true;
                else
                    move.IsCastleQueenSide = true;
            }

            Moves.Add(move);

            if (!isCastle && !isPromotion)
                piece.Position = position;

            if (IsCheckmate(PieceColor.White) || IsCheckmate(PieceColor.Black))
            {
                IsGameOver = true;
                if (playSound)
                    Sound.Play(Sound.SoundType.Checkmate);
                move.IsCheckmate = true;
                MovedPiece?.Invoke(move);   
                return true;
            }
            else if (IsStalemate(PieceColor.White) || IsStalemate(PieceColor.Black))
            {
                IsGameOver = true;
                move.IsStalemate = true;
                MovedPiece?.Invoke(move);
                if (playSound)
                    Sound.Play(Sound.SoundType.Checkmate); // TODO Different sound for stalemate!
                return true;
            }
            else if (willBeCheck && playSound)
                Sound.Play(Sound.SoundType.Check);

            MovedPiece?.Invoke(move);

            // Switch players
            PlayersTurn = Helper.InvertPieceColor(PlayersTurn);

            return true;
        }

        #endregion

        public override string ToString()
        {
            return board.ToString();
        }
    }
}