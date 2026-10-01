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
        [SerializeField] private Button nextButton, mapButton;
        public Button NextButton => nextButton;
        public Button MapButton => mapButton;
        public void ConfigureNavigation(Button next, Button map) { nextButton = next; mapButton = map; }
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
            if (nextButton) nextButton.onClick.AddListener(Next);
            if (mapButton) mapButton.onClick.AddListener(Map);
            Refresh();
        }

        private void OnDisable()
        {
            if (session) session.Changed -= Refresh;
            if (restartButton) restartButton.onClick.RemoveListener(Restart);
            if (nextButton) nextButton.onClick.RemoveListener(Next);
            if (mapButton) mapButton.onClick.RemoveListener(Map);
        }

        private void Restart() => session.RestartLevel();
        private void Next() => session.NextLevel();
        private void Map() => UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("WorldMap");

        private void Refresh()
        {
            var p = session.Progress;
            bool show = p != null && p.IsFinished;
            overlay.SetActive(show);
            if (!show) return;
            bool won = p.Outcome == LevelOutcome.Won;
            bool final = session.IsCampaignRun && session.Definition.Number == LevelCatalog.LevelCount;
            if (nextButton) nextButton.gameObject.SetActive(won && !final && session.IsCampaignRun);
            if (mapButton) mapButton.gameObject.SetActive(session.Catalog);
            titleText.text = won ? "스테이지 성공!" : "스테이지 실패";
            titleText.color = won ? new Color32(154, 58, 127, 255) : new Color32(113, 82, 153, 255);
            detailText.text = "획득 " + p.Score.ToString("N0", CultureInfo.InvariantCulture)
                + "점  /  목표 " + p.Rules.TargetScore.ToString("N0", CultureInfo.InvariantCulture) + "점";
            messageText.text = won ? (final ? LevelCatalog.LevelCount + "개 스테이지를 모두 클리어했어요!" : "모든 목표를 달성했어요!\n남은 이동 " + p.MovesRemaining + "회")
                : "이동 횟수를 모두 사용했어요.\n다시 도전해 보세요!";
            if (session.SaveFailed) messageText.text += "\n진행 기록을 저장하지 못했어요.";
        }
    }
}
