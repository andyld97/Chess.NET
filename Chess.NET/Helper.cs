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

        private static readonly Dictionary<PieceType, BitmapImage> bitmapCacheWhite = [];
        private static readonly Dictionary<PieceType, BitmapImage> bitmapCacheBlack = [];

        public static void ClearBitmapCache()
        {
            bitmapCacheWhite.Clear();
            bitmapCacheBlack.Clear();
        }

        public static BitmapImage ToBitmap(this PieceType pieceType, Color color, Theme theme)
        {
            if (color == Color.White && bitmapCacheWhite.TryGetValue(pieceType, out BitmapImage? value))
                return value;
            else if (color == Color.Black && bitmapCacheBlack.TryGetValue(pieceType, out BitmapImage? value1))
                return value1;

            string col = (color == Color.White ? "white" : "black");
            string appName = "Chess.NET";
#if STORE
            appName = "OpenChess";
#endif

            BitmapImage bi = new BitmapImage { CacheOption = BitmapCacheOption.OnLoad };
            bi.BeginInit();
            bi.UriSource = new Uri($"pack://application:,,,/{appName};component/resources/icons/themes/{theme.ToString().ToLower()}/{col}/{pieceType}.png");
            bi.EndInit();
            bi.Freeze();

            // Add to cache
            if (color == Color.White)
                bitmapCacheWhite.Add(pieceType, bi);
            else
                bitmapCacheBlack.Add(pieceType, bi);

            return bi;
        }

        public static BitmapImage GetBackground(Background background)
        {
            string appName = "Chess.NET";
#if STORE
            appName = "OpenChess";
#endif
            string backgroundUri = background switch
            {
                Background.Sand =>              $"pack://application:,,,/{appName};component/resources/backgrounds/sand.jpg",
                Background.Abstract =>          $"pack://application:,,,/{appName};component/resources/backgrounds/abstract.png",
                Background.AbstractPurple =>    $"pack://application:,,,/{appName};component/resources/backgrounds/abstract-purple.jpg",
                _ => throw new ArgumentOutOfRangeException(nameof(background), background, null)
            };

            BitmapImage bi = new BitmapImage { CacheOption = BitmapCacheOption.OnLoad };
            bi.BeginInit();
            bi.UriSource = new Uri(backgroundUri);
            bi.EndInit();
            bi.Freeze();
            return bi;
        }
    }
}
