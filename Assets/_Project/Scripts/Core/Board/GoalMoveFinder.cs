using System;
using PuzzleGame.Core.Levels;

namespace PuzzleGame.Core.Board
{
    // Rank only the first resolution, never future random refills. The live board and RNG are untouched.
    public static class GoalMoveFinder
    {
        public static bool TryFindMove(BoardState board, LevelProgress progress, out GridPosition first, out GridPosition second)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            first = second = default;
            int best = int.MinValue;
            for (int y = 0; y < board.Height; y++) for (int x = 0; x < board.Width; x++)
            {
                var a = new GridPosition(x, y);
                for (int direction = 0; direction < 2; direction++)
                {
                    var b = new GridPosition(x + (direction == 0 ? 1 : 0), y + (direction == 1 ? 1 : 0));
                    if (!MoveFinder.IsValidSwap(board, a, b)) continue;
                    var copy = new BoardState(board.Width, board.Height);
                    for (int cy = 0; cy < board.Height; cy++) for (int cx = 0; cx < board.Width; cx++)
                    {
                        var p = new GridPosition(cx, cy);
                        copy.SetPiece(p, board.GetPiece(p)); copy.SetCell(p, board.GetCell(p));
                    }
                    copy.SwapPieces(a, b);
                    var step = new BoardResolver(copy, 6, new Random(0)).ResolveMatches(copy, a, b);
                    int score = Evaluate(step, progress);
                    if (score <= best) continue;
                    best = score; first = a; second = b;
                }
            }
            return best != int.MinValue;
        }

        public static int Evaluate(ResolutionStep step, LevelProgress progress)
        {
            int score = step.RemovedIds.Count + step.SpecialCreations.Count * 18;
            if (progress == null) return score + step.FrostDamage.Count * 8;
            var colors = new int[7];
            foreach (var piece in step.RemovedPieces) if (piece.Color >= PieceColor.Red && piece.Color <= PieceColor.Purple) colors[(int)piece.Color]++;
            for (int i = 1; i <= 6; i++) score += Math.Min(colors[i], Math.Max(0, progress.Rules.CollectionTarget((PieceColor)i) - progress.Collected((PieceColor)i))) * 12;
            if (progress.FrostCleared < progress.Rules.FrostTarget)
                foreach (var hit in step.FrostDamage) score += hit.RemainingHealth == 0 ? 24 : 10;
            score += Math.Min(step.RemovedIds.Count * progress.Rules.PointsPerPiece, Math.Max(0, progress.Rules.TargetScore - progress.Score)) / 10;
            return score;
        }
    }
}
