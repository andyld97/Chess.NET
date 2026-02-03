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
        Timeout,       // not implemented yet
        FiftyMoveRule,
        ThreefoldReptition,
        Disconnected,
        InsufficentCheckmatingMaterial
    }
}
