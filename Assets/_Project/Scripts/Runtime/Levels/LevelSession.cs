using System;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using UnityEngine;

namespace PuzzleGame.Runtime.Levels
{
    public sealed class LevelSession : MonoBehaviour
    {
        [SerializeField] private BoardController board;
        [SerializeField] private LevelDefinition definition;
        private LevelRules rulesOverride;
        public LevelProgress Progress { get; private set; }
        public string DisplayName => definition ? definition.DisplayName : "연습 스테이지";
        public bool CanPlay => isActiveAndEnabled && Progress != null && !Progress.IsFinished && !Progress.IsResolving;
        public event Action Changed;

        public void Configure(BoardController controller, LevelDefinition level)
        { board = controller; definition = level; }

        public void ResetProgress()
        {
            if (!definition && rulesOverride == null) throw new InvalidOperationException("A level definition is required.");
            Progress = new LevelProgress(rulesOverride ?? definition.CreateRules());
            Changed?.Invoke();
        }

        public bool BeginMove()
        {
            if (!CanPlay || !Progress.TryBeginMove()) return false;
            Changed?.Invoke();
            return true;
        }

        public void ApplyRemoval(ResolutionStep step)
        {
            Progress.RecordRemovedPieces(step.RemovedIds);
            Changed?.Invoke();
        }

        public void CompleteMove()
        {
            if (Progress.CompleteMove()) Changed?.Invoke();
        }

        public void CancelMove()
        {
            if (Progress != null && Progress.CancelMove()) Changed?.Invoke();
        }

        public void RestartLevel() => board.GenerateBoard();

        // Explicit runtime rules are useful for tutorials and deterministic result tests.
        public void StartWithRules(LevelRules rules)
        {
            rulesOverride = rules ?? throw new ArgumentNullException(nameof(rules));
            RestartLevel();
        }
    }
}
