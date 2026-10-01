using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PuzzleGame.Runtime.Board;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    [DefaultExecutionOrder(-100)]
    public sealed class SettingsPopup : MonoBehaviour
    {
        [SerializeField] private BoardController board;
        [SerializeField] private GameObject overlay;
        [SerializeField] private RectTransform card;
        [SerializeField] private Button openButton, closeButton, applyButton, quitButton;
        [SerializeField] private TMP_Dropdown resolution;
        [SerializeField] private Toggle windowed;
        [SerializeField] private TMP_Text status;
        private readonly List<Vector2Int> sizes = new List<Vector2Int>();
        private InputAction escape;
        private float previousTimeScale;
        private bool ownsPause, applying;
        private Vector2Int appliedSize;
        private bool appliedWindowed;
        private string settingsPath;
        public bool IsVisible => overlay && overlay.activeSelf;
        public Button OpenButton => openButton;
        public Button CloseButton => closeButton;
        public Button ApplyButton => applyButton;
        public Button QuitButton => quitButton;
        public TMP_Dropdown ResolutionDropdown => resolution;
        public Toggle WindowedToggle => windowed;
        public IReadOnlyList<Vector2Int> AvailableResolutions => sizes;
        public bool IsApplying => applying;
        public string SettingsPath => settingsPath;

        public void Configure(BoardController controller, GameObject panel, RectTransform panelCard,
            Button open, Button close, Button apply, Button quit, TMP_Dropdown resolutions, Toggle window, TMP_Text message)
        {
            board = controller; overlay = panel; card = panelCard;
            openButton = open; closeButton = close; applyButton = apply; quitButton = quit;
            resolution = resolutions; windowed = window; status = message;
        }

        private void OnEnable()
        {
            escape = new InputAction("Settings", InputActionType.Button, "<Keyboard>/escape");
            escape.Enable();
            openButton.onClick.AddListener(Open);
            closeButton.onClick.AddListener(Close);
            applyButton.onClick.AddListener(Apply);
            quitButton.onClick.AddListener(Quit);
            resolution.onValueChanged.AddListener(ResolutionChanged);
            windowed.onValueChanged.AddListener(WindowModeChanged);
        }

        private IEnumerator Start()
        {
            settingsPath = Path.Combine(Application.persistentDataPath, "settings.json");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Opt-in diagnostics use an isolated file, never the player's preferences.
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-puzzleSettingsPath");
            if (index >= 0 && index + 1 < args.Length) settingsPath = args[index + 1];
#endif
            appliedSize = new Vector2Int(Screen.width,Screen.height);
            appliedWindowed = Screen.fullScreenMode == FullScreenMode.Windowed;
            if (!Application.isEditor)
            {
                DisplaySettings saved = DisplaySettings.Load(settingsPath);
                if (saved != null)
                {
                    var desktop = DesktopSize();
                    appliedSize = new Vector2Int(Mathf.Min(saved.width,desktop.x),Mathf.Min(saved.height,desktop.y));
                    appliedWindowed = saved.windowed;
                    Screen.SetResolution(appliedSize.x,appliedSize.y,appliedWindowed ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow);
                    yield return null; yield return null;
                }
            }
        }

        private void Update()
        {
            if (escape.WasPressedThisFrame())
            {
                if (IsVisible && resolution.IsExpanded) resolution.Hide();
                else if (IsVisible) Close();
                else Open();
            }
            if (IsVisible)
            {
                var parent = (RectTransform)overlay.transform;
                card.sizeDelta = new Vector2(Mathf.Min(560,parent.rect.width-32),Mathf.Min(530,parent.rect.height-32));
            }
        }

        public void Open()
        {
            if (IsVisible) return;
            board.GetComponent<BoardInput>().CancelGesture();
            previousTimeScale = Time.timeScale;
            ownsPause = true;
            board.SetPaused(true);
            Time.timeScale = 0;
            overlay.SetActive(true);
            overlay.transform.SetAsLastSibling();
            // Window resizing and Alt+Enter may have changed the actual display since last opening.
            appliedSize = new Vector2Int(Screen.width,Screen.height);
            appliedWindowed = Screen.fullScreenMode == FullScreenMode.Windowed;
            sizes.Clear();
            sizes.AddRange(DisplaySettings.Resolutions(DesktopSize(), appliedSize, Screen.resolutions));
            resolution.ClearOptions();
            var labels = new List<string>();
            foreach (var size in sizes) labels.Add(size.x + " x " + size.y);
            resolution.AddOptions(labels);
            int current = sizes.IndexOf(appliedSize);
            resolution.SetValueWithoutNotify(current >= 0 ? current : sizes.Count-1);
            windowed.SetIsOnWithoutNotify(appliedWindowed);
            status.text = "화면에 맞는 크기를 선택하세요.";
            RefreshApply();
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
        }

        public void Close()
        {
            if (!ownsPause) return;
            resolution.Hide();
            overlay.SetActive(false);
            Time.timeScale = previousTimeScale;
            if (board) board.SetPaused(false);
            ownsPause = false;
            if (EventSystem.current && openButton && openButton.IsActive())
                EventSystem.current.SetSelectedGameObject(openButton.gameObject);
        }

        private void ResolutionChanged(int value) => RefreshApply();
        private void WindowModeChanged(bool value) => RefreshApply();
        private void RefreshApply()
        {
            applyButton.interactable = !applying && sizes.Count > 0 &&
                (sizes[resolution.value] != appliedSize || windowed.isOn != appliedWindowed);
        }
        public void Apply()
        {
            if (applying || sizes.Count == 0) return;
            StartCoroutine(ApplyDisplay());
        }
        private IEnumerator ApplyDisplay()
        {
            applying = true; RefreshApply();
            Vector2Int selected = sizes[resolution.value]; bool asWindow = windowed.isOn;
            resolution.interactable = windowed.interactable = false;
            Screen.SetResolution(selected.x,selected.y,asWindow ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow);
            // SetResolution completes after the current frame; UI must keep working while paused.
            yield return null; yield return null;
            appliedSize = new Vector2Int(Screen.width,Screen.height);
            appliedWindowed = Screen.fullScreenMode == FullScreenMode.Windowed;
            bool saved = Application.isEditor || new DisplaySettings {
                width = appliedSize.x, height = appliedSize.y, windowed = appliedWindowed
            }.Save(settingsPath);
            int actual = sizes.IndexOf(appliedSize);
            if (actual < 0)
            {
                sizes.Add(appliedSize); actual = sizes.Count-1;
                resolution.AddOptions(new List<string> { appliedSize.x + " x " + appliedSize.y });
            }
            resolution.SetValueWithoutNotify(actual);
            windowed.SetIsOnWithoutNotify(appliedWindowed);
            status.text = saved ? "화면 설정을 적용했어요." : "화면은 변경됐지만 설정을 저장하지 못했어요.";
            resolution.interactable = windowed.interactable = true;
            applying = false; RefreshApply();
        }

        private static Vector2Int DesktopSize() => new Vector2Int(
            Mathf.Max(960, Screen.currentResolution.width),Mathf.Max(600,Screen.currentResolution.height));

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDisable()
        {
            Close();
            StopAllCoroutines(); applying = false;
            if (resolution) resolution.interactable = true;
            if (windowed) windowed.interactable = true;
            escape?.Dispose(); escape = null;
            if (openButton) openButton.onClick.RemoveListener(Open);
            if (closeButton) closeButton.onClick.RemoveListener(Close);
            if (applyButton) applyButton.onClick.RemoveListener(Apply);
            if (quitButton) quitButton.onClick.RemoveListener(Quit);
            if (resolution) resolution.onValueChanged.RemoveListener(ResolutionChanged);
            if (windowed) windowed.onValueChanged.RemoveListener(WindowModeChanged);
        }
    }
}
