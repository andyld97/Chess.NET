using Chess.NET.Netcode;
using Chess.NET.Shared.Model.Online;
using System.Windows;

namespace Chess.NET.Controls.Dialogs
{
    /// <summary>
    /// Interaktionslogik für WaitingQueueDialog.xaml
    /// </summary>
    public partial class WaitingQueueDialog : Window
    {
        public Client? Client { get; set; } = null;

        public bool FoundMatch { get; set; } = false;

        public WaitingQueueDialog()
        {
            InitializeComponent();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (FoundMatch)
                return;

            if (Client != null)
                await APIClient.LeaveQueueAsync(Client);
        }
    }
}
