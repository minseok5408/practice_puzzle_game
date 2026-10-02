using System;
using System.Collections.Generic;
using PuzzleGame.Core.Board;

namespace PuzzleGame.Core.Levels
{
    public enum LevelOutcome { Playing, Won, Lost }

    public sealed class LevelProgress
    {
        private readonly HashSet<int> removedThisTurn = new HashSet<int>();
        private int scoreBeforeTurn;
        private int movesBeforeTurn;
        private int frostBeforeTurn;
        private readonly HashSet<GridPosition> destroyedFrost = new HashSet<GridPosition>();
        public int FrostCleared { get; private set; }
        private readonly int[] collected = new int[7];
        private readonly int[] collectedBeforeTurn = new int[7];
        public int Collected(PieceColor color) => color >= PieceColor.Red && color <= PieceColor.Purple ? collected[(int)color] : 0;
        public LevelRules Rules { get; }
        public int Score { get; private set; }
        public int MovesRemaining { get; private set; }
        public int BonusMoves { get; private set; }
        public int MovesUsed=>Rules.StartingMoves+BonusMoves-MovesRemaining;
        public bool IsResolving { get; private set; }
        public bool IsBlocked { get; private set; }
        public LevelOutcome Outcome { get; private set; } = LevelOutcome.Playing;
        public bool IsFinished => Outcome != LevelOutcome.Playing;
        public bool GoalsMet
        {
            get
            {
                if (Score < Rules.TargetScore || FrostCleared < Rules.FrostTarget) return false;
                for (int i = 1; i <= 6; i++) if (collected[i] < Rules.CollectionTarget((PieceColor)i)) return false;
                return true;
            }
        }
        public bool WillFinish => GoalsMet || MovesRemaining == 0;

        public LevelProgress(LevelRules rules)
        {
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            MovesRemaining = rules.StartingMoves;
        }

        public bool TryBeginMove()=>TryBeginResolution(true);
        public bool TryBeginItem()=>TryBeginResolution(false);
        private bool TryBeginResolution(bool consumeMove)
        {
            if (IsFinished || IsResolving || MovesRemaining == 0) return false;
            scoreBeforeTurn = Score;
            movesBeforeTurn = MovesRemaining;
            frostBeforeTurn = FrostCleared;
            destroyedFrost.Clear();
            Array.Copy(collected, collectedBeforeTurn, collected.Length);
            removedThisTurn.Clear();
            if(consumeMove)MovesRemaining--;
            IsResolving = true;
            return true;
        }

        public bool AddBonusMoves(int amount)
        {
            if(IsFinished || IsResolving || amount<=0 || MovesRemaining>int.MaxValue-amount || BonusMoves>int.MaxValue-amount)return false;
            MovesRemaining+=amount;BonusMoves+=amount;return true;
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

        public bool CompleteMove(bool noMoves = false)
        {
            if (!IsResolving) return false;
            IsResolving = false;
            // Success takes precedence on the last move. Evaluate only after the cascade.
            if (GoalsMet) Outcome = LevelOutcome.Won;
            else if (MovesRemaining == 0 || noMoves) { Outcome = LevelOutcome.Lost; IsBlocked = noMoves && MovesRemaining > 0; }
            return true;
        }

        public bool CompleteImmediately()
        {
            if(IsFinished || IsResolving)return false;
            Score=Math.Max(Score,Rules.TargetScore);
            FrostCleared=Math.Max(FrostCleared,Rules.FrostTarget);
            for(int i=1;i<=6;i++)collected[i]=Math.Max(collected[i],Rules.CollectionTarget((PieceColor)i));
            Outcome=LevelOutcome.Won;
            return true;
        }

        public bool CancelMove()
        {
            if (!IsResolving) return false;
            Score = scoreBeforeTurn;
            MovesRemaining = movesBeforeTurn;
            FrostCleared = frostBeforeTurn;
            destroyedFrost.Clear();
            Array.Copy(collectedBeforeTurn, collected, collected.Length);
            IsResolving = false;
            removedThisTurn.Clear();
            return true;
        }

        public void RecordRemoval(ResolutionStep step)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));
            var newPieces = new List<RemovedPiece>();
            var ids = new HashSet<int>(step.RemovedIds);
            var unique = new HashSet<int>();
            foreach (var piece in step.RemovedPieces)
                if (ids.Contains(piece.Id) && !removedThisTurn.Contains(piece.Id) && unique.Add(piece.Id)) newPieces.Add(piece);
            RecordRemovedPieces(step.RemovedIds);
            foreach (var damage in step.FrostDamage)
                if (damage.RemainingHealth == 0 && destroyedFrost.Add(damage.Position)) FrostCleared++;
            foreach (var piece in newPieces)
                if (piece.Color >= PieceColor.Red && piece.Color <= PieceColor.Purple) collected[(int)piece.Color]++;
        }
    }
}
