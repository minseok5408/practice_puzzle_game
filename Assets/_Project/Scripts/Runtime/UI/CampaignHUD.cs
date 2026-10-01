using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed class CampaignHUD : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private SpriteRenderer backdrop;
        [SerializeField] private TMP_Text worldTitle;
        [SerializeField] private Button mapButton;
        [SerializeField] private GameObject[] collectionSlots;
        [SerializeField] private Image[] candyIcons;
        [SerializeField] private TMP_Text[] counts;
        [SerializeField] private Sprite[] candySprites;
        private int displayedWorld;

        public void Configure(LevelSession level, SpriteRenderer background, TMP_Text worldName, Button map,
            GameObject[] slots, Image[] icons, TMP_Text[] values, Sprite[] candies)
        { session = level; backdrop = background; worldTitle = worldName; mapButton = map; collectionSlots = slots; candyIcons = icons; counts = values; candySprites = candies; }

        private void OnEnable() { session.Changed += Refresh; mapButton.onClick.AddListener(OpenMap); }
        private void OnDisable() { if (session) session.Changed -= Refresh; if (mapButton) mapButton.onClick.RemoveListener(OpenMap); }
        private void Update() { mapButton.interactable = session.Progress != null && !session.Progress.IsResolving; }

        public void OpenMap()
        {
            if (session.Progress == null || session.Progress.IsResolving) return;
            SceneManager.LoadSceneAsync("WorldMap", LoadSceneMode.Single);
        }

        private void Refresh()
        {
            if (!session.Catalog || session.Progress == null) return;
            int world = session.Definition.World;
            if (displayedWorld != world)
            {
                displayedWorld = world; backdrop.sprite = session.Catalog.Background(world);
                backdrop.sharedMaterial = session.Catalog.BackgroundMaterial(world);
            }
            worldTitle.text = session.Catalog.WorldName(world);
            int slot = 0;
            for (int i = 1; i <= 6; i++)
            {
                var color = (PieceColor)i; int target = session.Progress.Rules.CollectionTarget(color);
                if (target == 0 || slot >= collectionSlots.Length) continue;
                collectionSlots[slot].SetActive(true); candyIcons[slot].sprite = candySprites[i - 1];
                counts[slot].text = Mathf.Min(target, session.Progress.Collected(color)) + "/" + target;
                counts[slot].color = session.Progress.Collected(color) >= target ? new Color32(54, 133, 97, 255) : new Color32(103, 57, 107, 255);
                slot++;
            }
            for (; slot < collectionSlots.Length; slot++) collectionSlots[slot].SetActive(false);
        }
    }
}
