#if ANDROID
using A = Android;
using Android.Media;
#endif
using Chess.NET.Shared.Model;
using Microsoft.Maui.Controls.Shapes;
using Image = Microsoft.Maui.Controls.Image;
using System.Reflection;
using Chess.NET.Shared.Model.GUI;
using Chess.NET.Shared.Model.Bot;

namespace Chess.NET.Android.Controls;

public partial class ChessBoard : ContentView
{
    private bool isMirrored = false;
    private Game game = new Game();

    private IChessBot? opponent = null;

    private Piece? _pieceToMove = null;
    private readonly BoardSquare<Grid>[,] _squares = new BoardSquare<Grid>[8, 8];

    public ChessBoard()
    {
        InitializeComponent();

        SizeChanged += (_, _) =>
        {
            var size = Math.Round(Math.Min(Width, Height));
            MainBorder.WidthRequest = size;
            MainBorder.HeightRequest = size;
        };

        InitializeGame();
    }

    private void InitializeGame()
    {
        InitializeSquares();

        game = new Game();
        game.OnPlaySound += Game_OnPlaySound;
        game.StartNewGame(null);
        RenderChessBoard(game.Board, false);

        opponent = new StupidoBot();
    }

    private async void Game_OnPlaySound(SoundType type)
    {
        string file = type switch
        {
            SoundType.Move => "move.mp3",
            SoundType.Capture => "capture.mp3",
            SoundType.Castle => "castle.mp3",
            SoundType.Check => "check.mp3",
            SoundType.Checkmate => "checkmate.mp3",
            SoundType.Stalemate => "stalemate.mp3",
            SoundType.PuzzleFail => "fail.mp3",
            SoundType.PuzzleSolved => "success.mp3",
            _ => throw new ArgumentOutOfRangeException()
        };

        var audioPlayer = Plugin.Maui.Audio.AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(file));
        audioPlayer.Play();
    }

    #region Rendering

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

                Image? img = (Image)_squares[file - 1, rank - 1].Border.Children.FirstOrDefault();

                var piece = board.GetPiece(position);
                img.Source = (piece != null) ? GetImage(piece.Type, piece.Color) : null; // piece.Type.ToBitmap(piece.Color, Settings.Instance.Theme) : null;

                if (renderLastMoveSquares && (lastMove != null && (lastMove.From == position || lastMove.To == position)))
                {
                    // Highlight last move squares  
                    _squares[file - 1, rank - 1].Border.Background = (Brush)Application.Current.Resources["ChessHighlightSquare"];
                }
                else
                {
                    bool dark = (file + (isMirrored ? (9 - rank) : rank)) % 2 == 0;
                    _squares[file - 1, rank - 1].Border.Background = (Brush)Application.Current.Resources[dark ? "ChessDarkSquare" : "ChessLightSquare"];
                }
            }
        }
    }

    private static ImageSource GetImage(PieceType type, Shared.Model.Color color)
    {
        string col = color.ToString().ToLower();
        return ImageSource.FromFile($"themes/default/{col}/{type.ToString().ToLower()}_{col}.png");
    }

    private void InitializeSquares()
    {
        BoardGrid.Children.Clear();

        for (int rank = 8; rank >= 1; rank--)
        {
            for (int file = 1; file <= 8; file++)
            {
                bool dark = (file + (isMirrored ? (9 - rank) : rank)) % 2 == 0;

                var square = new Grid
                {
                    Background = (Brush)Application.Current.Resources[dark ? "ChessDarkSquare" : "ChessLightSquare"],
                    RowSpacing = 0,
                    ColumnSpacing = 0   
                };

                if (isMirrored)
                    Grid.SetColumn(square, 8 - file);
                else
                    Grid.SetColumn(square, file - 1);

                Grid.SetRow(square, 8 - rank);

                var image = new Image() { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
#if WINDOWS
                image.Margin = new Thickness(10);
#elif ANDROID
                image.Margin = new Thickness(5);
#endif

                square.Children.Add(image);
                var circle = new Border
                {
                    Opacity = 1,
                    StrokeShape = new Ellipse(),
                    StrokeThickness = 3,
                    Stroke = new SolidColorBrush(Colors.Gray),
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    InputTransparent = true,
                    IsVisible = false,
                    Margin = new Thickness(15),
                };

                square.Children.Add(circle);
                square.BindingContext = new Position(file, rank);
                var tap = new TapGestureRecognizer();
                tap.Tapped += Tap_Tapped;
                square.GestureRecognizers.Add(tap);
                BoardGrid.Children.Add(square);

                _squares[file - 1, rank - 1] = new BoardSquare<Grid>(file, rank, square);
            }
        }
    }

    private bool ignoreTapping = false;

    private async void Tap_Tapped(object? sender, TappedEventArgs e)
    {
        if (ignoreTapping)
            return;

        var position = (sender as Grid)?.BindingContext as Position;
        if (position == null)
            return;

        if (_pieceToMove != null)
        {
            if (_pieceToMove.Color != game.PlayersTurn || game.IsGameOver)
            {
                ClearMoveIndicators();
                _pieceToMove = null;
                return;

            }
            var success = await game.MoveAsync(new PendingMove(_pieceToMove, position, null));
            if (success)
            {
                RenderChessBoard(game.Board, true);
                ClearMoveIndicators();
                _pieceToMove = null;

                if (opponent != null)
                {
                    ignoreTapping = true;
                    await Task.Delay(1000);

                    bool foundValidMove = false;
                    while (!foundValidMove)
                    {
                        if (game.IsGameOver)
                        {
                            ignoreTapping = false;
                            return;
                        }

                        var next = opponent.Move(game);
                        if (next == null)
                            break;

                        foundValidMove = await game.MoveAsync(next);
                    }
                }

                RenderChessBoard(game.Board);
                ignoreTapping = false;
                return;
            }
            else
            {
                ClearMoveIndicators();
                _pieceToMove = null;
                return;
            }
        }

        var piece = game.Board.GetPiece(position);
        if (piece == null)
            return;

        if (piece.Color != game.PlayersTurn || game.IsGameOver)
        {
            ClearMoveIndicators();
            _pieceToMove = null;
            return;
        }

        _pieceToMove = piece;
        var moves = piece.GetPossibleMoves(game);       
        foreach (var square in _squares)
        {         
            var border = (square.Border.Children[1] as Border);
            bool isVisible = moves.Any(p => p.Rank == square.Rank && p.File == square.File && !game.IsCheck(_pieceToMove.Color, _pieceToMove, p));

            var pos = new Position(square.File, square.Rank);
            if (isVisible && game.Board.GetPiece(pos) != null)
            {
                // Gray out pieces a bit
                (square.Border.Children.FirstOrDefault() as Image).Opacity = 0.4;
            }

            border?.IsVisible = isVisible;
        }
    }

    private void ClearMoveIndicators()
    {
        foreach (var square in _squares)
        {
            (square.Border.Children[0] as Image)?.Opacity = 1;
            (square.Border.Children[1] as Border)?.IsVisible = false;
        }
    }   

    #endregion
}