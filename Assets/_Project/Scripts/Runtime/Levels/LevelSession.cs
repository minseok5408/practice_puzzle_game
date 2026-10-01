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
        [SerializeField] private LevelCatalog catalog;
        private LevelRules rulesOverride;
        private CampaignState campaignState;
        public LevelCatalog Catalog => catalog;
        public LevelDefinition Definition => definition;
        public CampaignProgress Campaign => campaignState ? campaignState.Progress : null;
        public bool SaveFailed => campaignState && campaignState.SaveFailed;
        public bool IsCampaignRun => catalog && rulesOverride == null;
        public LevelProgress Progress { get; private set; }
        public string DisplayName => definition ? definition.DisplayName : "1-1";
        public bool CanPlay => isActiveAndEnabled && Progress != null && !Progress.IsFinished && !Progress.IsResolving;
        public event Action Changed;

        public void Configure(BoardController controller, LevelDefinition level)
        { board = controller; definition = level; }

        public void ConfigureCampaign(LevelCatalog levels) => catalog = levels;

        public void StartInitialLevel()
        {
            if (catalog)
            {
                campaignState = CampaignState.Ensure(catalog);
                definition = catalog.Get(Campaign.selectedLevel);
                board.ConfigureLevel(definition.Seed);
            }
            board.GenerateBoard();
        }

        public bool SelectLevel(int number)
        {
            if (!catalog || Campaign == null || !Campaign.IsUnlocked(number) || board.IsBusy) return false;
            rulesOverride = null; definition = catalog.Get(number);
            campaignState.Select(number);
            board.ConfigureLevel(definition.Seed); board.GenerateBoard();
            return true;
        }

        public bool NextLevel() => definition && definition.Number < LevelCatalog.LevelCount && SelectLevel(definition.Number + 1);

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
            Progress.RecordRemoval(step);
            Changed?.Invoke();
        }

        public void CompleteMove()
        {
            if (!Progress.CompleteMove()) return;
            if (IsCampaignRun && Progress.Outcome == LevelOutcome.Won && Campaign != null)
            {
                Campaign.RecordWin(definition.Number, Progress.Score);
                campaignState.Save();
            }
            Changed?.Invoke();
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
