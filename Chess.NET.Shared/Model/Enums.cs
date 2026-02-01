namespace Chess.NET.Shared.Model
{
    public enum SoundType
    {
        Move,
        Capture,
        Castle,
        Check,
        Checkmate,
        Stalemate,
        PuzzleFail,
        PuzzleSolved
    }

    public enum GameResult
    {
        Checkmate,
        Stalemate,
        Resign,
        TimeOver,
        FiftyMoveRule,
        ThreefoldReptition,
        Disconnected
    }
}
