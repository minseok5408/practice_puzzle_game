using System;

namespace PuzzleGame.Core.Levels
{
    public sealed class LevelRules
    {
        public int StartingMoves { get; }
        public int TargetScore { get; }
        public int PointsPerPiece { get; }

        public LevelRules(int startingMoves, int targetScore, int pointsPerPiece)
        {
            if (startingMoves < 1) throw new ArgumentOutOfRangeException(nameof(startingMoves));
            if (targetScore < 1) throw new ArgumentOutOfRangeException(nameof(targetScore));
            if (pointsPerPiece < 1) throw new ArgumentOutOfRangeException(nameof(pointsPerPiece));
            StartingMoves = startingMoves;
            TargetScore = targetScore;
            PointsPerPiece = pointsPerPiece;
        }
    }
}
