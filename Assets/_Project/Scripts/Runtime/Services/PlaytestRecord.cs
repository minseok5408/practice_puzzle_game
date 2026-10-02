using System;

namespace PuzzleGame.Runtime.Services
{
    [Serializable]
    public sealed class PlaytestRecord
    {
        public int schemaVersion=2;
        public string attemptId, source, recordedUtc, levelId, rulesVersion="frozen-2", outcome, reason;
        public int seed, score, movesRemaining, frostCleared, moves, bonusMoves;
        public float activeSeconds;
        public string[] inputs;
    }
}
