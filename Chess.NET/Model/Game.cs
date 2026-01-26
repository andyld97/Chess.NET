using Chess.NET.Bot;
using Chess.NET.Model.Pieces;

namespace Chess.NET.Model
{
    public class Game
    {
        #region Private Members
        private readonly Board board = new Board();
        private Puzzle? currentPuzzle = null;
        private IChessBot? opponent = null;

        private bool hasWhiteCastled = false;
        private bool hasBlackCastled = false;
        private bool isPuzzle = false;

        #endregion

        #region Events

        public delegate void onMovedPiece(MoveNotation move);
        public event onMovedPiece? OnMovedPiece;

        public delegate void onCheckmate(PieceColor pieceColor);
        public event onCheckmate? OnCheckmate;

        public delegate void onStalemate();
        public event onStalemate? OnStalemate;

        public delegate void onPlaySound(Sound.SoundType type);
        public event onPlaySound? OnPlaySound;

        #endregion

        #region Properties

        public IBoard Board => board;

        public List<MoveNotation> Moves { get; set; } = [];

        public PieceColor PlayersTurn { get; private set; } = PieceColor.White;

        public bool IsGameOver { get; private set; }

        public void StartNewGame(IChessBot? opponent)
        {
            board.Reset();
            IsGameOver = false;
            PlayersTurn = PieceColor.White;
            Moves.Clear();            
            hasWhiteCastled = false;
            hasBlackCastled = false;
            this.opponent = opponent;
        }

        #endregion

        #region EN PASSANT

        private bool IsEnPassant(PieceColor color, Piece piece, Position position)
        {
            if (piece.Type != PieceType.Pawn)
                return false;            

            // Ideen für Bedinungen um EN PASSANT zu ermitteln:
            // - Letzter Zug muss ein Bauern-Zug mit 2 Schritten gewesen sein!
            // - Letzter Zug muss ein Bauern-Zug gewesen sein, der direkt neben dem übergeben Bauern ist.
            //   Konkret bedeutet dass das File von LastMove.Pos.File == position.File sein muss
            // - Zielfeld muss leer sein!

            var lastMove = Moves.LastOrDefault();
            if (lastMove == null)
                return false;
            if (lastMove.Piece.Color == piece.Color)
                return false; // LastMove muss von der anderen Farbe gewesen sein!
            if (lastMove.Piece.Type != PieceType.Pawn)
                return false;
            if (Math.Abs(lastMove.From.Rank - lastMove.To.Rank) != 2)
                return false;
            if (lastMove.To.File != position.File)
                return false;

            // Zielfeld leer?
            if (board.GetPiece(position) != null)
                return false;

            // Eigener Bauer muss auf dem richtigen Rank stehen!
            if (color == PieceColor.White && piece.Position.Rank != 5)
                return false;
            if (color == PieceColor.Black && piece.Position.Rank != 4)
                return false;

            // Gegnerischer Bauer muss direkt neben diesem Bauern stehen
            if (lastMove.To.Rank != piece.Position.Rank)
                return false;

            // Gegnerischer Bauer muss direkt links oder rechts neben mir stehen
            if (Math.Abs(lastMove.To.File - piece.Position.File) != 1)
                return false;

            // Prüfen ob das Zielfeld korrekt ist
            if (color == PieceColor.White && position.Rank != piece.Position.Rank + 1)
                return false;
            else if (color == PieceColor.Black && position.Rank != piece.Position.Rank - 1)
                return false;

            return true;
        }

        #endregion

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
                if (king.Position.File - 1 < 1 || king.Position.File - 2 < 1)
                    return false;

                var position1 = new Position(king.Position.File - 1, king.Position.Rank);
                var position2 = new Position(king.Position.File - 2, king.Position.Rank);

                if (IsCheck(color, king, position1) || IsCheck(color, king, position2))
                    return false;
            }
            else
            {
                // Short castle
                if (king.Position.File + 1 > 8 || king.Position.File + 2 > 8)
                    return false;

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

        public bool IsMoveValid(Piece piece, Position position)
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

            if (IsEnPassant(piece.Color, piece, position) && !IsCheck(PlayersTurn, piece, position))
                return true;

            if (!piece.GetPossibleMoves(board).Contains(position))
                return false;

            if (IsCheck(PlayersTurn, piece, position))
                return false; // have to leave check first or cannot go into or capture into check

            return true;
        }

        public async Task<bool> MoveAsync(PendingMove? nxtMove, bool playSound = true)
        {
            if (nxtMove == null) 
                return false;

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
            bool isEnPassant = false;
            string disambiguation = string.Empty;
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
                board.PromotedPieces.Add(newPiece);
                board.Pieces.Add(newPiece);

                willBeCheck |= IsCheck(Helper.InvertPieceColor(piece.Color));
                isPromotion = true;
            }

