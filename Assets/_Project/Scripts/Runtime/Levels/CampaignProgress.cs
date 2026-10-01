using System;
using System.IO;
using UnityEngine;

namespace PuzzleGame.Runtime.Levels
{
    [Serializable]
    public sealed class CampaignProgress
    {
        public int version = 2;
        public int selectedLevel = 1;
        public bool[] completed = new bool[LevelCatalog.LevelCount];
        public int[] bestScores = new int[LevelCatalog.LevelCount];
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
            if (version != 2 || completed == null || bestScores == null || completed.Length != LevelCatalog.LevelCount || bestScores.Length != LevelCatalog.LevelCount)
                return false;
            bool gap = false;
            for (int i = 0; i < completed.Length; i++)
            {
                if (bestScores[i] < 0 || (gap && completed[i])) return false;
                if (!completed[i]) gap = true;
            }
            return IsUnlocked(selectedLevel);
        }

        public static CampaignProgress Load(string path)
        {
            if (!string.IsNullOrEmpty(path)) foreach (string candidate in new[] { path, path + ".bak" })
            {
                try
                {
                    if (!File.Exists(candidate)) continue;
                    var result = JsonUtility.FromJson<CampaignProgress>(File.ReadAllText(candidate));
                    if (result != null && result.version == 1) result = MigrateOriginalCampaign(result);
                    if (result != null && result.IsValid()) return result;
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
