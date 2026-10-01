using System.Globalization;
using PuzzleGame.Runtime.Levels;
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
            restartButton.onClick.AddListener(Restart);
            Refresh();
        }

        private void OnDisable()
        {
            if (session) session.Changed -= Refresh;
            if (restartButton) restartButton.onClick.RemoveListener(Restart);
        }

        private void Restart() => session.RestartLevel();

        private void Refresh()
        {
            var p = session.Progress;
            if (p == null) return;
            titleText.text = session.DisplayName;
            scoreText.text = p.Score.ToString("N0", CultureInfo.InvariantCulture);
            targetText.text = p.Rules.TargetScore.ToString("N0", CultureInfo.InvariantCulture);
            movesText.text = p.MovesRemaining.ToString();
            movesText.color = p.MovesRemaining <= 5 ? new Color32(255, 224, 112, 255) : Color.white;
            progressFill.anchorMax = new Vector2(Mathf.Clamp01((float)p.Score / p.Rules.TargetScore), 1);
            if (statusText) statusText.text = p.IsResolving ? "블록을 정리하는 중..." :
                p.IsFinished ? "아래 결과를 확인하세요" : "블록을 누른 채 옆으로 끌어 놓으세요";
            restartButton.interactable = !p.IsResolving && !p.IsFinished;
        }
    }
}
