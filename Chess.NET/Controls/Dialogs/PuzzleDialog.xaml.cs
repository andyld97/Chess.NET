using Chess.NET.Bot;
using Chess.NET.Model;
using System.Windows;

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

        public PuzzleDialog(Puzzle puzzle)
        {
            InitializeComponent();

            currentPuzzle = puzzle;
            if (currentPuzzle == null)
                return;

            Chessboard.LoadPuzzle(currentPuzzle);
            Chessboard.Game.OnMovedPiece += Game_MovedPiece;
            Chessboard.Game.OnPlaySound += Game_OnPlaySound;

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
                // TODO Puzzle successfully done! :)
                Sound.Play(Sound.SoundType.PuzzleSolved);

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

            // TODO Display wrong move!
            Sound.Play(Sound.SoundType.PuzzleFail);
        }
    }
}