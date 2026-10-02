using System;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Services;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed partial class PlayerDialogs
    {
        public void ItemDetails(ItemType type,Sprite icon,int count,Action use)
        {
            Open(ItemToolbar.NameKey(type));
            var art=Rect("ItemArtwork",content,Vector2.zero,Vector2.one);
            art.gameObject.AddComponent<LayoutElement>().preferredHeight=112;
            var image=art.gameObject.AddComponent<Image>();image.sprite=icon;image.preserveAspect=true;image.raycastTarget=false;
            Label(Localization.Get("itemOwned",count),20,32);
            Label(Localization.Get(ItemToolbar.NameKey(type)+"Help"),22,82);
            Label(Localization.Get("itemRules"),18,60).color=MutedInk;
            Label(Localization.Get("itemEarn"),18,90).color=MutedInk;
            FooterButton("cancel",Close);
            var action=FooterButton(count>0?(ItemRules.NeedsTarget(type)?"itemChoose":"itemUse"):"itemEmpty",()=>{Close();use();});
            action.interactable=count>0;CandyUIStyle.Button(action,CandyButtonRole.Primary,rounded);
        }
    }
}
