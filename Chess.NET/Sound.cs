using Chess.NET.Model;
using Chess.NET.Shared.Model;
using System.Windows.Media;

namespace Chess.NET
{
    public static class Sound
    {
        private static readonly MediaPlayer _movePlayer = CreatePlayer("resources/sounds/move.mp3");
        private static readonly MediaPlayer _capturePlayer = CreatePlayer("resources/sounds/capture.mp3");
        private static readonly MediaPlayer _castlePlayer = CreatePlayer("resources/sounds/castle.mp3");
        private static readonly MediaPlayer _checkPlayer = CreatePlayer("resources/sounds/check.mp3");
        private static readonly MediaPlayer _checkMatePlayer = CreatePlayer("resources/sounds/checkmate.mp3");
        private static readonly MediaPlayer _staleMatePlayer = CreatePlayer("resources/sounds/stalemate.mp3");
        private static readonly MediaPlayer _puzzleSolvedPlayer = CreatePlayer("resources/sounds/solved.mp3");
        private static readonly MediaPlayer _puzzleFailedPlayer = CreatePlayer("resources/sounds/fail.mp3");

        private static MediaPlayer CreatePlayer(string relativePath)
        {
            var player = new MediaPlayer();
            player.Open(new Uri(relativePath, UriKind.Relative));
            player.Volume = 0.8;
            return player;
        }

        public static void Play(SoundType type)
        {
            if (!Settings.Instance.PlaySounds)
                return;

            Dictionary<SoundType, MediaPlayer> soundPlayers = new()
            {
                { SoundType.Move, _movePlayer },
                { SoundType.Capture, _capturePlayer },
                { SoundType.Castle, _castlePlayer },
                { SoundType.Check, _checkPlayer },
                { SoundType.Checkmate, _checkMatePlayer },
                { SoundType.Stalemate, _staleMatePlayer  },
                { SoundType.PuzzleSolved, _puzzleSolvedPlayer  },
                { SoundType.PuzzleFail, _puzzleFailedPlayer  }  ,
            };

            if (soundPlayers.TryGetValue(type, out MediaPlayer? player))
                Play(player);
        }

        private static void Play(MediaPlayer player)
        {
            player.Stop();
            player.Position = TimeSpan.Zero;
            player.Play();
        }
    }
}
