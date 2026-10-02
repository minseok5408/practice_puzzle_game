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
            var positions = new List<GridPosition>();
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                {
                    var position = new GridPosition(x,y);
                    if (board.IsFrozen(position)) continue;
                    positions.Add(position);original.Add(board.GetPiece(position));
                }
            if (positions.Count < 2) return false;
            var shuffled = original.ToArray();
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                for (int i = shuffled.Length - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    PieceState temp = shuffled[i]; shuffled[i] = shuffled[j]; shuffled[j] = temp;
                }
                Apply(board, positions, shuffled);
                if (MatchFinder.FindMatches(board).Count == 0 && MoveFinder.HasAnyMove(board)) return true;
            }
            Apply(board, positions, original);
            return false;
        }

        private static void Apply(BoardState board, IList<GridPosition> positions, IList<PieceState> pieces)
        {
            for (int i = 0; i < positions.Count; i++) board.SetPiece(positions[i],pieces[i]);
        }
    }
}
