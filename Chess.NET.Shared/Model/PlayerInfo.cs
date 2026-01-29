namespace Chess.NET.Shared.Model
{
    public readonly struct PlayerInfo
    {
        public int WhiteCapturePiecesMaterialValue { get; }

        public int BlackCapturePiecesMaterialValue { get; }

        public IReadOnlyCollection<Piece> WhiteCapturedPieces { get; }

        public IReadOnlyCollection<Piece> BlackCapturedPieces { get; }

        public PlayerInfo(int whiteCapturePiecesMaterialValue, int blackCapturePiecesMaterialValue, IReadOnlyCollection<Piece> whiteCapturedPieces, IReadOnlyCollection<Piece> blackCapturedPieces)
        {
            WhiteCapturePiecesMaterialValue = whiteCapturePiecesMaterialValue;
            BlackCapturePiecesMaterialValue = blackCapturePiecesMaterialValue;  
            WhiteCapturedPieces = whiteCapturedPieces;
            BlackCapturedPieces = blackCapturedPieces;
        }

        public string GetWhite()
        {
            string piecesString = string.Join("", WhiteCapturedPieces.Select(p => p.ToEmoji()));
            if (WhiteCapturePiecesMaterialValue > BlackCapturePiecesMaterialValue)
                piecesString += $" (+{Math.Abs(WhiteCapturePiecesMaterialValue - BlackCapturePiecesMaterialValue)})";

            return piecesString;
        }

        public string GetBlack()
        {
            string piecesString = string.Join("", BlackCapturedPieces.Select(p => p.ToEmoji()));
            if (BlackCapturePiecesMaterialValue > WhiteCapturePiecesMaterialValue)
                piecesString += $" (+{Math.Abs(WhiteCapturePiecesMaterialValue - BlackCapturePiecesMaterialValue)})";

            return piecesString;
        }
    }
}
