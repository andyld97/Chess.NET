using Chess.NET.Shared.Model;
using Microsoft.Maui.Controls.Shapes;
using Image = Microsoft.Maui.Controls.Image;
using Chess.NET.Shared.Model.GUI;
using Chess.NET.Shared.Model.Bot;
#if ANDROID
using A = Android;
#endif

namespace Chess.NET.Android.Controls;

public partial class ChessBoard : ContentView
{
    private bool isMirrored = false;
    private bool ignoreTapping = false;
    private bool isOnline = false;
    private Shared.Model.Color? playerOnlineColor = null;

    private Game game = new Game();

    private IChessBot? opponent = null;

    private Piece? _pieceToMove = null;
    private readonly BoardSquare<Grid>[,] _squares = new BoardSquare<Grid>[8, 8];

    public delegate void onMoveMadeOnline(MoveNotation moveNotation);
    public event onMoveMadeOnline? OnMoveMadeOnline;

    public bool IsMirrored => isMirrored;

    public Game Game => game;

    public ChessBoard()
    {
        InitializeComponent();

        MainBorder.IsVisible = false;

        SizeChanged += (_, _) =>
        {
            if ((int)Width == (int)Height)
                MainBorder.IsVisible = true;

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
        game.OnGameOver += Game_OnGameOver;
        game.StartNewGame(null);
        RenderChessBoard(game.Board, false);        
    }

    private void Game_OnGameOver(GameResult result, Shared.Model.Color? colorWon)
    {
        if (isOnline)
            return;

#if ANDROID
        A.Widget.Toast.MakeText(A.App.Application.Context, $"Game Over: {result}. Won: {(colorWon == null ? "-" : colorWon.ToString())}", A.Widget.ToastLength.Long).Show();
#endif
    }

    public void Mirror()
    {
        isMirrored = !isMirrored; // toggle
        InitializeSquares();
        RenderChessBoard(game.Board);
    }

    public void Restart(IChessBot? opponent)
    {
        this.opponent = opponent;
        game.StartNewGame(opponent);
        RenderChessBoard(game.Board, false);
        ClearMoveIndicators();
        isOnline = false;
    }

    public void SetOnline(Shared.Model.Color pieceColor)
    {
        isOnline = true;
        playerOnlineColor = pieceColor;
    }

    public void ResetOnline()
    {
        isOnline = false;
        playerOnlineColor = null;
    }


    private async void Game_OnPlaySound(SoundType type)
    {
        await Sound.Play(type);
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

                var piece = board.GetPiece(position);

                Image? img = (Image?)_squares[file - 1, rank - 1].Border.Children.FirstOrDefault();
                img?.Source = (piece != null) ? GetImage(piece.Type, piece.Color) : null; 

                if (renderLastMoveSquares && (lastMove != null && (lastMove.From == position || lastMove.To == position)))
                {
                    // Highlight last move squares  
                    _squares[file - 1, rank - 1].Border.Background = (Brush?)Application.Current?.Resources["ChessHighlightSquare"];
                }
                else
                {
                    bool dark = (file + (isMirrored ? (9 - rank) : rank)) % 2 == 0;
                    _squares[file - 1, rank - 1].Border.Background = (Brush?)Application.Current?.Resources[dark ? "ChessDarkSquare" : "ChessLightSquare"];
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
                    Background = (Brush?)Application.Current?.Resources[dark ? "ChessDarkSquare" : "ChessLightSquare"],
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
                    Stroke = new SolidColorBrush(Microsoft.Maui.Graphics.Color.FromArgb("#202D40")),
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

    #endregion

    #region Moving

    private async void Tap_Tapped(object? sender, TappedEventArgs e)
    {
        if (ignoreTapping)
            return;

        if (game.IsGameOver)
        {
            ClearMoveIndicators();
            _pieceToMove = null;
            return;
        }

        var position = (sender as Grid)?.BindingContext as Position;
        if (position == null)
            return;

        if (isMirrored)
            position = position.Mirror();

        if (_pieceToMove != null)
        {
            bool doNotMove = false;

            if (_pieceToMove.Color != game.PlayersTurn)
            {
                ClearMoveIndicators();
                _pieceToMove = null;
                return;
            }
            else
            {
                // If a different piece is selected switch over to this piece 
                // In this case it should be the right color so you cannot capture your own pieces
                var targetSquarePiece = game.Board.GetPiece(position);

                if (targetSquarePiece == _pieceToMove)
                {
                    ClearMoveIndicators();
                    _pieceToMove = null;
                    return;
                }

                if (targetSquarePiece != null && targetSquarePiece.Color == _pieceToMove.Color)
                {
                    doNotMove = true;
                    ClearMoveIndicators();
                }
            }

            if (!doNotMove)
            {
                var success = game.Move(new PendingMove(_pieceToMove, position, PieceType.Queen));
                if (success)
                {
                    RenderChessBoard(game.Board, true);
                    ClearMoveIndicators();
                    _pieceToMove = null;

                    if (isOnline)
                    {
                        OnMoveMadeOnline?.Invoke(game.Moves.LastOrDefault()!);
                        return;
                    }

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

                            foundValidMove = game.Move(next);
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

        if (isOnline && playerOnlineColor != piece.Color)
        {
            ClearMoveIndicators();
            _pieceToMove = null;
            return; // illegal drag
        }

        _pieceToMove = piece;
        var moves = piece.GetPossibleMoves(game);       
        foreach (var square in _squares)
        {         
            var border = (square.Border.Children[1] as Border);

            var pos = new Position(square.File, square.Rank);
            if (isMirrored)
                pos = pos.Mirror();

            bool isVisible = moves.Any(p => p.Rank == pos.Rank && p.File == pos.File && !game.IsCheck(_pieceToMove.Color, _pieceToMove, p));            
            
            if (isVisible && game.Board.GetPiece(pos) != null && square.Border.Children.FirstOrDefault() is Image img)
            {
                // Gray out pieces a bit
                img.Opacity = 0.4;
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