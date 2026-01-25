using Chess.NET.Bot;
using Chess.NET.Controls.Dialogs;
using Chess.NET.Model;
using Chess.NET.Model.GUI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Chess.NET.Controls
{
    /// <summary>
    /// Interaktionslogik für ChessBoard.xaml
    /// </summary>
    public partial class ChessBoard : UserControl
    {
        private readonly BoardSquare[,] _squares = new BoardSquare[8, 8];
        private readonly Game game = new Game();

        private IChessBot? opponent = null;
        private bool isMirrored = false;
        private bool isPuzzle = false;
        private bool canMove = false;
        private bool isNavigationAllowed = true;

        public Game Game => game;

        public ChessBoard()
        {
            InitializeComponent();
            InitializeSquares();

            game = new Model.Game();
            game.StartNewGame(null);

            RenderChessBoard(game.Board);
            canMove = true;
        }

        public void LoadPuzzle(Puzzle puzzle)
        {
            game.LoadPuzzle(puzzle);

            RenderChessBoard(game.Board);
            isPuzzle = true;
            DisableNavigation();
        }

        public void DisablePieces()
        {
            canMove = false;
        }

        public void DisableNavigation()
        {
            isNavigationAllowed = false;
        }

        public void EnableNavigation()
        {
            isNavigationAllowed = true;     
        }

        private void InitializeSquares()
        {
            for (int rank = 8; rank >= 1; rank--)
            {
                for (int file = 1; file <= 8; file++)
                {
                    bool dark = (file + rank) % 2 == 0;

                    var square = new Border
                    {
                        Background = (Brush)FindResource(dark ? "ChessDarkSquare" : "ChessLightSquare")
                    };

                    Grid.SetColumn(square, file - 1);
                    Grid.SetRow(square, 8 - rank);

                    var image = new Image() { Margin = new System.Windows.Thickness(10), HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = System.Windows.VerticalAlignment.Center };
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Fant);
                    square.Child = image;
                    square.Tag = new Position(file, rank);
                    square.MouseDown += Square_MouseDown;
                    square.MouseMove += Square_MouseMove;
                    BoardGrid.Children.Add(square);

                    _squares[file - 1, rank - 1] = new BoardSquare(file, rank, square);
                }
            }
        }

        public void Restart(IChessBot? opponent)
        {
            ResetDrag();

            game.StartNewGame(opponent);
            RenderChessBoard(game.Board);
            this.opponent = opponent;
            isInNavigationMode = false;
            canMove = true;
        }

        public void Mirror()
        {
            isMirrored = !isMirrored; // toggle
            RenderChessBoard(game.Board);
        }


        #region Drag & Drop Pieces
        private Piece? _pieceToMove;
        private int clickCounter = 0;

        private void ResetDrag()
        {
            DraggedImage.Visibility = Visibility.Hidden;
            clickCounter = 0;
            _pieceToMove = null;
        }

        private async void Square_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!canMove) return;   

            if (isInNavigationMode)
            {
                navigationCurrentMove = -1;
                isInNavigationMode = false;
                ResetDrag();
                RenderChessBoard(game.Board, true);
                return;
            }

            var square = (Border)sender;
            var image = (Image)square.Child;

            if (clickCounter > 0)
            {
                var destinationSquare = (Position)square.Tag;
                if (isMirrored)
                    destinationSquare = destinationSquare.Mirror();

                PieceType? promotionType = null;

                if (Settings.Instance.AutoPromoteToQueen && !isPuzzle)
                    promotionType = PieceType.Queen;
                else if (_pieceToMove?.Type == PieceType.Pawn && (destinationSquare.Rank == 1 || destinationSquare.Rank == 8) && game.IsMoveValid(_pieceToMove!, destinationSquare))
                {
                    // Promotion Dialog
                    var dialog = new PromotionDialog(PieceColor.White) { Owner = Window.GetWindow(this) };
                    dialog.ShowDialog();

                    promotionType = dialog.PromotionResult;
                }

                bool wasMoveAccepted = await game.MoveAsync(new PendingMove(_pieceToMove!, destinationSquare, promotionType));

                ResetDrag();
                RenderChessBoard(game.Board);

                if (!wasMoveAccepted)
                    return;

                // Bot Move
                if (opponent != null)
                {
                    await Task.Delay(1000);

                    bool foundValidMove = false;
                    while (!foundValidMove)
                    {
                        if (game.IsCheckmate(PieceColor.Black) || game.IsCheckmate(PieceColor.White))
                            return;

                        if (game.IsStalemate(PieceColor.Black) || game.IsStalemate(PieceColor.White))
                            return;

                        var next = opponent.Move(game);
                        if (next == null)
                            break;

                        foundValidMove = await game.MoveAsync(next);
                    }
                }

                RenderChessBoard(game.Board);
            }
            else
            {
                var currentPosition = (Position)square.Tag;
                if (isMirrored)
                    currentPosition = currentPosition.Mirror();
                _pieceToMove = game.Board.GetPiece(currentPosition);

                if (_pieceToMove == null)
                {
                    ResetDrag();
                    return; // No piece to drag 
                }

                DraggedImage.Width = square.ActualWidth - 10;
                DraggedImage.Height = square.ActualHeight - 10;
                DraggedImage.Source = image.Source;
                image.Source = null;
                DraggedImage.Visibility = Visibility.Visible;

                clickCounter++;
            }
        }

        private void Square_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var pos = e.GetPosition(DragCanvas);

            Canvas.SetLeft(DraggedImage, pos.X - 32);
            Canvas.SetTop(DraggedImage, pos.Y - 32);
        }

        #endregion

        public void RenderChessBoard(IBoard board, bool renderLastMoveSquares = true)
        {
            var lastMove = game.Moves.LastOrDefault();

            for (int rank = 8; rank >= 1; rank--)
            {
                for (int file = 1; file <= 8; file++)
                {
                    var position = new Position(file, rank);
                    if (isMirrored)
                        position = position.Mirror();

                    if (_pieceToMove != null && position == _pieceToMove.Position)
                        continue;

                    Image img = (Image)_squares[file - 1, rank - 1].Border.Child;

                    var piece = board.GetPiece(position);
                    img.Source = (piece != null) ? piece.Type.ToBitmap(piece.Color) : null;

                    if (renderLastMoveSquares && (lastMove != null && (lastMove.From == position || lastMove.To == position)))
                    {
                        // Highlight last move squares  
                        _squares[file - 1, rank - 1].Border.Background = (Brush)FindResource("ChessHighlightSquare");
                    }
                    else
                    {
                        bool dark = (file + rank) % 2 == 0;
                        _squares[file - 1, rank - 1].Border.Background = (Brush)FindResource(dark ? "ChessDarkSquare" : "ChessLightSquare");
                    }
                }
            }
        }

        #region Game Navigation

        private int navigationCurrentMove = -1;
        private bool isInNavigationMode = false;

        public async Task JumpToStartAsync()
        {
            if (!isNavigationAllowed)
                return;

            navigationCurrentMove = 0;
            await ShowMoveAsync();
        }

        public async Task ShowLastMoveAsync()
        {
            if (!isNavigationAllowed)
                return;

            navigationCurrentMove = game.Moves.Count;
            await ShowMoveAsync();
        }

        public async Task ShowPreviousMoveAsync()
        {
            if (!isNavigationAllowed)
                return;

            if (navigationCurrentMove == -1 || navigationCurrentMove > game.Moves.Count)
                navigationCurrentMove = game.Moves.Count;

            if (navigationCurrentMove == 0)
                return;

            int removedMoveIndex = navigationCurrentMove - 1;
            var removedMove = game.Moves[removedMoveIndex];

            navigationCurrentMove--;

            var gm = await ShowMoveAsync();

            PlayMoveSound(gm, true, removedMove);
            RenderChessBoard(gm.Board, false);
            isInNavigationMode = true;
        }

        public async Task ShowMoveAsync(int index)
        {
            if (!isNavigationAllowed)
                return;

            navigationCurrentMove = index + 1;
            await ShowMoveAsync();
        }

        private async Task<Game> ShowMoveAsync()
        {
            if (!isNavigationAllowed)
                return null!;

            Game gm = new Game();
            gm.StartNewGame(null);

            for (int i = 0; i < navigationCurrentMove; i++)
            {
                var move = game.Moves[i];
                await gm.MoveAsync(new PendingMove(gm.Board.GetPiece(move.From)!, move.To, move.PromotionType), false);
            }

            RenderChessBoard(gm.Board, false);
            isInNavigationMode = true;
            return gm;
        }

        public async Task ShowNextMoveAsync()
        {
            if (!isNavigationAllowed)
                return;

            if (navigationCurrentMove == -1)
                navigationCurrentMove = game.Moves.Count;

            if (navigationCurrentMove >= game.Moves.Count)
                return;

            navigationCurrentMove++;

            var gm = await ShowMoveAsync();

            PlayMoveSound(gm, false);
            RenderChessBoard(gm.Board, false);
            isInNavigationMode = true;
        }

        private void PlayMoveSound(Game gm, bool isPreviousMove, MoveNotation? move = null)
        {
            var lastMove = move ?? gm.Moves.LastOrDefault();

            if (gm.IsCheck(PieceColor.White) || gm.IsCheck(PieceColor.Black))
                Sound.Play(Sound.SoundType.Check);
            else if (lastMove?.IsCapture == true)
            {
                if (!isPreviousMove)
                    Sound.Play(Sound.SoundType.Capture);
                else
                    Sound.Play(Sound.SoundType.Move);
            }
            else if (lastMove?.IsCastleKingSide == true || lastMove?.IsCastleQueenSide == true)
                Sound.Play(Sound.SoundType.Castle);
            else
                Sound.Play(Sound.SoundType.Move);
        }

        #endregion
    }
}