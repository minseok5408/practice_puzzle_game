using System;

namespace PuzzleGame.Core.Levels
{
    public static class StageRating
    {
        public static int ReserveTarget(int startingMoves) => Math.Max(3, (int)Math.Ceiling(startingMoves * .2));
        public static bool IsEfficient(LevelProgress progress) => progress.Outcome == LevelOutcome.Won
            && progress.Rules.StartingMoves - progress.MovesUsed >= ReserveTarget(progress.Rules.StartingMoves);
        public static int Stars(LevelProgress progress, int itemsUsed) => progress.Outcome != LevelOutcome.Won ? 0
            : 1 + (itemsUsed == 0 ? 1 : 0) + (IsEfficient(progress) ? 1 : 0);
    }
}
