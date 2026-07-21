using Chess.NET.Shared.Model;
using SkiaSharp;
using System.Diagnostics;

namespace Chess.NET.Shared
{
    public class Renderer
    {
        private readonly Dictionary<string, SKBitmap> _cache = [];

        private readonly int boardSize = 8;
        private readonly int squareSize;
        private readonly Position? highlightSquare1 = null;
        private readonly Position? hightlightSquare2 = null;

        private readonly SKPaint lightSquare = new SKPaint
        {
            Color = new SKColor(240, 217, 181),
            IsAntialias = true
        };

        private readonly SKPaint darkSquare = new SKPaint
        {
            Color = new SKColor(181, 136, 99),
            IsAntialias = true
        };

        private readonly SKPaint highlightSquare = new SKPaint
        {
            Color = new SKColor(247, 219, 105),
            IsAntialias = true
        };

        private readonly SKPaint playerAreaBackground = new SKPaint
        {
            Color = new SKColor(92, 64, 51),
            IsAntialias = true
        };

        public Renderer(int squareSize = 64, Position? hightlightSquare1 =null, Position? hightlightSquare2 = null)
        {
            this.squareSize = squareSize;
            this.highlightSquare1 = hightlightSquare1;
            this.hightlightSquare2 = hightlightSquare2;
        }

        public byte[] Render(Board board, string theme, string player1Name, string player1Elo, string player2Name, string player2Elo)
        {
            const int playerAreaOffset = 50;

            int fieldSize = boardSize * squareSize;
            int width = fieldSize;
            int height = fieldSize + (2 * playerAreaOffset);

            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);

            canvas.Clear(SKColors.Black);

            DrawBoard(canvas, playerAreaOffset);
            DrawPieces(canvas, board, playerAreaOffset, theme);

            DrawPlayerArea(canvas, width, 0, playerAreaOffset, player2Name, player2Elo);
            DrawPlayerArea(canvas, width, height - playerAreaOffset, playerAreaOffset, player1Name, player1Elo);

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            return data.ToArray();
        }

        private void DrawPlayerArea(SKCanvas canvas, int width, int yOffset, int playerAreaSize, string playerName, string playerElo)
        {
            const int margin = 5;
        
            canvas.DrawRect(new SKRect(0, yOffset, width, yOffset + playerAreaSize), playerAreaBackground);
            
            yOffset += margin;

            using var font = new SKFont { Size = 16 };
            using var paint = new SKPaint() { Color = SKColors.White, IsAntialias = true };

            float nameBaseline = yOffset + font.Size;
            float eloBaseline = nameBaseline + font.Size + 4;

            canvas.DrawText(TruncateText(playerName, font, paint, width - (2 * margin)), new SKPoint(2 * margin, nameBaseline), font, paint);
            canvas.DrawText(TruncateText($"Elo: {playerElo}", font, paint, width - (2 * margin)), new SKPoint(2 * margin, eloBaseline), font, paint);
        }

        private void DrawBoard(SKCanvas canvas, int yOffset)
        {
            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    var paint = (x + y) % 2 == 0 ? lightSquare : darkSquare;

                    var pos = new Position(x + 1, y + 1);
                    pos = pos.Mirror();

                    if (pos == highlightSquare1 || pos == hightlightSquare2)
                        canvas.DrawRect(x * squareSize, yOffset + (y * squareSize), squareSize, squareSize, highlightSquare);
                    else 
                        canvas.DrawRect(x * squareSize, yOffset + (y * squareSize), squareSize, squareSize, paint);
                }
            }
        }

        private void DrawPieces(SKCanvas canvas, Board board, int yOffset, string theme)
        {
            var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
            using var paint = new SKPaint(); 

            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    var pos = new Position(x + 1, y + 1);
                    pos = pos.Mirror();

                    var piece = board.GetPiece(pos);
                    if (piece == null) continue;

                    var bitmap = GetPieceBitmap(theme, piece);

                    float maxScale = 0.85f;
                    float maxAllowedSize = squareSize * maxScale;

                    // Calculate scale
                    float bitmapWidth = bitmap.Width;
                    float bitmapHeight = bitmap.Height;
                    float scale = Math.Min(maxAllowedSize / bitmapWidth, maxAllowedSize / bitmapHeight);

                    // Calculate proportions
                    float finalWidth = bitmapWidth * scale;
                    float finalHeight = bitmapHeight * scale;

                    // Center piece
                    float offsetX = (squareSize - finalWidth) / 2f;
                    float offsetY = yOffset + ((squareSize - finalHeight) / 2f);

                    // Create final destination rectangle
                    var destRect = new SKRect(
                        x * squareSize + offsetX,
                        y * squareSize + offsetY,
                        x * squareSize + offsetX + finalWidth,
                        y * squareSize + offsetY + finalHeight
                    );

                    // Draw piece with high quality
                    using (var image = SKImage.FromBitmap(bitmap))
                    {
                        canvas.DrawImage(image, destRect, sampling, paint);
                    }
                }
            }
        }

        private SKBitmap GetPieceBitmap(string theme, Piece piece)
        {
            string key =
                $"Chess.NET.Shared.resources.icons.themes.{theme}." +
                $"{piece.Color.ToString().ToLower()}." +
                $"{piece.Type}.png";

            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var bmp = LoadBitmap(key);
            _cache[key] = bmp;
            return bmp;
        }

        private SKBitmap LoadBitmap(string resourceName)
        {
            var asm = typeof(Renderer).Assembly;
            using var stream = asm.GetManifestResourceStream(resourceName) ?? throw new Exception($"Missing resource: {resourceName}");
            return SKBitmap.Decode(stream);
        }

        private static string TruncateText(string text, SKFont font, SKPaint paint, float maxWidth)
        {
            if (font.MeasureText(text, paint) <= maxWidth) return text;

            string ellipsis = "...";
            float ellipsisWidth = font.MeasureText(ellipsis, paint);

            for (int i = text.Length - 1; i > 0; i--)
            {
                string candidate = text[..i];
                if (font.MeasureText(candidate, paint) + ellipsisWidth <= maxWidth)
                    return candidate + ellipsis;
            }

            return ellipsis;
        }
    }
}
