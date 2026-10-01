using PuzzleGame.Core.Levels;
using UnityEngine;

namespace PuzzleGame.Runtime.Levels
{
    [CreateAssetMenu(menuName = "Puzzle Game/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string levelId = "level_001";
        [SerializeField] private string displayName = "1-1";
        [SerializeField, Range(1, LevelCatalog.LevelCount)] private int number = 1;
        [SerializeField] private int seed = 5408;
        [SerializeField] private int[] collectionTargets = new int[6];
        [SerializeField, Min(1)] private int startingMoves = 20;
        [SerializeField, Min(1)] private int targetScore = 1000;
        [SerializeField, Min(1)] private int pointsPerPiece = 10;
        public string LevelId => levelId;
        public string DisplayName => displayName;
        public int Number => number;
        public int World => (number - 1) / LevelCatalog.StagesPerWorld + 1;
        public int Stage => (number - 1) % LevelCatalog.StagesPerWorld + 1;
        public int Seed => seed;
        public LevelRules CreateRules() => new LevelRules(startingMoves, targetScore, pointsPerPiece, collectionTargets);

        public void ConfigureCampaign(int index, int boardSeed, int moves, int score, int[] targets)
        {
            if (index < 1 || index > LevelCatalog.LevelCount) throw new System.ArgumentOutOfRangeException(nameof(index));
            _ = new LevelRules(moves, score, 10, targets);
            number = index; seed = boardSeed; startingMoves = moves; targetScore = score; pointsPerPiece = 10;
            levelId = "level_" + index.ToString("000"); displayName = World + "-" + Stage;
            collectionTargets = (int[])targets.Clone();
        }
    }
}
