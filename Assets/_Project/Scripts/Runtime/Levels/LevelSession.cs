using System;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Services;
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
        public int[] LastItemRewards { get; private set; } = new int[ItemRules.Count];
        public int ItemsUsed { get; private set; }
        public int PreviousBest { get; private set; }
        public int EarnedStars { get; private set; }
        public bool NewBest { get; private set; }
        public bool WasAssistedClear { get; private set; }
        public void RecordItemUse() => ItemsUsed++;
        public int ItemCount(ItemType type) => Campaign?.ItemCount(type) ?? 0;
        public bool TrySpendItem(ItemType type)
        {
            bool spent = CanPlay && campaignState && campaignState.TrySpendItem(type);
            Changed?.Invoke(); return spent;
        }
        public void RefundItem(ItemType type) { if (campaignState) campaignState.RefundItem(type); Changed?.Invoke(); }
        public bool BeginItem()
        {
            if (!CanPlay || !Progress.TryBeginItem()) return false;
            Changed?.Invoke(); return true;
        }
        public bool AddItemMoves()
        {
            if (!CanPlay || !Progress.AddBonusMoves(ItemRules.BonusMoves)) return false;
            Changed?.Invoke(); return true;
        }

        public void Configure(BoardController controller, LevelDefinition level)
        { board = controller; definition = level; }

        public void ConfigureCampaign(LevelCatalog levels) => catalog = levels;

        public void StartInitialLevel()
        {
            if (catalog)
            {
                campaignState = CampaignState.Ensure(catalog);
                definition = catalog.Get(Campaign.selectedLevel);
                board.ConfigureLevel(definition);
            }
            board.GenerateBoard();
        }

        public bool SelectLevel(int number)
        {
            if (!catalog || Campaign == null || !Campaign.IsUnlocked(number) || board.IsBusy) return false;
            rulesOverride = null; definition = catalog.Get(number);
            campaignState.Select(number);
            board.ConfigureLevel(definition); board.GenerateBoard();
            return true;
        }

        public bool NextLevel() => definition && definition.Number < LevelCatalog.LevelCount && SelectLevel(definition.Number + 1);

        public void ResetProgress()
        {
            if (IsCampaignRun) PlaytestJournal.Finish(Progress, "restart");
            if (!definition && rulesOverride == null) throw new InvalidOperationException("A level definition is required.");
            Progress = new LevelProgress(rulesOverride ?? definition.CreateRules());
            LastItemRewards = new int[ItemRules.Count];
            ItemsUsed = EarnedStars = 0; NewBest = WasAssistedClear = false;
            PreviousBest = IsCampaignRun && Campaign != null ? Campaign.bestScores[definition.Number - 1] : 0;
            if (IsCampaignRun) PlaytestJournal.Begin(definition);
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

        public void CompleteMove(bool noMoves = false)
        {
            if (!Progress.CompleteMove(noMoves)) return;
            PublishProgress();
        }

        internal bool CompleteForEasterEgg()
        {
            if(Progress==null || !Progress.CompleteImmediately())return false;
            PublishProgress("easter_egg");
            return true;
        }

        private void PublishProgress(string outcomeOverride=null)
        {
            WasAssistedClear = outcomeOverride == "easter_egg";
            if (Progress.Outcome == LevelOutcome.Won && !WasAssistedClear)
            {
                EarnedStars = StageRating.Stars(Progress, ItemsUsed);
                NewBest = Progress.Score > PreviousBest;
            }
            if (Progress.IsFinished)
            {
                bool won = Progress.Outcome == LevelOutcome.Won;
                GameAudio.Play(won ? (IsCampaignRun && definition.Number == LevelCatalog.LevelCount ? "complete" : "win") : "lose");
                if (IsCampaignRun) PlaytestJournal.Finish(Progress, outcomeOverride ?? (won ? "won" : "lost"));
            }
            if (IsCampaignRun && Progress.Outcome == LevelOutcome.Won && Campaign != null)
            {
                int previouslyUnlocked=Campaign.UnlockedThrough;
                Campaign.RecordWin(definition.Number, Progress.Score);
                if(Campaign.UnlockedThrough>previouslyUnlocked)campaignState.PendingUnlock=Campaign.UnlockedThrough;
                if (outcomeOverride != "easter_egg")
                {
                    LastItemRewards = Campaign.ClaimItemReward(definition.Number);
                    Campaign.RecordRating(definition.Number, Progress, ItemsUsed);
                }
                campaignState.Save();
            }
            Changed?.Invoke();
        }

        public void CancelMove()
        {
            if (Progress != null && Progress.CancelMove()) Changed?.Invoke();
        }

        public void RestartLevel() => board.GenerateBoard();

        private void Update()
        {
            if (IsCampaignRun && Progress != null && !Progress.IsFinished && !board.IsPaused && Application.isFocused)
                PlaytestJournal.Tick(Time.deltaTime);
        }
        private void OnDestroy() { if (IsCampaignRun) PlaytestJournal.Finish(Progress, "exit"); }

        // Explicit runtime rules are useful for tutorials and deterministic result tests.
        public void StartWithRules(LevelRules rules)
        {
            rulesOverride = rules ?? throw new ArgumentNullException(nameof(rules));
            RestartLevel();
        }
    }
}
