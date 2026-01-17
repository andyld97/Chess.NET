using Chess.NET.Bot;
using Chess.NET.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chess.NET.Controls
{
    /// <summary>
    /// Interaktionslogik für ChessBoard.xaml
    /// </summary>
    public partial class ChessBoard : UserControl
    {
        private readonly BoardSquare[,] _squares = new BoardSquare[8, 8];
        private readonly Game game = new Game();

        private readonly IChessBot? chessBot = null;
        private bool isMirrored = false;

        public Game Game => game;

        public ChessBoard()
        {
            InitializeComponent();
            InitializeSquares();

            game = new Model.Game();
            game.StartNewGame();

            RefreshChessBoard(game.Board);

            chessBot = new StockfischBot(@"F:\Eigene Dateien\Downloads\stockfish-windows-x86-64\stockfish\stockfish-windows-x86-64.exe");
            // chessBot = new StupidoBot();
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

        public void Restart()
        {
            ResetDrag();

            game.StartNewGame();
            RefreshChessBoard(game.Board);
        }

        public void Mirror()
        {
            isMirrored = !isMirrored; // toggle
            RefreshChessBoard(game.Board);
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
            if (isInNavigationMode)
            {
                navigationCurrentMove = -1;
                isInNavigationMode = false;
                ResetDrag();
                RefreshChessBoard(game.Board, true);
                return;
            }

            var square = (sender as Border);
            var image = square.Child as Image;

            if (clickCounter > 0)
            {
                var destinationSquare = (Position)square.Tag;
                if (isMirrored)
                    destinationSquare = destinationSquare.Mirror();

                bool wasMoveAccepted = game.Move(new NextMove(_pieceToMove, destinationSquare, null));

                ResetDrag();
                RefreshChessBoard(game.Board);

                if (!wasMoveAccepted)
                    return;


                // THE BOOOOT
                await Task.Delay(1000);

                bool test = false;
                while (!test)
                {
                    if (game.IsCheckmate(PieceColor.Black) || game.IsCheckmate(PieceColor.White))
                        return;

                    if (game.IsStalemate(PieceColor.Black) || game.IsStalemate(PieceColor.White))
                        return;

                    var next = chessBot.Move(game);
                    if (next == null)
                        break;

                    test = game.Move(next);
                }

                RefreshChessBoard(game.Board);
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

        private void RefreshChessBoard(IBoard board, bool renderLastMoveSquares = true)
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

                    if (piece == null)
                        img.Source = null;
                    else
                    {
                        BitmapImage bi = new BitmapImage { CacheOption = BitmapCacheOption.OnLoad };
                        bi.BeginInit();
                        bi.UriSource = new Uri($"pack://application:,,,/Chess.NET;component/resources/icons/{(piece.Color == PieceColor.White ? "white" : "black")}/{piece.Type}.png");
                        bi.EndInit();
                        bi.Freeze();

                        img.Source = bi;
                    }

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

        public void ShowPreviousMove()
        {
            if (navigationCurrentMove == -1)
                navigationCurrentMove = game.Moves.Count;

            if (navigationCurrentMove == 0)
                return;

            int removedMoveIndex = navigationCurrentMove - 1;
            var removedMove = game.Moves[removedMoveIndex];

            navigationCurrentMove--;

            Game gm = new Game();
            gm.StartNewGame();

            for (int i = 0; i < navigationCurrentMove; i++)
            {
                var move = game.Moves[i];
                gm.Move(new NextMove(gm.Board.GetPiece(move.From), move.To, move.PromotionType), false);
            }

            PlayMoveSound(gm, true, removedMove);
            RefreshChessBoard(gm.Board, false);
            isInNavigationMode = true;
        }

        public void ShowNextMove()
        {
            if (navigationCurrentMove == -1)
                navigationCurrentMove = game.Moves.Count;
            else
                navigationCurrentMove++;

            if (navigationCurrentMove > game.Moves.Count)
            {
                navigationCurrentMove = game.Moves.Count;
                return;
            }

            Game gm = new Game();
            gm.StartNewGame();
            for (int i = 0; i < navigationCurrentMove; i++)
            {
                var move = game.Moves[i];
                gm.Move(new NextMove(gm.Board.GetPiece(move.From), move.To, move.PromotionType), false);
            }

            PlayMoveSound(gm, false);
            RefreshChessBoard(gm.Board, false);
            isInNavigationMode = true;
        }

        private void PlayMoveSound(Game gm, bool isPreviousMove, Move? move = null)
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

    public class BoardSquare
    {
        public int File { get; }

        public int Rank { get; }

        public Border Border { get; }

        public BoardSquare(int file, int rank, Border border)
        {
            File = file;
            Rank = rank;
            Border = border;
        }
    }
}
