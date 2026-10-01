using System;
using System.Collections.Generic;

namespace PuzzleGame.Core.Levels
{
    public enum LevelOutcome { Playing, Won, Lost }

    public sealed class LevelProgress
    {
        private readonly HashSet<int> removedThisTurn = new HashSet<int>();
        private int scoreBeforeTurn;
        private int movesBeforeTurn;
        public LevelRules Rules { get; }
        public int Score { get; private set; }
        public int MovesRemaining { get; private set; }
        public bool IsResolving { get; private set; }
        public LevelOutcome Outcome { get; private set; } = LevelOutcome.Playing;
        public bool IsFinished => Outcome != LevelOutcome.Playing;
        public bool WillFinish => Score >= Rules.TargetScore || MovesRemaining == 0;

        public LevelProgress(LevelRules rules)
        {
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            MovesRemaining = rules.StartingMoves;
        }

        public bool TryBeginMove()
        {
            if (IsFinished || IsResolving || MovesRemaining == 0) return false;
            scoreBeforeTurn = Score;
            movesBeforeTurn = MovesRemaining;
            removedThisTurn.Clear();
            MovesRemaining--;
            IsResolving = true;
            return true;
        }

        public int RecordRemovedPieces(IEnumerable<int> ids)
        {
            if (!IsResolving) throw new InvalidOperationException("No move is being resolved.");
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            var newIds = new HashSet<int>();
            foreach (int id in ids)
            {
                if (id <= 0) throw new ArgumentOutOfRangeException(nameof(ids));
                if (!removedThisTurn.Contains(id)) newIds.Add(id);
            }
            int gained = checked(newIds.Count * Rules.PointsPerPiece);
            Score = checked(Score + gained);
            removedThisTurn.UnionWith(newIds);
            return gained;
        }

        public bool CompleteMove()
        {
            if (!IsResolving) return false;
            IsResolving = false;
            // Success takes precedence on the last move. Evaluate only after the cascade.
            if (Score >= Rules.TargetScore) Outcome = LevelOutcome.Won;
            else if (MovesRemaining == 0) Outcome = LevelOutcome.Lost;
            return true;
        }

        public bool CancelMove()
        {
            if (!IsResolving) return false;
            Score = scoreBeforeTurn;
            MovesRemaining = movesBeforeTurn;
            IsResolving = false;
            removedThisTurn.Clear();
            return true;
        }
    }
}
