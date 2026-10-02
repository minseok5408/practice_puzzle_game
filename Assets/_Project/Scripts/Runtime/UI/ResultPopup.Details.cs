using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed partial class ResultPopup
    {
        private RectTransform details;
        private ScrollRect detailsScroll;
        public string GoalSummary { get; private set; }
        private void BuildDetails()
        {
            LayoutText(titleText,91,46,30);
            var badge=(RectTransform)card.Find("ResultStar");badge.anchoredPosition=new Vector2(0,-10);badge.sizeDelta=new Vector2(100,80);
            var confetti=(RectTransform)card.Find("CandyConfetti");confetti.offsetMin=new Vector2(16,-90);confetti.offsetMax=new Vector2(-16,-8);
            detailText.gameObject.SetActive(false);messageText.gameObject.SetActive(false);rewardText.gameObject.SetActive(false);
            if(details)return;
            var viewport=ProgressWidgets.Rect("DetailsViewport",card);viewport.offsetMin=new Vector2(24,144);viewport.offsetMax=new Vector2(-24,-144);
            viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            details=ProgressWidgets.Rect("Content",viewport);details.anchorMin=new Vector2(0,1);details.anchorMax=Vector2.one;details.pivot=new Vector2(.5f,1);
            var layout=details.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=8;layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandHeight=false;
            details.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            detailsScroll=viewport.gameObject.AddComponent<ScrollRect>();detailsScroll.viewport=viewport;detailsScroll.content=details;detailsScroll.horizontal=false;detailsScroll.movementType=ScrollRect.MovementType.Clamped;detailsScroll.scrollSensitivity=30;
            var rail=ProgressWidgets.Rect("Scrollbar",card);rail.anchorMin=new Vector2(1,0);rail.anchorMax=Vector2.one;rail.offsetMin=new Vector2(-14,144);rail.offsetMax=new Vector2(-8,-144);
            rail.gameObject.AddComponent<Image>().color=new Color32(228,209,231,255);
            var handle=ProgressWidgets.Rect("Handle",rail);var image=handle.gameObject.AddComponent<Image>();image.color=CandyUIStyle.Purple;
            var bar=rail.gameObject.AddComponent<Scrollbar>();bar.handleRect=handle;bar.targetGraphic=image;bar.direction=Scrollbar.Direction.BottomToTop;
            detailsScroll.verticalScrollbar=bar;detailsScroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
        }
        private TMP_Text Detail(string name,string value,float height,float size=20)
        {
            var text=ProgressWidgets.Text(name,details,messageText.font,value,size);ProgressWidgets.Height(text.gameObject,height);return text;
        }
        private void RefreshDetails()
        {
            foreach(Transform child in details){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            var p=session.Progress;var dialogs=GetComponent<PlayerDialogs>();
            bool won=p.Outcome==LevelOutcome.Won;
            if(won && !session.WasAssistedClear)
            {
                Detail("Stars",ProgressWidgets.Stars(session.EarnedStars)+"  "+Localization.Get("rating",session.EarnedStars),40,26);
                string medals=(session.ItemsUsed==0?Localization.Get("cleanMedal"):"")+(StageRating.IsEfficient(p)?"   "+Localization.Get("efficientMedal"):"");
                if(!string.IsNullOrWhiteSpace(medals))Detail("Medals",medals.Trim(),30,18).color=new Color32(145,98,25,255);
                if(session.NewBest)Detail("NewBest",Localization.Get("newBest",session.PreviousBest.ToString("N0")),32,21).color=CandyUIStyle.Pink;
            }
            else if(session.WasAssistedClear)Detail("Assisted",Localization.Get("ratingExcluded"),42,18);
            Detail("Score",detailText.text,34,23);
            Detail("Statistics",Localization.Get("resultStats",p.MovesUsed,session.ItemsUsed),28,18);
            bool reward=false;foreach(int amount in session.LastItemRewards)reward|=amount>0;
            if(reward)
            {
                Detail("RewardTitle",Localization.Get("rewardPreview"),28,20).color=CandyUIStyle.Pink;
                var row=ProgressWidgets.Rect("Rewards",details);ProgressWidgets.Height(row.gameObject,78);
                ProgressWidgets.Rewards(row,messageText.font,dialogs?dialogs.ItemArtwork:null,session.LastItemRewards);
            }
            GoalSummary="";
            AddResultGoal("ScoreGoal",Localization.Get("targetScore"),p.Score,p.Rules.TargetScore,null);
            for(int i=1;i<=6;i++)
            {
                var color=(PieceColor)i;int target=p.Rules.CollectionTarget(color);if(target<=0)continue;
                string key=new[]{"red","orange","yellow","green","blue","purple"}[i-1];
                AddResultGoal(color.ToString(),Localization.Get("candyName",Localization.Get(key)),p.Collected(color),target,dialogs?dialogs.CandyArtwork(color):null);
            }
            if(p.Rules.FrostTarget>0)AddResultGoal("Frost",Localization.Get("clearFrost"),p.FrostCleared,p.Rules.FrostTarget,dialogs?dialogs.IceArtwork:null);
            Detail("Message",messageText.text,session.SaveFailed?90:won?52:60,19);
            if(won && !session.WasAssistedClear && session.EarnedStars<3)Detail("NextChallenge",Localization.Get("ratingRules",StageRating.ReserveTarget(p.Rules.StartingMoves)),60,18);
            if(presentedProgress!=p){detailsScroll.StopMovement();details.anchoredPosition=Vector2.zero;}
        }
        private void AddResultGoal(string name,string label,int current,int target,Sprite sprite)
        {
            int missing=Mathf.Max(0,target-current);string status=missing==0?Localization.Get("goalDone"):Localization.Get("goalShort",missing);
            GoalSummary+=label+": "+status+"\n";
            var row=ProgressWidgets.Rect(name,details);ProgressWidgets.Height(row.gameObject,42);
            var bg=row.gameObject.AddComponent<Image>();bg.sprite=card.GetComponent<Image>().sprite;bg.type=Image.Type.Sliced;bg.raycastTarget=false;
            bg.color=missing==0?new Color32(226,241,224,255):new Color32(246,226,233,255);
            if(sprite)
            {
                var art=ProgressWidgets.Rect("Icon",row).gameObject.AddComponent<Image>();art.sprite=sprite;art.preserveAspect=true;art.raycastTarget=false;
                art.rectTransform.anchorMin=art.rectTransform.anchorMax=new Vector2(0,.5f);art.rectTransform.anchoredPosition=new Vector2(25,0);art.rectTransform.sizeDelta=new Vector2(32,32);
            }
            var text=ProgressWidgets.Text("Label",row,messageText.font,label,18);text.alignment=TextAlignmentOptions.MidlineLeft;
            text.rectTransform.anchorMax=new Vector2(.60f,1);text.rectTransform.offsetMin=new Vector2(sprite?48:12,0);
            var value=ProgressWidgets.Text("Status",row,messageText.font,status,19);value.alignment=TextAlignmentOptions.MidlineRight;
            value.rectTransform.anchorMin=new Vector2(.60f,0);value.rectTransform.offsetMax=new Vector2(-12,0);
            value.color=missing==0?new Color32(47,112,82,255):CandyUIStyle.Pink;
        }
    }
}
