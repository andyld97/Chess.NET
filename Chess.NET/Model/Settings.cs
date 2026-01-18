using System.Windows;

namespace Chess.NET.Model
{
    public class Settings
    {
        private static readonly string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chess.NET", "Settings.xml");

        public static Settings Instance = Settings.Load();

        static Settings()
        {
            try
            {
                var parent = System.IO.Path.GetDirectoryName(path);
                if (!System.IO.Directory.Exists(parent))
                {
                    System.IO.Directory.CreateDirectory(parent!);
                }
            }
            catch
            {
                // ignore
            }
        }

        public string StockfishPath { get; set; } = "stockfish.exe";

        public string Player1Name { get; set; } = string.Empty;

        public string Player1Elo { get; set; } = string.Empty;

        public string Player2Name { get; set; } = string.Empty;

        public string Player2Elo { get; set; } = string.Empty;

        public int Difficulty { get; set; } = 0;

        public bool AutoPromoteToQueen { get; set; } = false;

        public bool PlaySounds { get; set; } = true;

        public static Settings Load()
        {
            try
            {
                var result = Serialization.Read<Settings>(path, Serialization.Mode.XML);
                if (result != null)
                    return result;
            }
            catch (Exception e)
            {
                MessageBox.Show($"{Properties.Resources.strFailedToLoadSettings}{Environment.NewLine}{Environment.NewLine}{e.Message}", Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new Settings();
        }

        public void Save()
        {
            try
            {
                Serialization.Save<Settings>(path, this, Serialization.Mode.XML);
            }
            catch (Exception e)
            {
                MessageBox.Show($"{Properties.Resources.strFailedToSaveSettings}{Environment.NewLine}{Environment.NewLine}{e.Message}", Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}