using System.Collections.Generic;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    // Collection and ice share a readable counter, progress track and completion mark.
    public sealed class GoalPanel : MonoBehaviour
    {
        private sealed class Goal
        {
            public PieceColor Color;
            public int Target;
            public RectTransform Root, Fill;
            public TMP_Text Count;
            public Image Background;
            public GameObject Check, ColorLabel;
            public bool Complete;
            public float CompletedAt=-1;
        }
        private readonly List<Goal> goals=new List<Goal>();
        private LevelProgress progress;
        private TMP_FontAsset font;
        private Sprite rounded, checkSprite, iceSprite;
        private Sprite[] candies;
        public int GoalCount=>goals.Count;
        public int CompletedGoalCount { get; private set; }
        private void OnEnable(){GamePreferences.Changed+=RefreshColorLabels;RefreshColorLabels();}
        private void OnDisable()=>GamePreferences.Changed-=RefreshColorLabels;
        private void RefreshColorLabels(){foreach(var goal in goals)if(goal.ColorLabel)goal.ColorLabel.SetActive(GamePreferences.Current.colorLabels);}

        public void Configure(TMP_FontAsset body,Sprite panel,Sprite[] candyArtwork,Sprite check,Sprite ice)
        {font=body;rounded=panel;candies=candyArtwork;checkSprite=check;iceSprite=ice;}

        public void Refresh(LevelProgress value)
        {
            if(progress!=value)
            {
                progress=value;
                foreach(var goal in goals){goal.Root.gameObject.SetActive(false);Destroy(goal.Root.gameObject);}goals.Clear();
                for(int i=1;i<=6;i++)if(value.Rules.CollectionTarget((PieceColor)i)>0)
                    AddGoal((PieceColor)i,value.Rules.CollectionTarget((PieceColor)i));
                if(value.Rules.FrostTarget>0)AddGoal(PieceColor.None,value.Rules.FrostTarget);
            }
            CompletedGoalCount=0;
            foreach(var goal in goals)
            {
                int count=Mathf.Min(goal.Target,goal.Color==PieceColor.None?value.FrostCleared:value.Collected(goal.Color));
                bool complete=count>=goal.Target;
                if(complete && !goal.Complete)goal.CompletedAt=Time.unscaledTime;
                if(!complete)goal.CompletedAt=-1;
                goal.Complete=complete;
                goal.Count.text=complete?Localization.Get("goalDone"):count+" / "+goal.Target;
                goal.Count.color=complete?new Color32(47,112,82,255):CandyUIStyle.Ink;
                goal.Fill.anchorMax=new Vector2((float)count/goal.Target,1);
                goal.Fill.GetComponent<Image>().color=complete?new Color32(86,167,119,255):CandyUIStyle.Pink;
                goal.Background.color=complete?new Color32(221,243,222,255):new Color32(243,226,239,255);
                goal.Check.SetActive(complete);if(complete)CompletedGoalCount++;
            }
            gameObject.SetActive(goals.Count>0);
        }

        private void AddGoal(PieceColor color,int target)
        {
            var root=Rect(color==PieceColor.None?"Frost":color.ToString(),transform,Vector2.zero,Vector2.one);
            var background=Paint(root,rounded,Color.white);
            var icon=Rect("Icon",root,new Vector2(0,.5f),new Vector2(0,.5f));icon.anchoredPosition=new Vector2(24,3);icon.sizeDelta=new Vector2(34,34);
            if(color!=PieceColor.None){var image=Paint(icon,candies[(int)color-1],Color.white);image.type=Image.Type.Simple;image.preserveAspect=true;}
            else IceIcon(icon,iceSprite);
            var countRect=Rect("Count",root,Vector2.zero,Vector2.one);countRect.offsetMin=new Vector2(44,12);countRect.offsetMax=new Vector2(-8,-5);
            var count=countRect.gameObject.AddComponent<TextMeshProUGUI>();count.font=font;count.fontSize=21;count.fontSizeMin=15;count.fontSizeMax=21;count.enableAutoSizing=true;count.alignment=TextAlignmentOptions.Center;count.raycastTarget=false;count.textWrappingMode=TextWrappingModes.NoWrap;
            var track=Rect("Track",root,Vector2.zero,Vector2.right);track.offsetMin=new Vector2(10,7);track.offsetMax=new Vector2(-10,11);Paint(track,rounded,new Color32(216,192,216,255));
            var fill=Rect("Fill",track,Vector2.zero,Vector2.one);Paint(fill,rounded,CandyUIStyle.Pink);
            var check=Rect("Complete",root,Vector2.one,Vector2.one);check.anchoredPosition=new Vector2(-11,-9);check.sizeDelta=new Vector2(13,13);Paint(check,checkSprite,new Color32(39,117,71,255));
            GameObject colorLabel=null;
            if(color!=PieceColor.None)
            {
                var badge=Rect("ColorLabel",root,new Vector2(0,.5f),new Vector2(0,.5f));badge.anchoredPosition=new Vector2(36,5);badge.sizeDelta=new Vector2(16,16);
                Paint(badge,rounded,new Color32(70,35,83,255));
                var number=Rect("Number",badge,Vector2.zero,Vector2.one).gameObject.AddComponent<TextMeshProUGUI>();
                number.font=font;number.text=((int)color).ToString();number.fontSize=14;number.color=Color.white;number.alignment=TextAlignmentOptions.Center;number.raycastTarget=false;
                colorLabel=badge.gameObject;colorLabel.SetActive(GamePreferences.Current.colorLabels);
            }
            goals.Add(new Goal{Color=color,Target=target,Root=root,Count=count,Fill=fill,Background=background,Check=check.gameObject,ColorLabel=colorLabel});
        }

        private void LateUpdate()
        {
            var rect=(RectTransform)transform;
            bool wide=Camera.main && Camera.main.aspect>=1.25f;
            int columns=Mathf.Min(goals.Count,wide?2:4);
            if(columns==0)return;
            int rows=Mathf.CeilToInt((float)goals.Count/columns);
            const float gap=8;
            float width=(rect.rect.width-gap*(columns-1))/columns;
            float height=Mathf.Min(wide?62:56,(rect.rect.height-gap*(rows-1))/rows);
            float top=(rect.rect.height-(rows*height+(rows-1)*gap))*.5f;
            var ordered=new List<Goal>(goals);ordered.Sort((a,b)=>a.Complete.CompareTo(b.Complete));
            for(int i=0;i<goals.Count;i++)
            {
                var goal=ordered[i];var slot=goal.Root;slot.anchorMin=slot.anchorMax=new Vector2(0,1);slot.pivot=new Vector2(0,1);
                slot.anchoredPosition=new Vector2(i%columns*(width+gap),-top-i/columns*(height+gap));slot.sizeDelta=new Vector2(width,height);
                goal.Fill.parent.gameObject.SetActive(!goal.Complete);
                float age=Time.unscaledTime-goal.CompletedAt;
                bool pulse=goal.CompletedAt>=0 && age<.35f && !GamePreferences.Current.reducedEffects;
                slot.localScale=Vector3.one*(pulse?1+.04f*Mathf.Sin(age/.35f*Mathf.PI):1);
            }
        }

        internal static void IceIcon(RectTransform root,Sprite sprite)
        {
            var image=Paint(root,sprite,Color.white);image.type=Image.Type.Simple;image.preserveAspect=true;
        }
        private static Image Paint(RectTransform rect,Sprite sprite,Color color)
        {var image=rect.gameObject.AddComponent<Image>();image.sprite=sprite;image.type=Image.Type.Sliced;image.color=color;image.raycastTarget=false;return image;}
        private static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max)
        {var rect=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;rect.SetParent(parent,false);rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;return rect;}
    }
}
