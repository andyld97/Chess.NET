using Chess.NET.Shared.Model.Bot;
using Chess.NET.Shared.Model.Pieces;

namespace Chess.NET.Shared.Model
{
    public class Game
    {
        #region Private Members
        private readonly Board board = new Board();
        private List<ChessPosition> positions = [];
        private Puzzle? currentPuzzle = null;
        private IChessBot? opponent = null;

        private bool hasWhiteCastled = false;
        private bool hasBlackCastled = false;
        private bool isPuzzle = false;

        #endregion

        #region Events

        public delegate void onMovedPiece(MoveNotation move);
        public event onMovedPiece? OnMovedPiece;

        public delegate void onPlaySound(SoundType type);
        public event onPlaySound? OnPlaySound;

        public delegate void onGameOver(GameResult result, Color? colorWon);
        public event onGameOver? OnGameOver;    

        #endregion

        #region Properties

        public IBoard Board => board;

        public List<MoveNotation> Moves { get; set; } = [];

        public Color PlayersTurn { get; private set; } = Color.White;

        public bool IsGameOver { get; private set; }

        #endregion

        #region EN PASSANT

        private bool IsEnPassant(Color color, Piece piece, Position position)
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
            if (color == Color.White && piece.Position.Rank != 5)
                return false;
            if (color == Color.Black && piece.Position.Rank != 4)
                return false;

            // Gegnerischer Bauer muss direkt neben diesem Bauern stehen
            if (lastMove.To.Rank != piece.Position.Rank)
                return false;

            // Gegnerischer Bauer muss direkt links oder rechts neben mir stehen
            if (Math.Abs(lastMove.To.File - piece.Position.File) != 1)
                return false;

            // Prüfen ob das Zielfeld korrekt ist
            if (color == Color.White && position.Rank != piece.Position.Rank + 1)
                return false;
            else if (color == Color.Black && position.Rank != piece.Position.Rank - 1)
                return false;

            return true;
        }

        #endregion

        #region Castle

        private bool CastleShort(Color color)
        {
            // Move the king and the rook (short castle)   
            var king = board.Pieces.FirstOrDefault(p => p.Type == PieceType.King && p.Color == color);
            var rook = board.Pieces.FirstOrDefault(p => p.Type == PieceType.Rook && p.Color == color &&
            (
                (p.Position == new Position(8, 1) && color == Color.White) ||
                (p.Position == new Position(8, 8) && color == Color.Black)
            ));

            if (king == null || rook == null)
                return false;

            king.Position = new Position(7, color == Color.White ? 1 : 8);
            rook.Position = new Position(6, color == Color.White ? 1 : 8);

            return true;
        }

        private bool CastleLong(Color color)
        {
            // Move the king and the rook (long castle) 
            var king = board.Pieces.FirstOrDefault(p => p.Type == PieceType.King && p.Color == color);
            var rook = board.Pieces.FirstOrDefault(p => p.Type == PieceType.Rook && p.Color == color &&
            (
                (p.Position == new Position(1, 1) && color == Color.White) ||
                (p.Position == new Position(1, 8) && color == Color.Black)
            ));

            if (king == null || rook == null)
                return false;

            king.Position = new Position(3, color == Color.White ? 1 : 8);
            rook.Position = new Position(4, color == Color.White ? 1 : 8);

            return true;
        }

