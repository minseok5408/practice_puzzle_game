using System;
using System.Globalization;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Config;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    public sealed partial class PlayerDialogs
    {
        [SerializeField] private PieceCatalog pieces;
        [SerializeField] private Sprite frostIcon;
        private TMP_Text preferenceStatus;
        private static readonly Color SoftPanel=new Color32(247,235,243,255);
        private static readonly Color MutedInk=new Color32(139,106,146,255);

        public void ConfigureArtwork(PieceCatalog catalog) => pieces=catalog;
        public void ConfigureFrostIcon(Sprite icon)=>frostIcon=icon;

        public void Preferences() => PreferencesPage(0);
        public void UnifiedSettings() => PreferencesPage(3);

        private void PreferencesPage(int page)
        {
            if(GetComponent<SettingsPopup>()?.IsApplying==true)return;
            Open("settings");IsSettings=true;minimumCardHeight=594;preferenceStatus=null;
            var tabs=Horizontal("Tabs",content,48);
            string[] keys={"display","audioTab","experienceTab","otherTab"};
            int[] pages={3,0,1,2};
            Button selected=null;
            for(int i=0;i<keys.Length;i++)
            {
                int target=pages[i];
                var tab=CreateButton(tabs,keys[i],()=>PreferencesPage(target));
                tab.GetComponent<LayoutElement>().flexibleWidth=1;
                CandyUIStyle.Button(tab,page==target?CandyButtonRole.Secondary:CandyButtonRole.Quiet,rounded,20);
                if(page==target)selected=tab;
            }
            var p=GamePreferences.Current;
            if(page==0)
            {
                AudioCard("music",p.musicVolume,p.musicMuted,v=>p.musicVolume=v,v=>p.musicMuted=v);
                AudioCard("effects",p.effectsVolume,p.effectsMuted,v=>p.effectsVolume=v,v=>p.effectsMuted=v);
                Button("testSound",()=>GameAudio.Play("special"));
            }
            else if(page==1)
            {
                var automatic=Panel("AutoHints",content,72);
                TextAt(automatic,"Label",Localization.Get("autoHints"),22,TextAlignmentOptions.MidlineLeft,16,150,60,6);
                var autoSwitch=Switch(automatic,"Switch",p.autoHints,v=>{p.autoHints=v;SavePreference();});
                PlaceTop((RectTransform)autoSwitch.transform,132,16,16,40);
                ChoiceRow("HintDelay",Localization.Get("hintDelay",p.hintDelay),new[]{3f,6f,10f,15f},p.hintDelay,v=>p.hintDelay=v,"s");
                Label(Localization.Get("hintAdvice"),18,64).color=MutedInk;
                ChoiceRow("AnimationSpeed",Localization.Get("animationSpeed",p.animationSpeed.ToString("0.#",CultureInfo.InvariantCulture)),new[]{1f,1.5f,2f},p.animationSpeed,v=>p.animationSpeed=v,"x");
                var language=Panel("Language",content,132);
                TextAt(language,"Label",Localization.Get("language"),22,TextAlignmentOptions.MidlineLeft,16,16,42);
                var choices=Rect("Choices",language,Vector2.zero,Vector2.right);
                choices.offsetMin=new Vector2(16,16);choices.offsetMax=new Vector2(-16,64);
                foreach(string code in new[]{"ko","en"})
                {
                    string value=code;
                    var choice=CreateButton(choices,null,()=>{p.language=value;GamePreferences.Save();PreferencesPage(1);},code=="ko"?"한국어":"English");
                    choice.name=code;PlaceChoice(choice,code=="ko"?0:1,2);
                    CandyUIStyle.Button(choice,p.language==code?CandyButtonRole.Primary:CandyButtonRole.Quiet,rounded);
                }
                var effects=Panel("ReducedEffects",content,148);
                TextAt(effects,"Label",Localization.Get("reduce"),22,TextAlignmentOptions.MidlineLeft,16,150,48);
                var switcher=Switch(effects,"Switch",p.reducedEffects,v=>{p.reducedEffects=v;SavePreference();});
                PlaceTop((RectTransform)switcher.transform,132,16,12,40);
                var hint=TextAt(effects,"Hint",Localization.Get("reduceHint"),19,TextAlignmentOptions.TopLeft,16,16,76,60);
                hint.color=MutedInk;hint.textWrappingMode=TextWrappingModes.Normal;
                var colors=Panel("ColorLabels",content,126);
                TextAt(colors,"Label",Localization.Get("colorLabels"),22,TextAlignmentOptions.MidlineLeft,16,150,42);
                var colorSwitch=Switch(colors,"Switch",p.colorLabels,v=>{p.colorLabels=v;SavePreference();});
                PlaceTop((RectTransform)colorSwitch.transform,132,16,8,40);
                var colorHint=TextAt(colors,"Hint",Localization.Get("colorLabelsHint"),19,TextAlignmentOptions.TopLeft,16,16,66,52);
                colorHint.color=MutedInk;colorHint.textWrappingMode=TextWrappingModes.Normal;
            }
            else if(page==2)
            {
                Label(Localization.Get("controls"),21,104);
                Button("help",()=>Guide(0));
                Button("credits",()=>PreferenceMessage("credits",Localization.Get("creditsBody")));
                if(!board)
                {
                    var hint=Label(Localization.Get("resetHint"),19,60);hint.color=MutedInk;
                    Button("reset",()=>Confirm("reset",Localization.Get("resetConfirm"),()=>{
                        bool ok=CampaignState.Instance && CampaignState.Instance.ResetProgress();
                        FindFirstObjectByType<WorldMapView>()?.ShowWorld(1);
                        PreferenceMessage("reset",Localization.Get(ok?"resetDone":"resetFailed"));
                    }));
                }
            }
            else DisplayPage();
            preferenceStatus=Label("",18,42);RefreshPreferenceStatus();
            SettingsCloseButton=FooterButton("close",Close);
            if(page==3)FooterButton("quit",()=>GetComponent<SettingsPopup>().Quit());
            if(EventSystem.current && selected)EventSystem.current.SetSelectedGameObject(selected.gameObject);
        }

        private void PreferenceMessage(string title,string body)
        { Open(title);minimumCardHeight=594;Label(body,22);FooterButton("back",()=>PreferencesPage(2)); }

        private void SavePreference() { GamePreferences.Save();RefreshPreferenceStatus(); }
        private void RefreshPreferenceStatus()
        {
            if(!preferenceStatus)return;
            preferenceStatus.text=Localization.Get(GamePreferences.SaveFailed?"saveFailed":"autoSaved");
            preferenceStatus.color=GamePreferences.SaveFailed?CandyUIStyle.Danger:MutedInk;
        }

        private void AudioCard(string key,float value,bool muted,Action<float> volumeChanged,Action<bool> muteChanged)
        {
            var root=Panel(key,content,148);
            TextAt(root,"Label",Localization.Get(key),24,TextAlignmentOptions.MidlineLeft,16,120,42,8);
            var caption=TextAt(root,"Value",Percent(value),22,TextAlignmentOptions.MidlineRight,16,16,42,8);
            var sliderRoot=Rect("Volume",root,Vector2.zero,Vector2.one);
            sliderRoot.anchorMin=new Vector2(0,1);sliderRoot.anchorMax=Vector2.one;
            sliderRoot.offsetMin=new Vector2(16,-94);sliderRoot.offsetMax=new Vector2(-16,-46);
            sliderRoot.gameObject.AddComponent<Image>().color=Color.clear;
            var group=sliderRoot.gameObject.AddComponent<CanvasGroup>();
            var track=Rect("Track",sliderRoot,new Vector2(0,.5f),new Vector2(1,.5f));
            track.offsetMin=new Vector2(14,-5);track.offsetMax=new Vector2(-14,5);Paint(track,new Color32(223,201,222,255));
            var fill=Rect("Fill",track,Vector2.zero,Vector2.one);Paint(fill,CandyUIStyle.Pink);
            // Slider stretches its handle anchors vertically; a zero-height rail keeps the knob round.
            var handleArea=Rect("HandleArea",sliderRoot,new Vector2(0,.5f),new Vector2(1,.5f));handleArea.offsetMin=new Vector2(14,0);handleArea.offsetMax=new Vector2(-14,0);
            var knob=Rect("Handle",handleArea,new Vector2(0,.5f),new Vector2(0,.5f));knob.sizeDelta=new Vector2(28,28);
            var knobImage=Paint(knob,CandyUIStyle.Purple);
            var slider=sliderRoot.gameObject.AddComponent<Slider>();slider.fillRect=fill;slider.handleRect=knob;slider.targetGraphic=knobImage;
            slider.minValue=0;slider.maxValue=100;slider.wholeNumbers=true;slider.SetValueWithoutNotify(Mathf.RoundToInt(value*100));
            var colors=slider.colors;colors.highlightedColor=new Color(1.18f,1.08f,1.18f);colors.selectedColor=new Color32(255,221,143,255);slider.colors=colors;
            slider.onValueChanged.AddListener(v=>{volumeChanged(v/100f);caption.text=Percent(v/100f);SavePreference();});
            TextAt(root,"MuteLabel",Localization.Get("mute"),19,TextAlignmentOptions.MidlineLeft,16,150,40,100);
            void SetMuted(bool isMuted) { slider.interactable=!isMuted;group.alpha=isMuted?.38f:1; }
            var mute=Switch(root,"Mute",muted,v=>{muteChanged(v);SetMuted(v);SavePreference();});
            var muteRect=(RectTransform)mute.transform;muteRect.anchorMin=muteRect.anchorMax=Vector2.one;muteRect.pivot=Vector2.one;
            muteRect.sizeDelta=new Vector2(116,40);muteRect.anchoredPosition=new Vector2(-16,-100);
            SetMuted(muted);
        }

        private Toggle Switch(Transform parent,string name,bool value,Action<bool> changed)
        {
            var root=Rect(name,parent,Vector2.zero,Vector2.one);root.gameObject.AddComponent<Image>().color=Color.clear;
            var state=TextAt(root,"State","",18,TextAlignmentOptions.MidlineLeft,0,68,40,0);
            var track=Rect("Track",root,new Vector2(1,.5f),new Vector2(1,.5f));track.pivot=new Vector2(1,.5f);track.sizeDelta=new Vector2(64,32);
            var trackImage=Paint(track,CandyUIStyle.Purple);
            var knob=Rect("Knob",track,new Vector2(.5f,.5f),new Vector2(.5f,.5f));knob.sizeDelta=new Vector2(26,26);
            var knobImage=Paint(knob,Color.white);
            var toggle=root.gameObject.AddComponent<Toggle>();toggle.targetGraphic=knobImage;toggle.SetIsOnWithoutNotify(value);
            var colors=toggle.colors;colors.selectedColor=new Color32(255,212,111,255);colors.highlightedColor=new Color32(255,238,196,255);toggle.colors=colors;
            void Refresh(bool on) { trackImage.color=on?CandyUIStyle.Pink:new Color32(191,173,196,255);knob.anchoredPosition=new Vector2(on?16:-16,0);state.text=Localization.Get(on?"on":"off"); }
            Refresh(value);toggle.onValueChanged.AddListener(on=>{Refresh(on);changed(on);});return toggle;
        }

        public void Preview(LevelDefinition level,CampaignProgress progress,Action play)
        {
            Open("target");heading.text=Localization.Get("stageTitle",level.DisplayName);
            var world=Label(Localization.World(level.World),20,28);world.color=MutedInk;
            bool unlocked=progress.IsUnlocked(level.Number);
            if(!unlocked)Label(Localization.Get("lockedStage",((level.Number-2)/10+1)+"-"+((level.Number-2)%10+1)),21,40);
            var rules=level.CreateRules();var overview=Horizontal("Overview",content,86);
            Stat(overview,"Moves",Localization.Get("moveLimit"),rules.StartingMoves.ToString(CultureInfo.InvariantCulture),CandyUIStyle.Pink);
            int best=progress.bestScores[level.Number-1];
            Stat(overview,"Best",Localization.Get("bestScore"),best>0?best.ToString("N0",CultureInfo.InvariantCulture):Localization.Get("noRecord"),CandyUIStyle.Purple);
            bool claimed=progress.itemRewardsClaimed[level.Number-1];
            Label(Localization.Get(claimed?"rewardClaimed":"rewardPreview"),21,28);
            var rewards=Panel("RewardPreview",content,68);
            var amounts=new int[4];for(int i=0;i<4;i++)if(level.Number%5==0 || i==(level.Number-1)%4)amounts[i]=1;
            ProgressWidgets.Rewards(rewards,font,guideItems,amounts);
            if(claimed)rewards.gameObject.AddComponent<CanvasGroup>().alpha=.45f;
            var subtitle=Label(Localization.Get("allGoals"),20,28);subtitle.alignment=TextAlignmentOptions.MidlineLeft;
            Goal("Score",Localization.Get("targetScore"),Localization.Get("scoreAmount",rules.TargetScore),null,"★");
            string[] colors={"red","orange","yellow","green","blue","purple"};
            for(int i=0;i<colors.Length;i++)
            {
                int target=rules.CollectionTarget((PieceColor)(i+1));
                if(target>0)Goal(colors[i],Localization.Get("candyName",Localization.Get(colors[i])),Localization.Get("candyAmount",target),pieces?pieces.Get((PieceColor)(i+1)).sprite:null,null);
            }
            if(rules.FrostTarget>0)Goal("Frost",Localization.Get("clearFrost"),Localization.Get("tileAmount",rules.FrostTarget),null,null,true);
            Label(ProgressWidgets.Stars(progress.StarsAt(level.Number))+"  "+Localization.Get("rating",progress.StarsAt(level.Number)),24,40);
            Label(Localization.Get("ratingRules",PuzzleGame.Core.Levels.StageRating.ReserveTarget(rules.StartingMoves)),19,62).color=MutedInk;
            string Medal(string key,bool earned)=>Localization.Get(key)+" / "+Localization.Get(earned?"medalEarned":"medalOpen");
            Label(Medal("cleanMedal",progress.cleanMedals[level.Number-1])+"\n"+Medal("efficientMedal",progress.efficientMedals[level.Number-1]),19,62);
            Label(Localization.Get("rewardCap"),17,30).color=MutedInk;
            FooterButton("cancel",Close);
            var start=FooterButton(unlocked?"start":"stageLocked",()=>{if(progress.IsUnlocked(level.Number)){Close();play();}});
            start.interactable=unlocked;
            if(EventSystem.current)EventSystem.current.SetSelectedGameObject(start.gameObject);
        }

        private void Stat(Transform parent,string name,string title,string value,Color accent)
        {
            var root=Panel(name,parent,86);root.GetComponent<LayoutElement>().flexibleWidth=1;
            TextAt(root,"Label",title,18,TextAlignmentOptions.MidlineLeft,16,16,28,8).color=MutedInk;
            TextAt(root,"Value",value,28,TextAlignmentOptions.MidlineLeft,16,16,40,36).color=accent;
        }

        private void Goal(string name,string label,string amount,Sprite sprite,string glyph,bool frost=false)
        {
            var root=Panel("Goal_"+name,content,52);
            var icon=Rect("Icon",root,new Vector2(0,.5f),new Vector2(0,.5f));icon.anchoredPosition=new Vector2(34,0);icon.sizeDelta=new Vector2(38,38);
            if(sprite) { var image=icon.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false; }
            else if(frost)
            {
                GoalPanel.IceIcon(icon,frostIcon);
            }
            else if(glyph!=null)
            {
                var text=TextAt(icon,"Symbol",glyph,34,TextAlignmentOptions.Center,0,0,38,0);text.color=new Color32(236,170,51,255);
            }
            TextAt(root,"Label",label,21,TextAlignmentOptions.MidlineLeft,64,155,52,0);
            TextAt(root,"Amount",amount,22,TextAlignmentOptions.MidlineRight,64,16,52,0).color=CandyUIStyle.Pink;
        }

        private RectTransform Panel(string name,Transform parent,float height)
        {
            var root=Rect(name,parent,Vector2.zero,Vector2.one);Paint(root,SoftPanel);
            var layout=root.gameObject.AddComponent<LayoutElement>();layout.minHeight=layout.preferredHeight=height;return root;
        }
        private Image Paint(RectTransform rect,Color color)
        { var image=rect.gameObject.AddComponent<Image>();image.sprite=rounded;image.type=Image.Type.Sliced;image.color=color;image.raycastTarget=false;return image; }
        private static RectTransform Horizontal(string name,Transform parent,float height)
        {
            var root=Rect(name,parent,Vector2.zero,Vector2.one);var layout=root.gameObject.AddComponent<LayoutElement>();layout.minHeight=layout.preferredHeight=height;
            RowLayout(root);return root;
        }
        private static void RowLayout(RectTransform root)
        { var row=root.gameObject.AddComponent<HorizontalLayoutGroup>();row.spacing=CandyUIStyle.Gap;row.childControlWidth=row.childControlHeight=true;row.childForceExpandWidth=true;row.childForceExpandHeight=false; }
        private TMP_Text TextAt(Transform parent,string name,string value,float size,TextAlignmentOptions alignment,float left,float right,float height,float top=0)
        {
            var rect=Rect(name,parent,new Vector2(0,1),Vector2.one);rect.offsetMin=new Vector2(left,-top-height);rect.offsetMax=new Vector2(-right,-top);
            var text=rect.gameObject.AddComponent<TextMeshProUGUI>();text.font=font;text.text=value;text.fontSize=size;text.color=CandyUIStyle.Ink;text.alignment=alignment;text.raycastTarget=false;
            text.textWrappingMode=TextWrappingModes.NoWrap;text.enableAutoSizing=true;text.fontSizeMin=16;text.fontSizeMax=size;return text;
        }
        private static void PlaceTop(RectTransform rect,float width,float right,float top,float height)
        { rect.anchorMin=rect.anchorMax=Vector2.one;rect.pivot=Vector2.one;rect.sizeDelta=new Vector2(width,height);rect.anchoredPosition=new Vector2(-right,-top); }
        private static string Percent(float value) => Mathf.RoundToInt(value*100).ToString(CultureInfo.InvariantCulture)+"%";
    }
}
