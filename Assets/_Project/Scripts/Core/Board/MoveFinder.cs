using System;

namespace PuzzleGame.Core.Board
{
    public static class MoveFinder
    {
        public static bool HasAnyMove(BoardState board) => TryFindMove(board, out _, out _);

        public static bool TryFindMove(BoardState board, out GridPosition first, out GridPosition second)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                {
                    first = new GridPosition(x, y);
                    second = new GridPosition(x + 1, y);
                    if (IsValidSwap(board, first, second)) return true;
                    second = new GridPosition(x, y + 1);
                    if (IsValidSwap(board, first, second)) return true;
                }
            first = default;
            second = default;
            return false;
        }

        public static bool IsValidSwap(BoardState board, GridPosition first, GridPosition second)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (!board.Contains(first) || !board.Contains(second) || !first.IsAdjacentTo(second)) return false;
            PieceState a = board.GetPiece(first);
            PieceState b = board.GetPiece(second);
            if (a == null || b == null) return false;
            if (SpecialPieceRules.IsSpecialSwap(a, b)) return true;
            if (a.Color == b.Color) return false;
            board.SwapPieces(first, second);
            try
            {
                return MatchFinder.HasMatchAt(board, first) || MatchFinder.HasMatchAt(board, second);
            }
            finally
            {
                board.SwapPieces(first, second);
            }
        }
    }
}
