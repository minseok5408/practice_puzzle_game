using System;
using System.Globalization;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed partial class PlayerDialogs
    {
        private void ChoiceRow(string name,string title,float[] options,float current,Action<float> set,string suffix)
        {
            var root=Panel(name,content,100);
            TextAt(root,"Label",title,21,TextAlignmentOptions.MidlineLeft,16,16,36,4);
            var choices=Rect("Choices",root,Vector2.zero,Vector2.one);choices.offsetMin=new Vector2(16,10);choices.offsetMax=new Vector2(-16,-44);
            for(int i=0;i<options.Length;i++)
            {
                float value=options[i];
                var button=CreateButton(choices,null,()=>{set(value);GamePreferences.Save();PreferencesPage(1);},value.ToString("0.#",CultureInfo.InvariantCulture)+suffix);
                button.name=value.ToString("0.#",CultureInfo.InvariantCulture);PlaceChoice(button,i,options.Length);
                CandyUIStyle.Button(button,Mathf.Approximately(value,current)?CandyButtonRole.Primary:CandyButtonRole.Quiet,rounded,20);
            }
        }
        private static void PlaceChoice(Button button,int index,int count)
        {
            // Keep choices proportional as the card resizes, without a nested layout group's cached width.
            var rect=(RectTransform)button.transform;
            rect.anchorMin=new Vector2((float)index/count,0);rect.anchorMax=new Vector2((float)(index+1)/count,1);
            rect.offsetMin=new Vector2(index==0?0:CandyUIStyle.Gap/2,0);
            rect.offsetMax=new Vector2(index==count-1?0:-CandyUIStyle.Gap/2,0);
        }
        private void DisplayPage()
        {
            var settings=GetComponent<SettingsPopup>();settings.RefreshDisplayChoices();
            Label(Localization.Get("displayHint"),21,40);
            var root=Panel("Display",content,210);
            TextAt(root,"ResolutionLabel",Localization.Get("resolution"),22,TextAlignmentOptions.MidlineLeft,16,16,38,8);
            var dropdown=Instantiate(settings.ResolutionTemplate,root,false);dropdown.name="Resolution";
            dropdown.gameObject.SetActive(true);DisplayResolution=dropdown;
            var rect=(RectTransform)dropdown.transform;rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;
            rect.offsetMin=new Vector2(16,-104);rect.offsetMax=new Vector2(-16,-48);rect.localScale=Vector3.one;
            dropdown.onValueChanged=new TMP_Dropdown.DropdownEvent();
            TextAt(root,"WindowLabel",Localization.Get("windowed"),22,TextAlignmentOptions.MidlineLeft,16,150,42,132);
            DisplayWindowed=Switch(root,"Windowed",Screen.fullScreenMode==FullScreenMode.Windowed,_=>{});
            PlaceTop((RectTransform)DisplayWindowed.transform,132,16,132,40);
            DisplayApply=Button("apply",settings.Apply);CandyUIStyle.Button(DisplayApply,CandyButtonRole.Primary,rounded);
            displayStatus=Label(settings.StatusText,19,40);
        }
    }
}
