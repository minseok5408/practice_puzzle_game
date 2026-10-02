using System.Collections;
using System.Collections.Generic;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
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
        private bool applying;
        private Vector2Int appliedSize;
        private bool appliedWindowed;
        private string settingsPath;
        private string statusKey = "displayHint";
        private PlayerDialogs Dialogs => GetComponent<PlayerDialogs>();
        public bool IsVisible => Dialogs && Dialogs.IsVisible && Dialogs.IsSettings;
        public Button OpenButton => openButton;
        public Button CloseButton => Dialogs && Dialogs.SettingsCloseButton ? Dialogs.SettingsCloseButton : closeButton;
        public Button ApplyButton => Dialogs && Dialogs.DisplayApply ? Dialogs.DisplayApply : applyButton;
        public Button QuitButton => quitButton;
        public TMP_Dropdown ResolutionDropdown => Dialogs && Dialogs.DisplayResolution ? Dialogs.DisplayResolution : resolution;
        public TMP_Dropdown ResolutionTemplate => resolution;
        public Toggle WindowedToggle => Dialogs && Dialogs.DisplayWindowed ? Dialogs.DisplayWindowed : windowed;
        public IReadOnlyList<Vector2Int> AvailableResolutions => sizes;
        public bool IsApplying => applying;
        public string SettingsPath => settingsPath;
        public string StatusText => Localization.Get(statusKey);

        public void Configure(BoardController controller, GameObject panel, RectTransform panelCard,
            Button open, Button close, Button apply, Button quit, TMP_Dropdown resolutions, Toggle window, TMP_Text message)
        {
            board = controller; overlay = panel; card = panelCard;
            openButton = open; closeButton = close; applyButton = apply; quitButton = quit;
            resolution = resolutions; windowed = window; status = message;
        }
        private void OnEnable()
        {
            escape = new InputAction("Settings", InputActionType.Button, "<Keyboard>/escape"); escape.Enable();
            openButton.onClick.AddListener(Open); closeButton.onClick.AddListener(Close);
            applyButton.onClick.AddListener(Apply); quitButton.onClick.AddListener(Quit);
        }
        private void Start()
        {
            overlay.SetActive(false); settingsPath = DisplaySettings.SavePath;
            CandyUIStyle.Button(openButton, CandyButtonRole.Secondary, card.GetComponent<Image>().sprite);
            RefreshDisplayChoices();
        }
        private void Update()
        {
            if (escape.WasPressedThisFrame())
            {
                if (Dialogs && Dialogs.IsVisible)
                {
                    if (Dialogs.DisplayResolution && Dialogs.DisplayResolution.IsExpanded) Dialogs.DisplayResolution.Hide();
                    else Dialogs.Close();
                }
                else if (board && (board.SelectedItem.HasValue || board.SelectedPosition.HasValue || board.GetComponent<BoardInput>().KeyboardActive)) board.GetComponent<BoardInput>().CancelGesture();
                else Open();
            }
            if (IsVisible) RefreshApply();
        }
        public void Open()
        {
            if (IsVisible) return;
            RefreshDisplayChoices(); Dialogs.UnifiedSettings();
        }
        public void RefreshDisplayChoices()
        {
            if (applying) return;
            appliedSize = new Vector2Int(Screen.width, Screen.height);
            appliedWindowed = Screen.fullScreenMode == FullScreenMode.Windowed;
            sizes.Clear(); sizes.AddRange(DisplaySettings.Resolutions(new Vector2Int(Mathf.Max(960, Screen.currentResolution.width), Mathf.Max(600, Screen.currentResolution.height)), appliedSize, Screen.resolutions));
            resolution.ClearOptions(); var labels = new List<string>(); foreach (var size in sizes) labels.Add(size.x + " x " + size.y);
            resolution.AddOptions(labels); resolution.SetValueWithoutNotify(Mathf.Max(0, sizes.IndexOf(appliedSize)));
            windowed.SetIsOnWithoutNotify(appliedWindowed); statusKey = "displayHint";
        }
        public void Close() { if (!applying) Dialogs?.Close(); }
        private void RefreshApply()
        {
            if (CloseButton) CloseButton.interactable = !applying;
            if (ApplyButton) ApplyButton.interactable = !applying && sizes.Count > 0
                && (sizes[Mathf.Clamp(ResolutionDropdown.value, 0, sizes.Count - 1)] != appliedSize || WindowedToggle.isOn != appliedWindowed);
        }
        public void Apply() { if (!applying && sizes.Count > 0) StartCoroutine(ApplyDisplay()); }
        private IEnumerator ApplyDisplay()
        {
            Vector2Int selected = sizes[Mathf.Clamp(ResolutionDropdown.value, 0, sizes.Count - 1)]; bool asWindow = WindowedToggle.isOn;
            applying = true; RefreshApply();
            ResolutionDropdown.interactable = WindowedToggle.interactable = false;
            var mode = asWindow ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;
            Screen.SetResolution(selected.x, selected.y, mode);
            yield return null; yield return null;
            float deadline = Time.realtimeSinceStartup + 2;
            while (!Application.isEditor && (Screen.width != selected.x || Screen.height != selected.y || Screen.fullScreenMode != mode) && Time.realtimeSinceStartup < deadline) yield return null;
            appliedSize = new Vector2Int(Screen.width, Screen.height); appliedWindowed = Screen.fullScreenMode == FullScreenMode.Windowed;
            var preferences = GamePreferences.Current; preferences.width = appliedSize.x; preferences.height = appliedSize.y; preferences.windowed = appliedWindowed;
            statusKey = GamePreferences.Save() ? "displaySaved" : "saveFailed";
            int actual = sizes.IndexOf(appliedSize);
            if (actual < 0)
            {
                sizes.Add(appliedSize); actual = sizes.Count - 1;
                var label = new List<string> { appliedSize.x + " x " + appliedSize.y };
                resolution.AddOptions(label); if (ResolutionDropdown != resolution) ResolutionDropdown.AddOptions(label);
            }
            resolution.SetValueWithoutNotify(actual); ResolutionDropdown.SetValueWithoutNotify(actual);
            windowed.SetIsOnWithoutNotify(appliedWindowed); WindowedToggle.SetIsOnWithoutNotify(appliedWindowed);
            ResolutionDropdown.interactable = WindowedToggle.interactable = true;
            applying = false; RefreshApply();
        }
        public void Quit()
        {
            if (applying) return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        private void OnDisable()
        {
            StopAllCoroutines(); applying = false; Close(); escape?.Dispose(); escape = null;
            if (openButton) openButton.onClick.RemoveListener(Open); if (closeButton) closeButton.onClick.RemoveListener(Close);
            if (applyButton) applyButton.onClick.RemoveListener(Apply); if (quitButton) quitButton.onClick.RemoveListener(Quit);
        }
    }
}
