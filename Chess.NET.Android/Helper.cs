namespace Chess.NET.Android
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
    }
}
