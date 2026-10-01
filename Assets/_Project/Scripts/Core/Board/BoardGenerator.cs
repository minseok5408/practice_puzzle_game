using System;
using System.Collections.Generic;

namespace PuzzleGame.Core.Board
{
    public static class BoardGenerator
    {
        public static BoardState Generate(int width, int height, int colorCount, int seed, int maxAttempts = 128)
        {
            if (width < 3 || width > 64) throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 3 || height > 64) throw new ArgumentOutOfRangeException(nameof(height));
            if (colorCount < 3 || colorCount > 6) throw new ArgumentOutOfRangeException(nameof(colorCount));
            if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));

            var random = new Random(seed);
            var candidates = new List<PieceColor>(colorCount);
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var board = new BoardState(width, height);
                int id = 1;
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        candidates.Clear();
                        for (int colorIndex = 1; colorIndex <= colorCount; colorIndex++)
                        {
                            var color = (PieceColor)colorIndex;
                            bool horizontal = x >= 2 && board.GetPiece(new GridPosition(x - 1, y)).Color == color
                                && board.GetPiece(new GridPosition(x - 2, y)).Color == color;
                            bool vertical = y >= 2 && board.GetPiece(new GridPosition(x, y - 1)).Color == color
                                && board.GetPiece(new GridPosition(x, y - 2)).Color == color;
                            if (!horizontal && !vertical) candidates.Add(color);
                        }
                        board.SetPiece(new GridPosition(x, y),
                            new PieceState(id++, candidates[random.Next(candidates.Count)]));
                    }
                if (MoveFinder.HasAnyMove(board)) return board;
            }
            throw new InvalidOperationException($"Could not generate a playable board in {maxAttempts} attempts (seed {seed}).");
        }
    }
}
