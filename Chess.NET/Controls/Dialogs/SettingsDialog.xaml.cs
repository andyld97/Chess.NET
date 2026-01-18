using Chess.NET.Model;
using Microsoft.Win32;
using System;
using System.Windows;

namespace Chess.NET.Controls.Dialogs
{
    /// <summary>
    /// Interaktionslogik für SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private readonly bool isInitialized = false;

        public SettingsDialog()
        {
            InitializeComponent();

            TextPlayer1Name.Text = Settings.Instance.Player1Name;
            TextPlayer2Name.Text = Settings.Instance.Player2Name;
            TextPlayer1Elo.Text = Settings.Instance.Player1Elo;
            TextPlayer2Elo.Text = Settings.Instance.Player2Elo;
            TextStockfishPath.Text = Settings.Instance.StockfishPath;
            ChkAutoPromote.IsChecked = Settings.Instance.AutoPromoteToQueen;
            ChkPlaySounds.IsChecked = Settings.Instance.PlaySounds;
            CmbDifficuluty.SelectedIndex = Settings.Instance.Difficulty;

            isInitialized = true;
        }

        private void TextPlayer1Name_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!isInitialized)
                return;

            Settings.Instance.Player1Name = TextPlayer1Name.Text;
            Settings.Instance.Save();
        }

        private void TextPlayer2Name_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!isInitialized)
                return;

            Settings.Instance.Player2Name = TextPlayer2Name.Text;
            Settings.Instance.Save();
        }

        private void TextStockfishPath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!isInitialized)
                return;

            Settings.Instance.StockfishPath = TextStockfishPath.Text;   
            Settings.Instance.Save();
        }

        private void TextPlayer1Elo_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!isInitialized)
                return;

            Settings.Instance.Player1Elo = TextPlayer1Elo.Text;
            Settings.Instance.Save();
        }

        private void TextPlayer2Elo_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!isInitialized)
                return;

            Settings.Instance.Player2Elo = TextPlayer2Elo.Text;
            Settings.Instance.Save();
        }

        private void ButtonOpenFileDialog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Executable Files (*.exe)|*.exe";

            if (ofd.ShowDialog() == true) 
                TextStockfishPath.Text = ofd.FileName;  
        }

        private void ChkAutoPromote_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Instance.AutoPromoteToQueen = ChkAutoPromote.IsChecked!.Value;
            Settings.Instance.Save();
        }

        private void ChkPlaySounds_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Instance.PlaySounds = ChkPlaySounds.IsChecked!.Value;
            Settings.Instance.Save();
        }

        private void CmbDifficuluty_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!isInitialized)
                return;

            Settings.Instance.Difficulty = CmbDifficuluty.SelectedIndex;
            Settings.Instance.Save();
        }

        private void Link_Click(object sender, RoutedEventArgs e)
        {
            Helper.OpenHyperlink(Link.NavigateUri.ToString());
        }
    }
}
