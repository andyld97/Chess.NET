using Chess.NET.Model;
using System.Diagnostics;
using System.IO;

namespace Chess.NET.Bot
{
    public class StockfischBot : IChessBot, IDisposable
    {
        private readonly Process _process;
        private readonly StreamWriter _input;
        private readonly StreamReader _output;

        public StockfischBot(string stockfishPath = "stockfish.exe")
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = stockfishPath,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            _process.Start();

            _input = _process.StandardInput;
            _output = _process.StandardOutput;

            InitUci();
        }

        private void InitUci()
        {
            Send("uci");
            WaitFor("uciok");

            Send("isready");
            WaitFor("readyok");
            Send("setoption name Skill Level value 8");
        }

        public (Piece, Position)? Move(Game game)
        {
            // 1️ Position setzen (über Moves!)
            var movesUci = string.Join(" ", game.Moves.Select(m => m.ToUci()));
            Send($"position startpos moves {movesUci}");

            // 2️ Engine rechnen lassen
            Send("go depth 10");

            // 3️ bestmove lesen
            string? line;
            while ((line = _output.ReadLine()) != null)
            {
                Debug.WriteLine($"[Stockfish] {line}"); 
                if (line.StartsWith("bestmove"))
                {
                    var parts = line.Split(' ');
                    if (parts.Length < 2 || parts[1] == "(none)")
                        return null; // Matt oder Patt

                    string uciMove = parts[1]; // z.B. e2e4
                    return MapUciMoveToGame(uciMove, game);
                }
            }

            return null;
        }

        private (Piece, Position)? MapUciMoveToGame(string uci, Game game)
        {
            // e2e4, e7e8q
            var from = Position.Parse(uci.Substring(0, 2));
            var to = Position.Parse(uci.Substring(2, 2));

            var piece = game.Board.GetPiece(from);
            if (piece == null)
                return null;

            return (piece, to);
        }

        private void Send(string command)
        {
            _input.WriteLine(command);
            _input.Flush();
        }

        private void WaitFor(string token)
        {
            string? line;
            while ((line = _output.ReadLine()) != null)
            {
                if (line.Contains(token))
                    return;
            }
        }

        public void Dispose()
        {
            try
            {
                Send("quit");
                _process.Kill();
            }
            catch { }
        }
    }
}
