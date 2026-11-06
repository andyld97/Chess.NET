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

    public enum Theme
    {
        Default = 0,
        Comic = 1
    }

    public enum Background
    {
        None,
        Sand,
        Abstract,
        AbstractPurple
    }

    public enum TimeControl
    {
        Bullet1,
        Blitz3,
        Blitz5,
        Rapid10,
        Rapid15,
        Classical30,
        Classical60,
    }
}
