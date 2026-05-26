namespace Chess.NET.Android
{
    // This is just a temporary workaround until we have a settings dialog!
    public static class Settings
    {
        public static class Instance
        {
            public static string Player1Elo = "500";
            public static string Player2Elo = "500";

            public static string Player1Name => Preferences.Get("Player1", "Player 1");

            public static string Player2Name => Preferences.Get("Player2", "Player 2");

            public static int LastSelectedGameMode => Preferences.Get("LastSelectedGameMode", 0);
        }
    }
}