        private bool Castle(Color color, Position position)
        {
            bool result;
            if (position.File == 7)
                result = CastleShort(color);
            else
                result = CastleLong(color);

            if (result)
            {
                if (color == Color.White)
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

        public bool CanCastle(Color color, Position position)
        {
            if (IsCheck(color))
                return false;
            else if (color == Color.White && hasWhiteCastled)
                return false;
            else if (color == Color.Black && hasBlackCastled)
                return false;

            bool isLongCastle = position.File == 3;

            // Check if the king and rook are in their starting positions
            bool hasLeftRookMoved = Moves.Any(m => m.Piece.Type == PieceType.Rook && m.Piece.Color == color &&
            (
                (m.From == new Position(1, 1) && color == Color.White) ||
                (m.From == new Position(1, 8) && color == Color.Black)
            ));
            
            bool hasRightRookMoved = Moves.Any(m => m.Piece.Type == PieceType.Rook && m.Piece.Color == color &&
            (
                (m.From == new Position(8, 1) && color == Color.White) ||
                (m.From == new Position(8, 8) && color == Color.Black)
            ));

            bool hasKingMoved = Moves.Any(m => m.Piece.Type == PieceType.King && m.Piece.Color == color);

            //  Weiter muss berücksichtigt werden, dass zwischen dem König und dem Turm keine Figuren stehen dürfen
            if (isLongCastle)
            {
                if (color == Color.White)
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
                if (color == Color.White)
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

        #region Check, Checkmate, Stalemate, 50-Move Rule, Threefold Repition

        public bool IsCheckmate(Color color)
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

        public bool IsStalemate(Color color)
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

        public bool IsCheck(Color pieceColor)
        {
            return board.IsCheck(pieceColor);
        }

        public bool IsCheck(Color color, Piece piece, Position target)
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

        private void CheckFiftyMoveRule()
        {
            if (Moves.Count >= 100)
            {
                var last100HalfMoves = Moves.TakeLast(100);

                bool noCapture = !last100HalfMoves.Any(m => m.IsCapture);
                bool noPawnMove = !last100HalfMoves.Any(m => m.Piece.Type == PieceType.Pawn);

                if (noCapture && noPawnMove)
                {
                    IsGameOver = true;
                    OnGameOver?.Invoke(GameResult.FiftyMoveRule, null);
                }
            }
        }

        private void CheckInsufficientCheckmatingMaterial()
        {
            if (board.Pieces.Any(p => p.Type == PieceType.Pawn))
                return;

            bool insufficientCheckmatingMaterial = false;
            if (board.Pieces.Count == 2 && board.Pieces.All(p => p.Type == PieceType.King))
                insufficientCheckmatingMaterial = true;
            else if (board.Pieces.Count == 3 || board.Pieces.Count == 4)
            {
                // Case 1) King + Bishop (White) && King (Black) [3]

                // Case 2) King + Knight (White) && King (Black) [3]

                // Case 3) King (White) && King + Bishop (Black) [3]

                // Case 4) King (White) && King + Knight (Black) [3]

                // Case 5) King + Bishop (White and Black) but both bishops needs to be on the same color [4]

                var whiteKing = board.Pieces.FirstOrDefault(p => p.Type == PieceType.King && p.Color == Color.White);
                var blackKing = board.Pieces.FirstOrDefault(p => p.Type == PieceType.King && p.Color == Color.Black);

                if (whiteKing == null || blackKing == null)
                    return;

                if (board.Pieces.Count == 3)
                {
                    // Case 1-4)
                    if (board.Pieces.Any(p => p.Type == PieceType.Bishop))
                        insufficientCheckmatingMaterial = true;
                    else if (board.Pieces.Any(p => p.Type == PieceType.Knight))
                        insufficientCheckmatingMaterial = true;
                }

                else if (board.Pieces.Count == 4)
                {
                    // Case 5)
                    var whiteBishop = board.Pieces.FirstOrDefault(p => p.Type == PieceType.Bishop && p.Color == Color.White);
                    var blackBishop = board.Pieces.FirstOrDefault(p => p.Type == PieceType.Bishop && p.Color == Color.Black);

                    if (whiteBishop == null || blackBishop == null)
                        return;

                    insufficientCheckmatingMaterial =
                        (whiteBishop.Position.IsDarkSquare && blackBishop.Position.IsDarkSquare) ||
                        (whiteBishop.Position.IsLightSquare && blackBishop.Position.IsLightSquare);
                }
                else
                    return;
            }

            if (insufficientCheckmatingMaterial)
            {
                IsGameOver = true;
                OnGameOver?.Invoke(GameResult.InsufficentCheckmatingMaterial, null);
            }
        }

        #region Threefold Repition
        private void CheckThreefoldRepition()
        {
            bool threefoldRepition = false;

            // Check if there are three positions that are exactly the same!
            var groups = positions.GroupBy(p => p.GetHash());
            threefoldRepition = groups.Any(g => g.Count() >= 3);

            if (threefoldRepition)
                OnGameOver?.Invoke(GameResult.ThreefoldReptition, null);
        }

        private void AddCurrentPosition()
        {
            Position? enPassantSquare = null;

            var lastMove = Moves.LastOrDefault();
            if (lastMove != null && lastMove.Piece.Type == PieceType.Pawn && Math.Abs(lastMove.From.Rank - lastMove.To.Rank) == 2)
            {
                int file = lastMove.To.File;
                int rank = lastMove.Piece.Color == Color.White
                    ? lastMove.To.Rank - 1
                    : lastMove.To.Rank + 1;

                enPassantSquare = new Position(file, rank);
            }      

            ChessPosition currentPosition = new ChessPosition
            {
                SideToMove = PlayersTurn,
                WhiteCanCastleQueenSide = CanCastle(Color.White, Position.Parse("c1")),
                WhiteCanCastleKingSide = CanCastle(Color.White, Position.Parse("g1")),
                BlackCanCastleQueenSide = CanCastle(Color.Black, Position.Parse("c8")),
                BlackCanCastleKingSide = CanCastle(Color.Black, Position.Parse("g8")),
                PiecesHash = GetPiecesHashSha(),
                EnPassantSquare = enPassantSquare
            };

           positions.Add(currentPosition);  
        }

        private string GetPiecesHashSha()
        {
            var key = string.Join("|", board.Pieces
                .OrderBy(p => p.Position.File)
                .ThenBy(p => p.Position.Rank)
                .Select(p => $"{p.Type}-{p.Color}-{p.Position.File}-{p.Position.Rank}"));

            var bytes = System.Text.Encoding.UTF8.GetBytes(key);
            var hash = System.Security.Cryptography.SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        #endregion

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
            bool willBeCheck = IsCheck(piece.Color.InvertColor(), piece, position);

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

                willBeCheck |= IsCheck(piece.Color.InvertColor());
                isPromotion = true;
            }

            if (IsEnPassant(piece.Color, piece, position))
            {
                // Execute EN PASSANT

                // 1) Bauer an Zielposition schieben
                // 2) gegenerischen Bauern (File, Rank - 1) [Weiß], (File, Rank + 1) schwarz entfernen

                piece.Position = position;
                Piece? pieceToCapture = null;

                if (piece.Color == Color.White)
                    pieceToCapture = board.GetPiece(new Position(position.File, position.Rank - 1));
                else if (piece.Color == Color.Black)
                    pieceToCapture = board.GetPiece(new Position(position.File, position.Rank + 1));

                board.Pieces.Remove(pieceToCapture!);
                board.CapturedPieces.Add(pieceToCapture!);

                OnPlaySound?.Invoke(SoundType.Capture);
                isEnPassant = true;
            }
            else if (IsCastlePosition(position) && CanCastle(piece.Color, position) && piece.Type == PieceType.King && Castle(piece.Color, position))
            {
                isCastle = true;
                if (playSound)
                    OnPlaySound?.Invoke(SoundType.Castle);
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

                        if (ambiguousPieces.Any())
                        {
                            if (!sameFile)
                                disambiguation = ((char)('a' + piece.Position.File - 1)).ToString();
                            else if (!sameRank)
                                disambiguation = piece.Position.Rank.ToString();
                        }
                    }

                    board.CapturePiece(currentPiece);

                    if (!willBeCheck && playSound)
                        OnPlaySound?.Invoke(SoundType.Capture);
                }
                else if (!willBeCheck && playSound)
                    OnPlaySound?.Invoke(SoundType.Move);
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

            bool isCheckmate = IsCheckmate(Color.White) || IsCheckmate(Color.Black);
            if (isCheckmate)
                move.IsCheckmate = true;

            Moves.Add(move);

            if (isCheckmate)
            {
                IsGameOver = true;

                OnMovedPiece?.Invoke(move);
                OnGameOver?.Invoke(GameResult.Checkmate, IsCheckmate(Color.White) ? Color.Black : Color.White);

                if (playSound)
                {
                    OnPlaySound?.Invoke(SoundType.Move);
                    _ = DelaySoundPlay(SoundType.Checkmate);
                }

                return true;
            }
            else if (IsStalemate(Color.White) || IsStalemate(Color.Black))
            {
                IsGameOver = true;
                move.IsStalemate = true;

                OnMovedPiece?.Invoke(move);
                OnGameOver?.Invoke(GameResult.Stalemate, null);

                if (playSound)
                    OnPlaySound?.Invoke(SoundType.Stalemate);

                return true;
            }
            else if (willBeCheck && playSound)
                OnPlaySound?.Invoke(SoundType.Check);

            // Important for Threefold Repition
            AddCurrentPosition();

            // Switch players
            PlayersTurn = PlayersTurn.InvertColor();
            OnMovedPiece?.Invoke(move);

            // Check if there is still enough material to checkmate, otherwise its a draw too ...
            CheckInsufficientCheckmatingMaterial();

            // Check for fifty move rule (= 100 plies/Halbzüge)
            CheckFiftyMoveRule();

            // Check for threefold repition!
            CheckThreefoldRepition();            

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

        public void StartNewGame(IChessBot? opponent)
        {
            board.Reset();
            positions.Clear();
            IsGameOver = false;
            PlayersTurn = Color.White;
            Moves.Clear();
            hasWhiteCastled = false;
            hasBlackCastled = false;
            this.opponent = opponent;
        }

        public void Resign(Color color)
        {
            if (IsGameOver)
                return;

            IsGameOver = true;
            OnGameOver?.Invoke(GameResult.Resign, color.InvertColor());
        }

        public PlayerInfo GetPlayerInformation()
        {
            var whiteCapturedPieces = board.CapturedPieces.Where(p => p.Color == Color.Black);
            var blackCapturedPieces = board.CapturedPieces.Where(p => p.Color == Color.White);

            var whitePromotedPieces = board.PromotedPieces.Where(p => p.Color == Color.White);
            var blackPromotedPieces = board.PromotedPieces.Where(p => p.Color == Color.Black);

            // Promotion: -1 per Piece cause you promoted the pawn, but the pawn is on your team anymore!
            int whiteCapturePiecesMaterialValue = whiteCapturedPieces.Sum(p => p.MaterialValue) + whitePromotedPieces.Sum(p => p.MaterialValue - 1);
            int blackCapturePiecesMaterialValue = blackCapturedPieces.Sum(p => p.MaterialValue) + blackPromotedPieces.Sum(p => p.MaterialValue - 1);

            return new PlayerInfo(whiteCapturePiecesMaterialValue, blackCapturePiecesMaterialValue, [.. whiteCapturedPieces], [.. blackCapturedPieces]);
        }

        private async Task DelaySoundPlay(SoundType sound, int delayMs = 500)
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