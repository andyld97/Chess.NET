using Chess.NET.Model;
using Chess.NET.Shared.Model;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Chess.NET
{
    public static class Helper
    {
        public static string GetPlayerName(int player)
        {
            if (player == 1)
            {
                if (!string.IsNullOrEmpty(Settings.Instance.Player1Name))
                    return Settings.Instance.Player1Name;

                return Properties.Resources.strPlayer1;
            }
            else if (player == 2)
            {
                if (!string.IsNullOrEmpty(Settings.Instance.Player2Name))
                    return Settings.Instance.Player2Name;

                return Properties.Resources.strPlayer2;
            }

            return Properties.Resources.strPlayer1;
        }

        public static void OpenHyperlink(string url)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", $"\"{url}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.strFailedToOpenHyperlink, url, ex.Message), Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static Dictionary<PieceType, BitmapImage> bitmapCacheWhite = [];
        private static Dictionary<PieceType, BitmapImage> bitmapCacheBlack = [];

        public static BitmapImage ToBitmap(this PieceType pieceType, Color color)
        {
            if (color == Color.White && bitmapCacheWhite.TryGetValue(pieceType, out BitmapImage? value))
                return value;
            else if (color == Color.Black && bitmapCacheBlack.TryGetValue(pieceType, out BitmapImage? value1))
                return value1;

            string col = (color == Color.White ? "white" : "black");

            BitmapImage bi = new BitmapImage { CacheOption = BitmapCacheOption.OnLoad };
            bi.BeginInit();
            bi.UriSource = new Uri($"pack://application:,,,/Chess.NET;component/resources/icons/{col}/{pieceType}.png");
            bi.EndInit();
            bi.Freeze();

            // Add to cache
            if (color == Color.White)
                bitmapCacheWhite.Add(pieceType, bi);
            else
                bitmapCacheBlack.Add(pieceType, bi);

            return bi;
        }
    }
}
