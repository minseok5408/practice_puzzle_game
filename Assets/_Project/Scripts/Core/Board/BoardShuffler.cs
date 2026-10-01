using System;
using System.Collections.Generic;

namespace PuzzleGame.Core.Board
{
    public static class BoardShuffler
    {
        public static bool TryShuffle(BoardState board, Random random, int maxAttempts = 256)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
            var original = new List<PieceState>();
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                    original.Add(board.GetPiece(new GridPosition(x, y)));
            var shuffled = original.ToArray();
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                for (int i = shuffled.Length - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    PieceState temp = shuffled[i]; shuffled[i] = shuffled[j]; shuffled[j] = temp;
                }
                Apply(board, shuffled);
                if (MatchFinder.FindMatches(board).Count == 0 && MoveFinder.HasAnyMove(board)) return true;
            }
            Apply(board, original);
            return false;
        }

        private static void Apply(BoardState board, IList<PieceState> pieces)
        {
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                    board.SetPiece(new GridPosition(x, y), pieces[y * board.Width + x]);
        }
    }
}
