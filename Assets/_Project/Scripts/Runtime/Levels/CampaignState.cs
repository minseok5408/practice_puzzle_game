using UnityEngine;
using PuzzleGame.Core.Levels;

namespace PuzzleGame.Runtime.Levels
{
    // A single campaign survives Map <-> Game. Scene UI and boards are never persisted here.
    public sealed class CampaignState : MonoBehaviour
    {
        public static CampaignState Instance { get; private set; }
        public CampaignProgress Progress { get; private set; }
        public LevelCatalog Catalog { get; private set; }
        public bool SaveFailed { get; private set; }
        public int PendingUnlock { get; set; }
        public ProgressLoadStatus LoadStatus { get; private set; }
        private string savePath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetInstance() => Instance = null;

        public static CampaignState Ensure(LevelCatalog catalog)
        {
            if (Instance) return Instance;
            var state = new GameObject("CampaignState").AddComponent<CampaignState>();
            Instance = state; state.Catalog = catalog; state.savePath = CampaignProgress.SavePath;
            state.Progress = CampaignProgress.Load(state.savePath, out var status);
            state.LoadStatus = status;
            DontDestroyOnLoad(state.gameObject);
            return state;
        }

        public bool Select(int number)
        {
            if (!Progress.IsUnlocked(number)) return false;
            Progress.selectedLevel = number; Save(); return true;
        }
        public void Save() => SaveFailed = !Progress.Save(savePath);
        public bool TrySpendItem(ItemType type)
        {
            if (!Progress.TrySpendItem(type)) return false;
            Save();
            if (!SaveFailed) return true;
            Progress.RefundItem(type); return false;
        }
        public void RefundItem(ItemType type) { Progress.RefundItem(type); Save(); }
        public void AcknowledgeRecovery() => LoadStatus = ProgressLoadStatus.Loaded;
        public bool ResetProgress()
        {
            var fresh = new CampaignProgress();
            if (!fresh.Save(savePath)) { SaveFailed = true; return false; }
            Progress = fresh;
            PendingUnlock=0;
            // A second atomic write replaces the backup with the empty campaign too.
            SaveFailed = !fresh.Save(savePath);
            LoadStatus = ProgressLoadStatus.Loaded;
            return !SaveFailed;
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
