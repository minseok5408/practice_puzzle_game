using System;
using System.IO;
using UnityEngine;
using PuzzleGame.Core.Levels;

namespace PuzzleGame.Runtime.Levels
{
    public enum ProgressLoadStatus { New, Loaded, Recovered, Damaged }
    [Serializable]
    public sealed class CampaignProgress
    {
        public int version = 4;
        public int selectedLevel = 1;
        public bool[] completed = new bool[LevelCatalog.LevelCount];
        public int[] bestScores = new int[LevelCatalog.LevelCount];
        public int[] items = { ItemRules.StartingStock, ItemRules.StartingStock, ItemRules.StartingStock, ItemRules.StartingStock };
        public bool[] itemRewardsClaimed = new bool[LevelCatalog.LevelCount];
        public int[] stars = new int[LevelCatalog.LevelCount];
        public bool[] cleanMedals = new bool[LevelCatalog.LevelCount];
        public bool[] efficientMedals = new bool[LevelCatalog.LevelCount];
        public int TotalStars { get { int total = 0; foreach (int value in stars) total += value; return total; } }
        public int StarsAt(int number) => number >= 1 && number <= stars.Length ? stars[number - 1] : 0;
        public void RecordRating(int number, LevelProgress progress, int itemsUsed)
        {
            if (!IsComplete(number) || progress.Outcome != LevelOutcome.Won) return;
            stars[number - 1] = Math.Max(stars[number - 1], StageRating.Stars(progress, itemsUsed));
            cleanMedals[number - 1] |= itemsUsed == 0;
            efficientMedals[number - 1] |= StageRating.IsEfficient(progress);
        }
        public int ItemCount(ItemType type) => ItemRules.IsValid(type) ? items[(int)type] : 0;
        public bool TrySpendItem(ItemType type)
        {
            if (!ItemRules.IsValid(type) || ItemCount(type) <= 0) return false;
            items[(int)type]--; return true;
        }
        public void RefundItem(ItemType type)
        {
            if (ItemRules.IsValid(type)) items[(int)type] = Math.Min(ItemRules.MaximumStock, items[(int)type] + 1);
        }
        public int[] ClaimItemReward(int number)
        {
            var granted = new int[ItemRules.Count];
            if (!IsComplete(number) || itemRewardsClaimed[number - 1]) return granted;
            itemRewardsClaimed[number - 1] = true;
            for (int i = 0; i < ItemRules.Count; i++)
                if ((number % 5 == 0 || i == (number - 1) % ItemRules.Count) && items[i] < ItemRules.MaximumStock)
                { items[i]++; granted[i] = 1; }
            return granted;
        }
        public int UnlockedThrough
        {
            get
            {
                int unlocked = 1;
                while (unlocked < LevelCatalog.LevelCount && completed[unlocked - 1]) unlocked++;
                return unlocked;
            }
        }
        public bool IsUnlocked(int number) => number >= 1 && number <= UnlockedThrough;
        public bool IsComplete(int number) => number >= 1 && number <= completed.Length && completed[number - 1];
        public int CompletedCount { get { int count = 0; foreach (bool value in completed) if (value) count++; return count; } }
        public bool RecordWin(int number, int score)
        {
            if (!IsUnlocked(number) || score < 0) return false;
            completed[number - 1] = true;
            bestScores[number - 1] = Math.Max(bestScores[number - 1], score);
            return true;
        }

        private bool IsValid()
        {
            if (version != 4 || completed == null || bestScores == null || completed.Length != LevelCatalog.LevelCount || bestScores.Length != LevelCatalog.LevelCount
                || items == null || items.Length != ItemRules.Count || itemRewardsClaimed == null || itemRewardsClaimed.Length != LevelCatalog.LevelCount)
                return false;
            if (stars == null || stars.Length != LevelCatalog.LevelCount || cleanMedals == null || cleanMedals.Length != LevelCatalog.LevelCount
                || efficientMedals == null || efficientMedals.Length != LevelCatalog.LevelCount) return false;
            foreach (int count in items) if (count < 0 || count > ItemRules.MaximumStock) return false;
            bool gap = false;
            for (int i = 0; i < completed.Length; i++)
            {
                if (bestScores[i] < 0 || (gap && completed[i]) || (itemRewardsClaimed[i] && !completed[i])) return false;
                if (stars[i] < 0 || stars[i] > 3 || (!completed[i] && (stars[i] > 0 || cleanMedals[i] || efficientMedals[i]))) return false;
                if (!completed[i]) gap = true;
            }
            return IsUnlocked(selectedLevel);
        }

