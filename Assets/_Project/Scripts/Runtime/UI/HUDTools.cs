using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed class HUDTools : MonoBehaviour
    {
        private LevelSession session;
        private BoardController board;
        private TMP_Text stage, challenge;
        private RectTransform row;
        private Button hint, help;
        public void Configure(LevelSession level,TMP_FontAsset font,Sprite rounded)
        {
            session=level;board=FindFirstObjectByType<BoardController>();var panel=transform.Find("ScoreCard");
            stage=ProgressWidgets.Text("StageName",panel,font,"",24);
            challenge=ProgressWidgets.Text("StageChallenge",panel,font,"",19);
            row=ProgressWidgets.Rect("ToolRow",panel);
            hint=MakeButton("ManualHint",row,font,rounded,()=>{board.GetComponent<BoardInput>().CancelGesture();board.GetComponent<BoardGuidance>().RequestHint();},0);
            help=MakeButton("QuickHelp",row,font,rounded,()=>GetComponent<PlayerDialogs>().Guide(),1);
            session.Changed+=Refresh;GamePreferences.Changed+=Refresh;Refresh();
        }
        private Button MakeButton(string name,Transform parent,TMP_FontAsset font,Sprite rounded,UnityEngine.Events.UnityAction click,int index)
        {
            var root=ProgressWidgets.Rect(name,parent);root.anchorMin=new Vector2(index*.5f,0);root.anchorMax=new Vector2((index+1)*.5f,1);root.offsetMin=new Vector2(3,0);root.offsetMax=new Vector2(-3,0);
            root.gameObject.AddComponent<Image>().sprite=rounded;var button=root.gameObject.AddComponent<Button>();button.onClick.AddListener(click);
            ProgressWidgets.Text("Label",root,font,"",19);CandyUIStyle.Button(button,CandyButtonRole.Quiet,rounded,19);return button;
        }
        public void Layout(bool wide)
        {
            Place(stage.rectTransform,wide?new Rect(.065f,.80f,.58f,.06f):new Rect(.30f,.70f,.40f,.10f));
            Place(row,wide?new Rect(.065f,.215f,.87f,.062f):new Rect(.025f,.135f,.19f,.16f));
            Place(challenge.rectTransform,wide?new Rect(.085f,.30f,.83f,.15f):new Rect(.28f,.255f,.68f,.16f));
            for(int i=0;i<2;i++)
            {
                var rect=(RectTransform)(i==0?hint:help).transform;
                rect.anchorMin=wide?new Vector2(i*.5f,0):new Vector2(0,i==0?.53f:0);
                rect.anchorMax=wide?new Vector2((i+1)*.5f,1):new Vector2(1,i==0?1:.47f);
                rect.offsetMin=new Vector2(3,0);rect.offsetMax=new Vector2(-3,0);
            }
        }
        private void Refresh()
        {
            if(session.Progress==null)return;
            stage.text=Localization.Get("stageTitle",session.DisplayName);
            hint.GetComponentInChildren<TMP_Text>().text=Localization.Get("manualHint");
            help.GetComponentInChildren<TMP_Text>().text=Localization.Get("quickHelp");
            var goals=GetComponentInChildren<GoalPanel>(true);
            challenge.gameObject.SetActive(!goals || goals.GoalCount==0);
            challenge.text=ProgressWidgets.Stars(session.Campaign?.StarsAt(session.Definition.Number)??0)+"\n"+Localization.Get("ratingRules",StageRating.ReserveTarget(session.Progress.Rules.StartingMoves));
        }
        private void Update()
        {
            if(!board)return;
            hint.interactable=!board.IsPaused && !board.IsBusy && session.CanPlay && !board.SelectedItem.HasValue;
            help.interactable=!board.IsPaused;
        }
        private static void Place(RectTransform rect,Rect area){rect.anchorMin=area.min;rect.anchorMax=area.max;rect.offsetMin=rect.offsetMax=Vector2.zero;}
        private void OnDestroy(){if(session)session.Changed-=Refresh;GamePreferences.Changed-=Refresh;}
    }
}