            if (IsEnPassant(piece.Color, piece, position))
            {
                // Execute EN PASSANT

                // 1) Bauer an Zielposition schieben
                // 2) gegenerischen Bauern (File, Rank - 1) [Weiß], (File, Rank + 1) schwarz entfernen

                piece.Position = position;
                Piece? pieceToCapture = null;

                if (piece.Color == PieceColor.White)
                    pieceToCapture = board.GetPiece(new Position(position.File, position.Rank - 1));
                else if (piece.Color == PieceColor.Black)
                    pieceToCapture = board.GetPiece(new Position(position.File, position.Rank + 1));

                board.Pieces.Remove(pieceToCapture!);
                board.CapturedPieces.Add(pieceToCapture!);

                OnPlaySound?.Invoke(Sound.SoundType.Capture);
                isEnPassant = true;
            }
            else if (IsCastlePosition(position) && CanCastle(piece.Color, position) && piece.Type == PieceType.King && Castle(piece.Color, position))
            {
                isCastle = true;
                if (playSound)
                    OnPlaySound?.Invoke(Sound.SoundType.Castle);
            }
            else
            {
                if (currentPiece != null)
                {
                    // Capture!!
                    isCapture = true;

                    if (piece.Type != PieceType.Pawn && piece.Type != PieceType.King && board.Pieces.Count(p => p.Color == piece.Color && p.Type == piece.Type) > 1)
                    {
                        // Check for Raxc6 or Rhxc6 or R2xc2
                        var ambiguousPieces = board.Pieces.Where(p => p.Color == piece.Color && p.Type == piece.Type && p != piece && p.GetPossibleMoves(board).Contains(position));
                        bool sameFile = ambiguousPieces.Any(p => p.Position.File == position.File);
                        bool sameRank = ambiguousPieces.Any(p => p.Position.Rank == position.Rank);

                        if (!sameFile)
                            disambiguation = ((char)('a' + piece.Position.File - 1)).ToString();
                        else if (!sameRank)
                            disambiguation = piece.Position.Rank.ToString();
                    }

                    board.CapturePiece(currentPiece);

                    if (!willBeCheck && playSound)
                        OnPlaySound?.Invoke(Sound.SoundType.Capture);
                }
                else if (!willBeCheck && playSound)
                    OnPlaySound?.Invoke(Sound.SoundType.Move);
            }

            var move = new MoveNotation()
            {
                From = oldPosition,
                To = position,
                IsCapture = isCapture,
                Piece = piece,
                IsPromotion = isPromotion,
                PromotionType = promotionType,
                IsCheck = willBeCheck,
                Disambiguation = disambiguation
            };

            if (isEnPassant)
                move.IsCapture = true;

            if (Moves.Count > 0)
            {
                move.Count = Moves[^1].Count;
                //if (piece.Color == PieceColor.White)
                    move.Count++;
            }

            if (isCastle)
            {
                if (position.File == 7)
                    move.IsCastleKingSide = true;
                else
                    move.IsCastleQueenSide = true;
            }

            if (!isCastle && !isPromotion && !isEnPassant)
                piece.Position = position;

            bool isCheckmate = IsCheckmate(PieceColor.White) || IsCheckmate(PieceColor.Black);
            if (isCheckmate)
                move.IsCheckmate = true;

            Moves.Add(move);

            if (isCheckmate)
            {
                IsGameOver = true;

                OnMovedPiece?.Invoke(move);
                OnCheckmate?.Invoke((IsCheckmate(PieceColor.White) ? PieceColor.White : PieceColor.Black));

                if (playSound)
                {
                    OnPlaySound?.Invoke(Sound.SoundType.Move);
                    _ = DelaySoundPlay(Sound.SoundType.Checkmate);
                }

                return true;
            }
            else if (IsStalemate(PieceColor.White) || IsStalemate(PieceColor.Black))
            {
                IsGameOver = true;
                move.IsStalemate = true;

                OnMovedPiece?.Invoke(move);
                OnStalemate?.Invoke();

                if (playSound)
                    OnPlaySound?.Invoke(Sound.SoundType.Stalemate);

                return true;
            }
            else if (willBeCheck && playSound)
                OnPlaySound?.Invoke(Sound.SoundType.Check);

            // Switch players
            PlayersTurn = Helper.InvertPieceColor(PlayersTurn);
            OnMovedPiece?.Invoke(move);

            return true;
        }

        #endregion

        #region Puzzle
        public void LoadPuzzle(Puzzle puzzle)
        {
            board.Pieces.Clear();
            PlayersTurn = puzzle.ColorToMove;

            foreach (var piece in puzzle.Pieces)
                board.Pieces.Add((Piece)piece.Clone());

            currentPuzzle = puzzle;
            isPuzzle = true;
        }

        #endregion

        #region Other

        public PlayerInfo GetPlayerInformation()
        {
            var whiteCapturedPieces = board.CapturedPieces.Where(p => p.Color == PieceColor.Black);
            var blackCapturedPieces = board.CapturedPieces.Where(p => p.Color == PieceColor.White);

            var whitePromotedPieces = board.PromotedPieces.Where(p => p.Color == PieceColor.White);
            var blackPromotedPieces = board.PromotedPieces.Where(p => p.Color == PieceColor.Black);

            // Promotion: -1 per Piece cause you promoted the pawn, but lost it anyways!
            int whiteCapturePiecesMaterialValue = whiteCapturedPieces.Sum(p => p.MaterialValue) + whitePromotedPieces.Sum(p => p.MaterialValue - 1);
            int blackCapturePiecesMaterialValue = blackCapturedPieces.Sum(p => p.MaterialValue) + blackPromotedPieces.Sum(p => p.MaterialValue - 1);

            return new PlayerInfo(whiteCapturePiecesMaterialValue, blackCapturePiecesMaterialValue, [.. whiteCapturedPieces], [.. blackCapturedPieces]);
        }

        private async Task DelaySoundPlay(Sound.SoundType sound, int delayMs = 500)
        {
            await Task.Delay(delayMs);
            OnPlaySound?.Invoke(sound);
        }

        public override string ToString()
        {
            return board.ToString();
        }

        #endregion
    }
}