        public static CampaignProgress Load(string path) => Load(path, out _);

        public static CampaignProgress Load(string path, out ProgressLoadStatus status)
        {
            status = ProgressLoadStatus.New;
            if (!string.IsNullOrEmpty(path)) foreach (string candidate in new[] { path, path + ".bak" })
            {
                try
                {
                    if (!File.Exists(candidate)) continue;
                    status = ProgressLoadStatus.Damaged;
                    var result = JsonUtility.FromJson<CampaignProgress>(File.ReadAllText(candidate));
                    if (result != null && result.version == 1) result = MigrateOriginalCampaign(result);
                    if (result != null && result.version == 2)
                    {
                        result.version = 3;
                        result.items = new[] { ItemRules.StartingStock, ItemRules.StartingStock, ItemRules.StartingStock, ItemRules.StartingStock };
                        result.itemRewardsClaimed = new bool[LevelCatalog.LevelCount];
                    }
                    if (result != null && result.version == 3)
                    {
                        result.version = 4;
                        result.stars = new int[LevelCatalog.LevelCount];
                        result.cleanMedals = new bool[LevelCatalog.LevelCount];
                        result.efficientMedals = new bool[LevelCatalog.LevelCount];
                        if (result.completed != null && result.completed.Length == LevelCatalog.LevelCount)
                            for (int i = 0; i < result.stars.Length; i++) if (result.completed[i]) result.stars[i] = 1;
                    }
                    if (result != null && result.IsValid())
                    { status = candidate == path ? ProgressLoadStatus.Loaded : ProgressLoadStatus.Recovered; return result; }
                }
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException) { }
            }
            return new CampaignProgress();
        }

        // v1 had 20 stages per world. Retain each world's first ten, then derive its new frontier.
        private static CampaignProgress MigrateOriginalCampaign(CampaignProgress old)
        {
            if (old.completed == null || old.bestScores == null || old.completed.Length != 100 || old.bestScores.Length != 100) return null;
            int cleared = 0; bool gap = false;
            for (int i = 0; i < 100; i++)
            {
                if (old.bestScores[i] < 0 || (gap && old.completed[i])) return null;
                if (old.completed[i]) cleared++; else gap = true;
            }
            if (old.selectedLevel < 1 || old.selectedLevel > Math.Min(100, cleared + 1)) return null;
            var migrated = new CampaignProgress();
            for (int world = 0; world < 5; world++) for (int stage = 0; stage < 10; stage++)
            {
                int source = world * 20 + stage, destination = world * 10 + stage;
                migrated.completed[destination] = old.completed[source];
                migrated.bestScores[destination] = old.bestScores[source];
                migrated.stars[destination] = old.completed[source] ? 1 : 0;
            }
            int selectedStage = (old.selectedLevel - 1) % 20;
            migrated.selectedLevel = selectedStage < 10 ? (old.selectedLevel - 1) / 20 * 10 + selectedStage + 1 : migrated.UnlockedThrough;
            return migrated.IsValid() ? migrated : null;
        }

        public bool Save(string path)
        {
            if (string.IsNullOrEmpty(path)) return true; // Editor/test play uses an in-memory campaign.
            if (!IsValid()) return false;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
                File.WriteAllText(path + ".tmp", JsonUtility.ToJson(this, true));
                if (File.Exists(path)) File.Replace(path + ".tmp", path, path + ".bak");
                else File.Move(path + ".tmp", path);
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            { Debug.LogWarning("Could not save campaign: " + e.Message); return false; }
        }

        public static string SavePath
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                string[] args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-puzzleProgressPath");
                if (index >= 0 && index + 1 < args.Length) return args[index + 1];
#endif
                return Application.isEditor ? null : Path.Combine(Application.persistentDataPath, "progress.json");
            }
        }
    }
}
