using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed partial class WorldMapView
    {
        private RectTransform worldFill,unlockDot;
        private TMP_Text nextReward,unlockNotice;
        private float unlockBegan=-10;
        private Vector2 unlockFrom,unlockTo;
        private void BuildProgress()
        {
            var heading=(RectTransform)title.transform.parent;
            heading.anchorMin=new Vector2(.023f,.73f);heading.anchorMax=new Vector2(.35f,.974f);heading.offsetMin=heading.offsetMax=Vector2.zero;
            Place(worldTag.rectTransform,new Rect(.08f,.82f,.84f,.15f));
            Place(title.rectTransform,new Rect(.07f,.59f,.87f,.24f));title.fontSizeMax=34;
            Place(summary.rectTransform,new Rect(.08f,.32f,.85f,.26f));summary.fontSize=summary.fontSizeMax=16;summary.fontSizeMin=14;
            var track=ProgressWidgets.Rect("WorldProgressTrack",heading);Place(track,new Rect(.08f,.245f,.84f,.035f));
            var sprite=heading.GetComponent<Image>().sprite;var bg=track.gameObject.AddComponent<Image>();bg.sprite=sprite;bg.type=Image.Type.Sliced;bg.color=new Color32(222,199,218,255);bg.raycastTarget=false;
            worldFill=ProgressWidgets.Rect("Fill",track);var fill=worldFill.gameObject.AddComponent<Image>();fill.sprite=sprite;fill.type=Image.Type.Sliced;fill.color=CandyUIStyle.Pink;fill.raycastTarget=false;
            nextReward=ProgressWidgets.Text("NextReward",heading,title.font,"",16);nextReward.alignment=TextAlignmentOptions.MidlineLeft;Place(nextReward.rectTransform,new Rect(.08f,.045f,.84f,.16f));
            unlockNotice=ProgressWidgets.Text("UnlockNotice",composition,title.font,"",22);Place(unlockNotice.rectTransform,new Rect(.38f,.87f,.35f,.08f));
            var noticeBg=unlockNotice.gameObject.AddComponent<Outline>();noticeBg.effectColor=Color.white;noticeBg.effectDistance=new Vector2(2,-2);
            unlockDot=ProgressWidgets.Rect("UnlockSpark",composition);unlockDot.anchorMin=unlockDot.anchorMax=Vector2.zero;unlockDot.sizeDelta=Vector2.one*26;
            var dot=unlockDot.gameObject.AddComponent<Image>();dot.sprite=halo.GetComponent<Image>().sprite;dot.color=new Color32(255,220,97,255);dot.raycastTarget=false;
            unlockDot.gameObject.SetActive(false);unlockNotice.gameObject.SetActive(false);
        }
        private void RefreshProgress(int complete)
        {
            int stars=0;for(int i=1;i<=10;i++)stars+=campaign.Progress.StarsAt((World-1)*10+i);
            summary.text=Localization.Get("worldProgress",complete,stars)+"\n"+Localization.Get("campaignStars",campaign.Progress.CompletedCount,campaign.Progress.TotalStars);
            worldFill.anchorMax=new Vector2(complete/10f,1);
            int reward=5;while(reward<=50 && campaign.Progress.itemRewardsClaimed[reward-1])reward+=5;
            nextReward.text=reward<=50?Localization.Get("nextReward",catalog.Get(reward).DisplayName):Localization.Get("allRewards");
        }
        private void StartUnlock()
        {
            int pending=campaign.PendingUnlock;
            if(pending<=0 || (pending-1)/10+1!=World)return;
            int index=(pending-1)%10;unlockTo=((RectTransform)stageButtons[index].transform).anchoredPosition;
            unlockFrom=index>0?((RectTransform)stageButtons[index-1].transform).anchoredPosition:unlockTo+new Vector2(0,-55);
            campaign.PendingUnlock=0;unlockBegan=Time.unscaledTime;
            unlockNotice.text=Localization.Get("unlockedPath");
        }
        private void AnimateUnlock()
        {
            if(!unlockNotice)return;float age=Time.unscaledTime-unlockBegan;
            unlockNotice.gameObject.SetActive(age<2.4f && !entering);
            unlockDot.gameObject.SetActive(age<1.2f && !entering && !GamePreferences.Current.reducedEffects);
            if(unlockDot.gameObject.activeSelf)unlockDot.anchoredPosition=Vector2.Lerp(unlockFrom,unlockTo,Mathf.SmoothStep(0,1,age/1.2f));
        }
        private static void Place(RectTransform rect,Rect area){rect.anchorMin=area.min;rect.anchorMax=area.max;rect.offsetMin=rect.offsetMax=Vector2.zero;}
    }
}
