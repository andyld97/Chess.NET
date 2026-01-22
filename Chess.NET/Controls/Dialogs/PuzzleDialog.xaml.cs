using Chess.NET.Model;
using System.Windows;

namespace Chess.NET.Controls.Dialogs
{
    /// <summary>
    /// Interaktionslogik für PuzzleDialog.xaml
    /// </summary>
    public partial class PuzzleDialog : Window
    {
        public PuzzleDialog()
        {
            InitializeComponent();

            var puzzle = Puzzle.Puzzles[1]; //.FirstOrDefault();
            if (puzzle == null)
                return;

            Chessboard.LoadPuzzle(puzzle);
            Title = $"Puzzle - {puzzle.Name}";
        }
    }
}
