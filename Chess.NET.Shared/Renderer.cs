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

        public Renderer(int squareSize = 64, Position? hightlightSquare1 =null, Position? hightlightSquare2 = null)
        {
            this.squareSize = squareSize;
            this.highlightSquare1 = hightlightSquare1;
            this.hightlightSquare2 = hightlightSquare2;
        }

        public byte[] Render(Board board, string theme)
        {
            int size = boardSize * squareSize;

            using var bitmap = new SKBitmap(size, size);
            using var canvas = new SKCanvas(bitmap);

            canvas.Clear(SKColors.Black);

            DrawBoard(canvas);
            DrawPieces(canvas, board, theme);

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            return data.ToArray();
        }

        private void DrawBoard(SKCanvas canvas)
        {
            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    var paint = (x + y) % 2 == 0 ? lightSquare : darkSquare;

                    var pos = new Position(x + 1, y + 1);
                    pos = pos.Mirror();

                    if (pos == highlightSquare1 || pos == hightlightSquare2)
                        canvas.DrawRect(x * squareSize, y * squareSize, squareSize, squareSize, highlightSquare);
                    else 
                        canvas.DrawRect(x * squareSize, y * squareSize, squareSize, squareSize, paint);
                }
            }
        }

        private void DrawPieces(SKCanvas canvas, Board board, string theme)
        {
            var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
            using var paint = new SKPaint(); 

            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    var pos = new Position(x + 1, y + 1);
                    pos = pos.Mirror();

                    Debug.WriteLine(pos.ToString());

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
                    float offsetY = (squareSize - finalHeight) / 2f;

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
    }
}
