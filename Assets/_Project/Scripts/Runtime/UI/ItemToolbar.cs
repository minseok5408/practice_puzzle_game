using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed class ItemToolbar : MonoBehaviour
    {
        private BoardController board;
        private LevelSession session;
        private Sprite[] sprites;
        private Sprite rounded;
        private TMP_FontAsset font;
        private readonly Button[] buttons=new Button[ItemRules.Count];
        private readonly TMP_Text[] counts=new TMP_Text[ItemRules.Count], names=new TMP_Text[ItemRules.Count];
        private RectTransform banner;
        private TMP_Text instruction;
        public static string NameKey(ItemType type)=>"item"+type;
        public void Configure(LevelSession level,BoardController controller,TMP_FontAsset bodyFont,Sprite panel,Sprite[] icons)
        {
            session=level;board=controller;font=bodyFont;rounded=panel;sprites=icons;
            var root=Rect("ItemBar",transform.Find("ScoreCard"),Vector2.zero,Vector2.one);
            for(int i=0;i<ItemRules.Count;i++)
            {
                var type=(ItemType)i;
                var slot=Rect(type.ToString(),root,new Vector2(i*.25f,0),new Vector2((i+1)*.25f,1));slot.offsetMin=new Vector2(3,0);slot.offsetMax=new Vector2(-3,0);
                var image=slot.gameObject.AddComponent<Image>();image.sprite=rounded;image.type=Image.Type.Sliced;
                buttons[i]=slot.gameObject.AddComponent<Button>();CandyUIStyle.Button(buttons[i],CandyButtonRole.Quiet,rounded);
                buttons[i].onClick.AddListener(()=>ShowDetails(type));
                var icon=Rect("Icon",slot,new Vector2(.04f,.24f),new Vector2(.96f,.98f)).gameObject.AddComponent<Image>();icon.sprite=sprites[i];icon.preserveAspect=true;icon.raycastTarget=false;
                names[i]=Text("Name",slot,new Vector2(.02f,0),new Vector2(.98f,.24f),16);names[i].fontSizeMin=14;
                var badge=Rect("Stock",slot,new Vector2(.70f,.72f),new Vector2(1.02f,1.02f));
                var bg=badge.gameObject.AddComponent<Image>();bg.sprite=rounded;bg.type=Image.Type.Sliced;bg.color=CandyUIStyle.Purple;bg.raycastTarget=false;
                counts[i]=Text("Count",badge,Vector2.zero,Vector2.one,17);counts[i].color=Color.white;counts[i].fontSizeMin=14;
            }
            banner=Rect("ItemTargetBanner",transform,new Vector2(.34f,.015f),new Vector2(.97f,.10f));
            var background=banner.gameObject.AddComponent<Image>();background.sprite=rounded;background.type=Image.Type.Sliced;background.color=CandyUIStyle.Cream;
            instruction=Text("Instruction",banner,new Vector2(.025f,.08f),new Vector2(.82f,.92f),22);
            CandyUIStyle.CloseButton(banner,rounded,()=>board.CancelItem());
            var close=(RectTransform)banner.Find("CloseDialog");close.anchorMin=close.anchorMax=new Vector2(1,.5f);close.pivot=new Vector2(1,.5f);close.anchoredPosition=new Vector2(-12,0);
            board.ItemSelectionChanged+=Refresh;session.Changed+=Refresh;GamePreferences.Changed+=Refresh;
            board.ItemUseFailed+=ShowFailure;
            Refresh();
        }
        public void ShowDetails(ItemType type)
        {
            if(!session || !board || board.IsBusy || board.IsPaused || session.Progress.IsFinished)return;
            if(board.SelectedItem==type){board.CancelItem();return;}
            board.CancelItem();
            GetComponent<PlayerDialogs>().ItemDetails(type,sprites[(int)type],session.ItemCount(type),()=>
            {
                if(ItemRules.NeedsTarget(type))board.ArmItem(type);
                else if(!board.TryUseItem(type) && !GetComponent<PlayerDialogs>().IsVisible)ShowFailure();
            });
        }
        private void ShowFailure()
        {board.CancelItem();GetComponent<PlayerDialogs>().Message("items",Localization.Get(board.ItemFailureKey));}
        private void Refresh()
        {
            if(!board || !session)return;
            for(int i=0;i<ItemRules.Count;i++)
            {
                counts[i].text=session.ItemCount((ItemType)i).ToString();names[i].text=Localization.Get(NameKey((ItemType)i)+"Short");
                buttons[i].GetComponent<Image>().color=board.SelectedItem==(ItemType)i?new Color32(255,216,143,255):CandyUIStyle.Quiet;
            }
            banner.gameObject.SetActive(board.SelectedItem.HasValue);
            if(board.SelectedItem.HasValue)instruction.text=Localization.Get("itemTarget",Localization.Get(NameKey(board.SelectedItem.Value)));
        }
        private void Update()
        {
            if(!board)return;
            foreach(var button in buttons)if(button)button.interactable=!board.IsBusy && !board.IsPaused && session.CanPlay;
            bool wide=Screen.width/(float)Screen.height>=1.25f;
            banner.anchorMin=new Vector2(wide?.34f:.045f,.015f);banner.anchorMax=new Vector2(.97f,.10f);
        }
        private void OnDestroy()
        {
            if(board)board.ItemSelectionChanged-=Refresh;
            if(board)board.ItemUseFailed-=ShowFailure;
            if(session)session.Changed-=Refresh;
            GamePreferences.Changed-=Refresh;
        }
        private TMP_Text Text(string name,Transform parent,Vector2 min,Vector2 max,float size)
        {
            var text=Rect(name,parent,min,max).gameObject.AddComponent<TextMeshProUGUI>();text.font=font;text.fontSize=text.fontSizeMax=size;text.fontSizeMin=16;
            text.enableAutoSizing=true;text.color=CandyUIStyle.Ink;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;return text;
        }
        private static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max)
        {var rect=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;rect.SetParent(parent,false);rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;return rect;}
    }
}
