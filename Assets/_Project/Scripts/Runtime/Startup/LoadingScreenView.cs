using TMPro;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.Startup
{
    public sealed class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private RectTransform composition, fill, sparkle;
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject recovery;
        [SerializeField] private Button retryButton, quitButton;
        private bool failed;
        private float progress;
        private readonly string[] messages = { "로딩 중", "로딩 중.", "로딩 중..", "로딩 중..." };
        public Button RetryButton => retryButton;
        public Button QuitButton => quitButton;
        public float Progress => progress;
        public string StatusText => label.text;

        public void Configure(RectTransform artwork, RectTransform progressFill, RectTransform shine,
            TMP_Text status, GameObject recoveryButtons, Button retry, Button quit)
        {
            composition = artwork; fill = progressFill; sparkle = shine;
            label = status; recovery = recoveryButtons; retryButton = retry; quitButton = quit;
        }

        public void ResetLoading()
        {
            failed = false; recovery.SetActive(false); SetProgress(0);
        }

        private void Start()
        {
            CandyUIStyle.Button(retryButton,CandyButtonRole.Primary);
            CandyUIStyle.Button(quitButton,CandyButtonRole.Danger);
        }

        public void SetProgress(float value)
        {
            progress = Mathf.Clamp01(value);
            fill.anchorMax = new Vector2(progress, 1);
            fill.gameObject.SetActive(progress > .001f);
            sparkle.anchorMin = sparkle.anchorMax = new Vector2(progress, .5f);
            sparkle.gameObject.SetActive(!failed && progress > .02f && progress < 1);
        }

        public void ShowFailure()
        {
            failed = true; label.text = Localization.Get("loadFailed");
            sparkle.gameObject.SetActive(false); recovery.SetActive(true);
        }

        private void Update()
        {
            ApplyLayout();
            if (failed) return;
            label.text = Localization.Get("loading") + new string('.', (int)(Time.unscaledTime * 2.5f) % 4);
            sparkle.localScale = Vector3.one * (GamePreferences.Current.reducedEffects ? 1 : 1 + .18f * Mathf.Sin(Time.unscaledTime * 5));
        }

        public void ApplyLayout()
        {
            if (!composition) return;
            var parent = (RectTransform)composition.parent;
            // Keep the complete approved composition on 16:9, 16:10 and narrow windows.
            float scale = Mathf.Min(parent.rect.width / 1280f, parent.rect.height / 720f);
            composition.sizeDelta = new Vector2(1280, 720);
            composition.localScale = Vector3.one * scale;
        }
    }
}
