using System;
using System.Collections.Generic;
using PuzzleGame.Core.Board;

namespace PuzzleGame.Core.Levels
{
    public sealed class LevelRules
    {
        public int StartingMoves { get; }
        public int TargetScore { get; }
        public int PointsPerPiece { get; }
        private readonly int[] collectionTargets = new int[6];
        public int CollectionTarget(PieceColor color) => color >= PieceColor.Red && color <= PieceColor.Purple
            ? collectionTargets[(int)color - 1] : 0;

        public LevelRules(int startingMoves, int targetScore, int pointsPerPiece, IReadOnlyList<int> targets = null)
        {
            if (startingMoves < 1) throw new ArgumentOutOfRangeException(nameof(startingMoves));
            if (targetScore < 1) throw new ArgumentOutOfRangeException(nameof(targetScore));
            if (pointsPerPiece < 1) throw new ArgumentOutOfRangeException(nameof(pointsPerPiece));
            StartingMoves = startingMoves;
            TargetScore = targetScore;
            PointsPerPiece = pointsPerPiece;
            if (targets != null)
            {
                if (targets.Count != 6) throw new ArgumentException("Collection targets must contain six colors.", nameof(targets));
                for (int i = 0; i < 6; i++)
                {
                    if (targets[i] < 0) throw new ArgumentOutOfRangeException(nameof(targets));
                    collectionTargets[i] = targets[i];
                }
            }
        }
    }
}
