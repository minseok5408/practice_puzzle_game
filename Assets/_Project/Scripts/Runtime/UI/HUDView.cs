using System.Globalization;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed class HUDView : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private TMP_Text titleText, scoreText, targetText, movesText, statusText;
        [SerializeField] private RectTransform progressFill;
        [SerializeField] private Button restartButton;
        public string ScoreText => scoreText.text;
        public string MovesText => movesText.text;
        public Button RestartButton => restartButton;

        public void Configure(LevelSession level, TMP_Text title, TMP_Text score, TMP_Text target,
            TMP_Text moves, TMP_Text status, RectTransform fill, Button restart)
        {
            session = level; titleText = title; scoreText = score; targetText = target;
            movesText = moves; statusText = status; progressFill = fill; restartButton = restart;
        }

        private void OnEnable()
        {
            session.Changed += Refresh;
            GamePreferences.Changed += Refresh;
            restartButton.onClick.AddListener(Restart);
            Refresh();
        }

        private void OnDisable()
        {
            if (session) session.Changed -= Refresh;
            GamePreferences.Changed -= Refresh;
            if (restartButton) restartButton.onClick.RemoveListener(Restart);
        }

        private void Restart()
        {var dialogs=GetComponent<PlayerDialogs>();if(dialogs)dialogs.ConfirmAbandon(true,session.RestartLevel);else session.RestartLevel();}

        private void Refresh()
        {
            var p = session.Progress;
            if (p == null) return;
            titleText.text = session.Catalog ? Localization.Get("mapShort") : session.DisplayName;
            scoreText.text = p.Score.ToString("N0", CultureInfo.InvariantCulture);
            targetText.text = p.Rules.TargetScore.ToString("N0", CultureInfo.InvariantCulture);
            movesText.text = p.MovesRemaining.ToString();
            movesText.color = p.MovesRemaining <= 5 ? new Color32(255, 224, 112, 255) : Color.white;
            progressFill.anchorMax = new Vector2(Mathf.Clamp01((float)p.Score / p.Rules.TargetScore), 1);
            bool scoreComplete=p.Score>=p.Rules.TargetScore;
            targetText.color=scoreComplete?new Color32(47,112,82,255):CandyUIStyle.Pink;
            progressFill.GetComponent<Image>().color=scoreComplete?new Color32(86,167,119,255):CandyUIStyle.Pink;
            if (statusText) statusText.text = Localization.Get(p.IsResolving ? "resolving" : p.IsFinished ? "resultHint" : "dragHint");
            restartButton.interactable = !p.IsResolving && !p.IsFinished;
        }
    }
}
