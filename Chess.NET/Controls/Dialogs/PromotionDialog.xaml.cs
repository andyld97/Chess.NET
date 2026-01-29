using Chess.NET.Shared.Model;
using System.Windows;

namespace Chess.NET.Controls.Dialogs
{
    /// <summary>
    /// Interaktionslogik für ConversationDialog.xaml
    /// </summary>
    public partial class PromotionDialog : Window
    {
        public PieceType? PromotionResult { get; set; } = null;

        public PromotionDialog(Color color)
        {
            InitializeComponent();

            ImgQueen.Source = PieceType.Queen.ToBitmap(color);
            ImgRook.Source = PieceType.Rook.ToBitmap(color);
            ImgBishop.Source = PieceType.Bishop.ToBitmap(color);
            ImgKnight.Source = PieceType.Knight.ToBitmap(color);         
        }

        private void RadioQueen_Checked(object sender, RoutedEventArgs e)
        {
            PromotionResult = PieceType.Queen;
            DialogResult = true;
        }

        private void RadioRook_Checked(object sender, RoutedEventArgs e)
        {
            PromotionResult = PieceType.Rook;
            DialogResult = true;
        }

        private void RadioKnight_Checked(object sender, RoutedEventArgs e)
        {
            PromotionResult = PieceType.Knight;
            DialogResult = true;
        }

        private void RadioBishop_Checked(object sender, RoutedEventArgs e)
        {
            PromotionResult = PieceType.Bishop;
            DialogResult = true;
        }
    }
}