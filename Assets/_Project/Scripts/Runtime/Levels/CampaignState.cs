using UnityEngine;

namespace PuzzleGame.Runtime.Levels
{
    // A single campaign survives Map <-> Game. Scene UI and boards are never persisted here.
    public sealed class CampaignState : MonoBehaviour
    {
        public static CampaignState Instance { get; private set; }
        public CampaignProgress Progress { get; private set; }
        public LevelCatalog Catalog { get; private set; }
        public bool SaveFailed { get; private set; }
        private string savePath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetInstance() => Instance = null;

        public static CampaignState Ensure(LevelCatalog catalog)
        {
            if (Instance) return Instance;
            var state = new GameObject("CampaignState").AddComponent<CampaignState>();
            Instance = state; state.Catalog = catalog; state.savePath = CampaignProgress.SavePath;
            state.Progress = CampaignProgress.Load(state.savePath);
            DontDestroyOnLoad(state.gameObject);
            return state;
        }

        public bool Select(int number)
        {
            if (!Progress.IsUnlocked(number)) return false;
            Progress.selectedLevel = number; Save(); return true;
        }
        public void Save() => SaveFailed = !Progress.Save(savePath);
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
