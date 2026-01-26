using Chess.NET.Bot;
using Chess.NET.Model;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Chess.NET.Controls.MoveNotationDisplay;

namespace Chess.NET.Controls.Dialogs
{
    /// <summary>
    /// Interaktionslogik für PuzzleDialog.xaml
    /// </summary>
    public partial class PuzzleDialog : Window
    {
        private readonly StupidoBot stupidoBot = new StupidoBot();
        private int currentPuzzleMove = 0;
        private readonly Puzzle currentPuzzle = null!;
        private bool ignoreMoveEvent = false;

        private const int NEXT_MOVE_DELAY = 600; // [ms.]

        #region Commands

        public ICommand MoveLeftCommand { get; } = null!;

        public ICommand MoveRightCommand { get; } = null!;

        public ICommand MirrorBoardCommand { get; } = null!;

        #endregion

        public PuzzleDialog(Puzzle puzzle)
        {
            InitializeComponent();
            DataContext = this;

            currentPuzzle = puzzle;
            if (currentPuzzle == null)
                return;

            // Load puuzzle
            Chessboard.LoadPuzzle(currentPuzzle);

            // Assign events
            Chessboard.Game.OnMovedPiece += Game_MovedPiece;
            Chessboard.Game.OnPlaySound += Game_OnPlaySound;

            // Assign commands
            MoveLeftCommand = new RelayCommand(async () => await Chessboard.ShowPreviousMoveAsync());
            MoveRightCommand = new RelayCommand(async () => await Chessboard.ShowNextMoveAsync());
            MirrorBoardCommand = new RelayCommand(new Action(Chessboard.Mirror));

            Title = $"{Properties.Resources.strPuzzle} - {currentPuzzle.Name}";
        }

        private void Game_OnPlaySound(Sound.SoundType type)
        {
            if (currentPuzzle.SolveType == PuzzleSolved.Checkmate && type == Sound.SoundType.Checkmate)
                return;

            if (currentPuzzle.SolveType == PuzzleSolved.Stalemate && type == Sound.SoundType.Stalemate)
                return;

            Sound.Play(type);
        }

        private async void Game_MovedPiece(MoveNotation move)
        {
            if (ignoreMoveEvent)
            {
                ignoreMoveEvent = false;
                return;
            }

            if (move.FormatMove(false) != currentPuzzle!.Moves[currentPuzzleMove])
                await HandleWrongMoveAsync();
            else
                await HandleCorrectMoveAsync(move);
        }

        private async Task HandleCorrectMoveAsync(MoveNotation lastMove)
        {
            // Play next move
            currentPuzzleMove++;

            await Task.Delay(NEXT_MOVE_DELAY);

            if (currentPuzzleMove >= currentPuzzle!.Moves.Count)
            {
                // Puzzle successfully done! :)
                Sound.Play(Sound.SoundType.PuzzleSolved);
                PanelPuzzleSolved.Visibility = Visibility.Visible;

                // Render moves
                ListMoves.Items.Clear();

                MoveNotationDisplay start = new MoveNotationDisplay(true);
                start.OnJumpToMove += Display_OnJumpToMove;
                ListMoves.Items.Add(start);

                MoveNotationDisplay display = new MoveNotationDisplay();
                display.OnJumpToMove += Display_OnJumpToMove;
                foreach (var move in Chessboard.Game.Moves)
                {
                    if (display.Move1 == null)
                        display.Move1 = move;
                    else if (display.Move2 == null)
                    {
                        display.Move2 = move;
                        ListMoves.Items.Add(display);
                        display = new MoveNotationDisplay();
                        display.OnJumpToMove += Display_OnJumpToMove;
                    }
                }

                if (display.Move2 == null)
                    ListMoves.Items.Add(display);

                Chessboard.EnableNavigation();
                return;
            }

            string nextMove = currentPuzzle.Moves[currentPuzzleMove];
            var pendingPuzzleMove = PendingMove.Parse(nextMove, (Board)Chessboard.Game.Board, Helper.InvertPieceColor(lastMove.Piece.Color));

            ignoreMoveEvent = true;
            await Chessboard.Game.MoveAsync(pendingPuzzleMove);
            Chessboard.RenderChessBoard(Chessboard.Game.Board);
            currentPuzzleMove++;
        }
        
        private async void Display_OnJumpToMove(int index)
        {
            if (index == -1)
            {
                await Chessboard.JumpToStartAsync();
                return;
            }

            await Chessboard.ShowMoveAsync(index);
        }

        private async Task HandleWrongMoveAsync()
        {
            // Let stupido bot execute a move and then notify puzzle failed!!      
            await Task.Delay(NEXT_MOVE_DELAY);

            var pendingBotMove = stupidoBot.Move(Chessboard.Game);
            int counter = 0;
            while (!Chessboard.Game.IsMoveValid(pendingBotMove!.Piece, pendingBotMove.To))
            {
                if (counter >= 10)
                {
                    pendingBotMove = null;
                    break;
                }

                pendingBotMove = stupidoBot.Move(Chessboard.Game);
                counter++;
            }

            if (pendingBotMove != null)
            {
                ignoreMoveEvent = true;
                await Chessboard.Game.MoveAsync(pendingBotMove, true);
                Chessboard.RenderChessBoard(Chessboard.Game.Board, true);
            }

            Chessboard.DisablePieces();

            Sound.Play(Sound.SoundType.PuzzleFail);
            PanelPuzzleFailed.Visibility = Visibility.Visible;
        }

        private void ButtonRetry_Click(object sender, RoutedEventArgs e)
        {
            PanelPuzzleFailed.Visibility = Visibility.Collapsed;
            Chessboard.LoadPuzzle(currentPuzzle);
            Chessboard.RenderChessBoard(Chessboard.Game.Board, false);
            Chessboard.DisableNavigation();
            Chessboard.EnablePieces();
            currentPuzzleMove = 0;
        }

        private async void ButtonReplay_Click(object sender, RoutedEventArgs e)
        {
            ButtonReplay.IsEnabled = false;

            Chessboard.EnableNavigation();
            Chessboard.DisablePieces();

            const int delay = 1500;

            await Chessboard.JumpToStartAsync();

            await Task.Delay(delay);

            while (await Chessboard.ShowNextMoveAsync())
                await Task.Delay(delay);

            ButtonReplay.IsEnabled = true;  
        }
    }
}