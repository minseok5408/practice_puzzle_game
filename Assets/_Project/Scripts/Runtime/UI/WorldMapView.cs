using PuzzleGame.Runtime.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed class WorldMapView : MonoBehaviour
    {
        [SerializeField] private LevelCatalog catalog;
        [SerializeField] private Image backdrop, surrounding;
        [SerializeField] private RectTransform composition;
        [SerializeField] private TMP_Text title, summary, worldTag;
        [SerializeField] private Button[] worldButtons, stageButtons;
        [SerializeField] private TMP_Text[] stageLabels, stateLabels;
        [SerializeField] private Sprite availableMedal, lockedMedal, completeMedal;
        [SerializeField] private Button continueButton;
        [SerializeField] private RectTransform currentMarker, halo;
        [SerializeField] private RectTransform[] sparkles;
        private Vector2[] sparkleOrigins;
        private CampaignState campaign;
        private bool entering;
        private int currentIndex = -1;
        public int World { get; private set; } = 1;
        public Button[] StageButtons => stageButtons;
        public Button ContinueButton => continueButton;

        public void Configure(LevelCatalog levels, Image background, Image outer, RectTransform layout,
            TMP_Text heading, TMP_Text progress, TMP_Text tag, Button[] worlds, Button[] stages,
            TMP_Text[] numbers, TMP_Text[] states, Button resume, Sprite available, Sprite locked, Sprite complete,
            RectTransform marker, RectTransform glow, RectTransform[] particles)
        {
            catalog = levels; backdrop = background; surrounding = outer; composition = layout; title = heading;
            summary = progress; worldTag = tag; worldButtons = worlds; stageButtons = stages; stageLabels = numbers;
            stateLabels = states; continueButton = resume; availableMedal = available; lockedMedal = locked; completeMedal = complete;
            currentMarker = marker; halo = glow; sparkles = particles;
        }

        private void Start()
        {
            campaign = CampaignState.Ensure(catalog);
            for (int i = 0; i < worldButtons.Length; i++) { int world = i + 1; worldButtons[i].onClick.AddListener(() => ShowWorld(world)); }
            for (int i = 0; i < stageButtons.Length; i++) { int index = i; stageButtons[i].onClick.AddListener(() => Enter((World - 1) * LevelCatalog.StagesPerWorld + index + 1)); }
            continueButton.onClick.AddListener(() => Enter(campaign.Progress.UnlockedThrough));
            sparkleOrigins = new Vector2[sparkles.Length];
            for (int i = 0; i < sparkles.Length; i++) sparkleOrigins[i] = sparkles[i].anchoredPosition;
            ShowWorld((campaign.Progress.UnlockedThrough - 1) / LevelCatalog.StagesPerWorld + 1);
        }

        public void ShowWorld(int world)
        {
            if (world < 1 || world > 5 || entering || !campaign) return;
            World = world; backdrop.sprite = surrounding.sprite = catalog.MapArtwork(world);
            title.text = catalog.WorldName(world); worldTag.text = "WORLD  " + world.ToString("00");
            var progress = campaign.Progress;
            int completedHere = 0; currentIndex = -1;
            for (int i = 0; i < worldButtons.Length; i++)
                worldButtons[i].GetComponent<Image>().color = i + 1 == world ? new Color32(214, 66, 135, 255) : new Color32(131, 100, 146, 242);
            for (int i = 0; i < stageButtons.Length; i++)
            {
                int number = (world - 1) * LevelCatalog.StagesPerWorld + i + 1;
                bool unlocked = progress.IsUnlocked(number), complete = progress.IsComplete(number);
                if (complete) completedHere++;
                stageButtons[i].interactable = unlocked;
                stageButtons[i].GetComponent<Image>().sprite = complete ? completeMedal : unlocked ? availableMedal : lockedMedal;
                stageButtons[i].transform.localScale = Vector3.one;
                stageLabels[i].text = catalog.Get(number).Stage.ToString();
                stageLabels[i].color = unlocked ? new Color32(255, 252, 234, 255) : new Color32(113, 87, 133, 255);
                stateLabels[i].text = "";
                if (number == progress.UnlockedThrough && !complete) currentIndex = i;
            }
            summary.text = "이 월드 " + completedHere + " / 10    전체 " + progress.CompletedCount + " / 50";
            continueButton.GetComponentInChildren<TMP_Text>().text = progress.CompletedCount == LevelCatalog.LevelCount ? "5-10 다시 하기" : catalog.Get(progress.UnlockedThrough).DisplayName + " 도전하기";
            currentMarker.gameObject.SetActive(currentIndex >= 0); halo.gameObject.SetActive(currentIndex >= 0);
            if (currentIndex >= 0)
            {
                var node = (RectTransform)stageButtons[currentIndex].transform;
                currentMarker.anchoredPosition = node.anchoredPosition + new Vector2(0, 45);
                halo.anchoredPosition = node.anchoredPosition;
            }
        }

        public void Enter(int number)
        {
            if (entering || !campaign || !campaign.Select(number)) return;
            entering = true;
            foreach (var button in stageButtons) button.interactable = false;
            continueButton.interactable = false;
            SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
        }

        private void LateUpdate()
        {
            var parent = (RectTransform)composition.parent;
            composition.localScale = Vector3.one * Mathf.Min(parent.rect.width / 1280f, parent.rect.height / 800f);
            float time = Time.unscaledTime;
            if (currentIndex >= 0 && !entering)
            {
                var node = (RectTransform)stageButtons[currentIndex].transform;
                currentMarker.anchoredPosition = node.anchoredPosition + new Vector2(0, 45 + Mathf.Sin(time * 2.6f) * 2);
                halo.localScale = Vector3.one * (1 + Mathf.Sin(time * 2.6f) * .08f);
            }
            if (sparkleOrigins == null) return;
            for (int i = 0; i < sparkles.Length; i++)
            {
                sparkles[i].anchoredPosition = sparkleOrigins[i] + new Vector2(Mathf.Sin(time * .35f + i) * 5, Mathf.Sin(time * .5f + i * 2) * 9);
                sparkles[i].localScale = Vector3.one * (.6f + .4f * Mathf.Sin(time * 1.6f + i));
            }
        }
    }
}
