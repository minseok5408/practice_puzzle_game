using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed partial class ResultPopup : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private GameObject overlay;
        [SerializeField] private TMP_Text titleText, detailText, messageText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button nextButton, mapButton;
        [SerializeField] private Sprite victoryBadge, retryBadge, completionCrown;
        [SerializeField] private Sprite[] confettiCandies;
        private RectTransform card;
        private ResultPresentation presentation;
        private LevelProgress presentedProgress;
        private TMP_Text rewardText;
        public Button NextButton => nextButton;
        public Button MapButton => mapButton;
        public void ConfigureNavigation(Button next, Button map) { nextButton = next; mapButton = map; }
        public bool IsVisible => overlay && overlay.activeSelf;
        public Button RestartButton => restartButton;
        public string Title => titleText.text;
        public ResultPresentation Presentation=>presentation;
        public void ConfigureArtwork(Sprite victory,Sprite retry,Sprite completion,Sprite[] candies)
        {victoryBadge=victory;retryBadge=retry;completionCrown=completion;confettiCandies=candies;}

        public void Configure(LevelSession level, GameObject panel, TMP_Text title, TMP_Text detail,
            TMP_Text message, Button restart)
        {
            session = level; overlay = panel; titleText = title; detailText = detail;
            messageText = message; restartButton = restart;
        }

        private void OnEnable()
        {
            card=(RectTransform)overlay.transform.Find("ResultCard");
            var rounded=card.GetComponent<Image>().sprite;
            CandyUIStyle.Popup(overlay,card,rounded);
            CandyUIStyle.Button(nextButton,CandyButtonRole.Primary,rounded);
            CandyUIStyle.Button(mapButton,CandyButtonRole.Quiet,rounded);
            CandyUIStyle.ActionRow(restartButton,0);CandyUIStyle.ActionRow(mapButton,1);
            float actionBottom=CandyUIStyle.Padding+CandyUIStyle.ButtonHeight+CandyUIStyle.Gap;
            CandyUIStyle.ActionRow(nextButton,-1,actionBottom);
            var feedback=card.Find("Feedback")?.GetComponent<Button>();
            CandyUIStyle.Button(feedback,CandyButtonRole.Secondary,rounded);
            CandyUIStyle.ActionRow(feedback,-1,actionBottom);
            presentation=card.GetComponent<ResultPresentation>();if(!presentation)presentation=card.gameObject.AddComponent<ResultPresentation>();
            presentation.Configure(card.Find("ResultStar").GetComponent<Image>(),victoryBadge,retryBadge,completionCrown,confettiCandies);
            LayoutText(titleText,154,48,32);
            LayoutText(detailText,211,34,22);
            LayoutText(messageText,258,76,20);
            if(!rewardText)
            {
                var root=new GameObject("ItemReward",typeof(RectTransform));root.transform.SetParent(card,false);
                rewardText=root.AddComponent<TextMeshProUGUI>();rewardText.font=messageText.font;rewardText.color=CandyUIStyle.Pink;
            }
            LayoutText(rewardText,337,26,18);
            BuildDetails();
            session.Changed += Refresh;
            GamePreferences.Changed += Refresh;
            restartButton.onClick.AddListener(Restart);
            if (nextButton) nextButton.onClick.AddListener(Next);
            if (mapButton) mapButton.onClick.AddListener(Map);
            Refresh();
        }

        private void OnDisable()
        {
            if (session) session.Changed -= Refresh;
            GamePreferences.Changed -= Refresh;
            if (restartButton) restartButton.onClick.RemoveListener(Restart);
            if (nextButton) nextButton.onClick.RemoveListener(Next);
            if (mapButton) mapButton.onClick.RemoveListener(Map);
            if(presentation)presentation.Settle();
            presentedProgress=null;
        }

        private void Restart() => session.RestartLevel();
        private void Next() => session.NextLevel();
        private void Map() => UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("WorldMap");

        private void Update()
        {
            if(!IsVisible)return;
            var parent=(RectTransform)overlay.transform;
            card.sizeDelta=new Vector2(Mathf.Min(580,parent.rect.width-2*CandyUIStyle.Padding),Mathf.Min(720,parent.rect.height-2*CandyUIStyle.Padding));
        }

        private static void LayoutText(TMP_Text text,float top,float height,float size)
        {
            var rect=text.rectTransform;rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;
            rect.offsetMin=new Vector2(CandyUIStyle.Padding,-top-height);rect.offsetMax=new Vector2(-CandyUIStyle.Padding,-top);
            text.fontSize=size;text.fontSizeMax=size;text.fontSizeMin=size-4;text.enableAutoSizing=true;
            text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
        }

        private void Refresh()
        {
            var p = session.Progress;
            bool show = p != null && p.IsFinished;
            overlay.SetActive(show);
            if (!show){presentedProgress=null;return;}
            bool won = p.Outcome == LevelOutcome.Won;
            bool final = session.IsCampaignRun && session.Definition.Number == LevelCatalog.LevelCount;
            CandyUIStyle.Button(restartButton,won && !final && session.IsCampaignRun?CandyButtonRole.Secondary:CandyButtonRole.Primary,card.GetComponent<Image>().sprite);
            var feedback = overlay.transform.Find("ResultCard/Feedback");
            if (feedback) feedback.gameObject.SetActive(!won);
            if (nextButton) nextButton.gameObject.SetActive(won && !final && session.IsCampaignRun);
            if (mapButton) mapButton.gameObject.SetActive(session.Catalog);
            titleText.text = Localization.Get(won ? (final ? "complete" : "won") : "lost");
            titleText.color = won ? new Color32(154, 58, 127, 255) : new Color32(113, 82, 153, 255);
            detailText.text = Localization.Get("resultScore", p.Score, p.Rules.TargetScore);
            messageText.text = won ? (final ? Localization.Get("completeMessage") : Localization.Get("wonMessage", p.MovesRemaining)) : Localization.Get(p.IsBlocked ? "boardBlocked" : "lostMessage");
            if (session.SaveFailed) messageText.text += "\n" + Localization.Get("progressFailed");
            int rewards=0,lastReward=0;
            for(int i=0;i<session.LastItemRewards.Length;i++)if(session.LastItemRewards[i]>0){rewards++;lastReward=i;}
            rewardText.text=rewards==4?Localization.Get("itemRewardAll"):rewards==1?Localization.Get("itemReward",Localization.Get(ItemToolbar.NameKey((ItemType)lastReward))):"";
            // A partially full inventory may receive only some of the four milestone rewards.
            if(rewards>1 && rewards<4)
            {
                var names=new System.Collections.Generic.List<string>();
                for(int i=0;i<session.LastItemRewards.Length;i++)if(session.LastItemRewards[i]>0)names.Add(Localization.Get(ItemToolbar.NameKey((ItemType)i))+" +1");
                rewardText.text=string.Join(" / ",names);
            }
            RefreshDetails();
            if(presentedProgress!=p){presentedProgress=p;presentation.Present(won,final);}
            else if(GamePreferences.Current.reducedEffects)presentation.Settle();
        }
    }
}
