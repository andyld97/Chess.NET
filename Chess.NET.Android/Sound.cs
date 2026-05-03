using Chess.NET.Shared.Model;

namespace Chess.NET.Android
{
    public static class Sound
    {
        private static Dictionary<SoundType, Plugin.Maui.Audio.IAudioPlayer> audioCache = [];

        public static async Task Play(SoundType type)
        {
            if (audioCache.TryGetValue(type, out var player))
            {
                player.Play();
                return;
            }

            string file = type switch
            {
                SoundType.Move => "move.mp3",
                SoundType.Capture => "capture.mp3",
                SoundType.Castle => "castle.mp3",
                SoundType.Check => "check.mp3",
                SoundType.Checkmate => "checkmate.mp3",
                SoundType.Stalemate => "stalemate.mp3",
                SoundType.PuzzleFail => "fail.mp3",
                SoundType.PuzzleSolved => "success.mp3",
                _ => throw new ArgumentOutOfRangeException()
            };

            var audioPlayer = Plugin.Maui.Audio.AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(file));
            audioCache.TryAdd(type, audioPlayer);
            audioPlayer.Play();
        }
    }
}

