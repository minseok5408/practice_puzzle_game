using System.Globalization;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed class ResultPopup : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private GameObject overlay;
        [SerializeField] private TMP_Text titleText, detailText, messageText;
        [SerializeField] private Button restartButton;
        public bool IsVisible => overlay && overlay.activeSelf;
        public Button RestartButton => restartButton;
        public string Title => titleText.text;

        public void Configure(LevelSession level, GameObject panel, TMP_Text title, TMP_Text detail,
            TMP_Text message, Button restart)
        {
            session = level; overlay = panel; titleText = title; detailText = detail;
            messageText = message; restartButton = restart;
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
            bool show = p != null && p.IsFinished;
            overlay.SetActive(show);
            if (!show) return;
            bool won = p.Outcome == LevelOutcome.Won;
            titleText.text = won ? "스테이지 성공!" : "스테이지 실패";
            titleText.color = won ? new Color32(154, 58, 127, 255) : new Color32(113, 82, 153, 255);
            detailText.text = "획득 " + p.Score.ToString("N0", CultureInfo.InvariantCulture)
                + "점  /  목표 " + p.Rules.TargetScore.ToString("N0", CultureInfo.InvariantCulture) + "점";
            messageText.text = won ? "목표 점수를 달성했어요!\n남은 이동 " + p.MovesRemaining + "회"
                : "이동 횟수를 모두 사용했어요.\n다시 도전해 보세요!";
        }
    }
}
