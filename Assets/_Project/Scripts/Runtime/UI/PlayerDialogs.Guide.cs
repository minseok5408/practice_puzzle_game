using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed partial class PlayerDialogs
    {
        [SerializeField] private Sprite[] guideItems;
        public void ConfigureGuide(Sprite[] icons)=>guideItems=icons;
        public void Guide(int page=0)
        {
            page=Mathf.Clamp(page,0,3);
            Open("help");minimumCardHeight=620;
            var tabs=Horizontal("GuideTabs",content,48);
            string[] keys={"guideBasic","guideSpecial","guideFrost","guideItems"};
            Button selected=null;
            for(int i=0;i<4;i++)
            {
                int target=i;var tab=CreateButton(tabs,keys[i],()=>Guide(target));
                tab.GetComponent<LayoutElement>().flexibleWidth=1;
                CandyUIStyle.Button(tab,i==page?CandyButtonRole.Secondary:CandyButtonRole.Quiet,rounded,20);
                if(i==page)selected=tab;
            }
            if(page==0)
            {
                GuideRow("Swap",Candy(PieceColor.Red),"guideSwap",124);
                GuideRow("Goals",Candy(PieceColor.Green),"guideGoals",124);
                var legend=Horizontal("ColorLegend",content,90);
                for(int i=1;i<=6;i++)
                {
                    var slot=Rect("Color"+i,legend,Vector2.zero,Vector2.one);
                    slot.gameObject.AddComponent<LayoutElement>().preferredHeight=90;
                    var icon=Rect("Icon",slot,new Vector2(.1f,.35f),new Vector2(.9f,1));
                    var image=icon.gameObject.AddComponent<Image>();image.sprite=Candy((PieceColor)i);image.preserveAspect=true;image.raycastTarget=false;
                    var label=TextAt(slot,"Number",i.ToString(),22,TextAlignmentOptions.Center,0,0,30,60);
                }
                Label(Localization.Get("colorLabelsHint"),18,56);
            }
            else if(page==1)
            {
                GuideRow("Striped",Candy(PieceColor.Red,SpecialPieceType.Row),"guideStriped",120);
                GuideRow("Wrapped",Candy(PieceColor.Orange,SpecialPieceType.Bomb),"guideWrapped",120);
                GuideRow("Rainbow",Candy(PieceColor.None,SpecialPieceType.ColorClear),"guideRainbow",140);
            }
            else if(page==2)
            {
                GuideRow("Frost",frostIcon,"guideIceHit",150);
                GuideRow("FrostGoal",frostIcon,"guideIceGoal",150);
                Label(Localization.Get("guideIceShuffle"),21,84);
            }
            else
            {
                for(int i=0;i<ItemRules.Count;i++)
                {
                    var type=(ItemType)i;
                    GuideRow(type.ToString(),guideItems!=null && i<guideItems.Length?guideItems[i]:null,ItemToolbar.NameKey(type)+"Help",110);
                }
                Label(Localization.Get("itemRules"),19,68);
                Label(Localization.Get("itemEarn"),19,100);
            }
            FooterButton("back",()=>PreferencesPage(2));
            if(EventSystem.current && selected)EventSystem.current.SetSelectedGameObject(selected.gameObject);
        }
        private Sprite Candy(PieceColor color,SpecialPieceType special=SpecialPieceType.None)
            =>pieces?pieces.Get(new PieceState(1,color,special)).sprite:null;
        private void GuideRow(string name,Sprite sprite,string key,float height)
        {
            var root=Panel(name,content,height);
            var art=Rect("Artwork",root,new Vector2(0,.5f),new Vector2(0,.5f));
            art.anchoredPosition=new Vector2(53,0);art.sizeDelta=new Vector2(74,74);
            var image=art.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
            var text=TextAt(root,"Explanation",Localization.Get(key),22,TextAlignmentOptions.MidlineLeft,106,16,height-24,12);
            text.fontSizeMin=19;text.textWrappingMode=TextWrappingModes.Normal;
        }
    }
}
