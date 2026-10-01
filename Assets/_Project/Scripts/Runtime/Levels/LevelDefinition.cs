using PuzzleGame.Core.Levels;
using UnityEngine;

namespace PuzzleGame.Runtime.Levels
{
    [CreateAssetMenu(menuName = "Puzzle Game/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string levelId = "level_001";
        [SerializeField] private string displayName = "연습 스테이지";
        [SerializeField, Min(1)] private int startingMoves = 20;
        [SerializeField, Min(1)] private int targetScore = 1000;
        [SerializeField, Min(1)] private int pointsPerPiece = 10;
        public string LevelId => levelId;
        public string DisplayName => displayName;
        public LevelRules CreateRules() => new LevelRules(startingMoves, targetScore, pointsPerPiece);
    }
}
