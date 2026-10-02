using System;
using System.Collections.Generic;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    // Shared, scrollable modal used by both the map and the game canvas.
    public sealed partial class PlayerDialogs : MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private Sprite rounded;
        private GameObject overlay;
        private RectTransform card, content, footer;
        private TMP_Text heading;
        private ScrollRect scroll;
        private BoardController board;
        private float previousTimeScale;
        private bool previousPause;
        private float minimumCardHeight=280;
        private GameObject previousSelection;
        private readonly Dictionary<Selectable, Navigation> previousNavigation = new Dictionary<Selectable, Navigation>();
        public bool IsVisible => overlay && overlay.activeSelf;
        public bool IsSettings { get; private set; }
        public Button SettingsCloseButton { get; private set; }
        public TMP_Dropdown DisplayResolution { get; private set; }
        public Toggle DisplayWindowed { get; private set; }
        public Button DisplayApply { get; private set; }
        private TMP_Text displayStatus;
        public Sprite[] ItemArtwork => guideItems;
        public Sprite CandyArtwork(PuzzleGame.Core.Board.PieceColor color) => Candy(color);
        public Sprite IceArtwork => frostIcon;
        public void Configure(TMP_FontAsset bodyFont, Sprite panel) { font=bodyFont; rounded=panel; }
        private void Awake() { board=FindFirstObjectByType<BoardController>(); }
        private void Start()
        {
            GameAudio.Ensure();
            var restart=transform.Find("ScoreCard/Restart")?.GetComponent<Button>();
            CandyUIStyle.Button(restart,CandyButtonRole.Secondary,rounded);
            var map=transform.Find("ScoreCard/StageRibbon")?.GetComponent<Button>();
            CandyUIStyle.Button(map,CandyButtonRole.Secondary,rounded);
            var tutorial=transform.Find("TutorialBanner");
            if(tutorial)
            {
                CandyUIStyle.CloseButton(tutorial.Find("Dismiss").GetComponent<Button>(),rounded);
                var closeRect=(RectTransform)tutorial.Find("Dismiss");
                closeRect.anchorMin=closeRect.anchorMax=new Vector2(1,.5f);closeRect.pivot=new Vector2(1,.5f);closeRect.anchoredPosition=new Vector2(-CandyUIStyle.Padding,0);
                var text=(RectTransform)tutorial.Find("Tutorial");text.anchorMax=new Vector2(1,text.anchorMax.y);text.offsetMax=new Vector2(-88,0);
            }
        }
        private void Update()
        {
            if(!IsVisible)return;
            if(displayStatus)displayStatus.text=GetComponent<SettingsPopup>().StatusText;
            var parent=(RectTransform)overlay.transform;
            float footerSpace=footer.gameObject.activeSelf?CandyUIStyle.ButtonHeight+CandyUIStyle.Gap:0;
            scroll.viewport.offsetMin=new Vector2(CandyUIStyle.Padding,CandyUIStyle.Padding+footerSpace);
            ((RectTransform)scroll.verticalScrollbar.transform).offsetMin=new Vector2(-14,CandyUIStyle.Padding+footerSpace);
            float preferred=Mathf.Clamp(LayoutUtility.GetPreferredHeight(content)+CandyUIStyle.HeaderHeight+CandyUIStyle.Padding+footerSpace,minimumCardHeight,660);
            card.sizeDelta=new Vector2(Mathf.Min(660,parent.rect.width-2*CandyUIStyle.Padding),Mathf.Min(preferred,parent.rect.height-2*CandyUIStyle.Padding));
        }
        private void Open(string title)
        {
            IsSettings=false;SettingsCloseButton=null;DisplayResolution=null;DisplayWindowed=null;DisplayApply=null;displayStatus=null;
            if(!overlay) Build();
            minimumCardHeight=280;
            if(!IsVisible)
            {
                previousTimeScale=Time.timeScale;previousPause=board && board.IsPaused;
                previousSelection=EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
                previousNavigation.Clear();
                foreach(var selectable in GetComponentsInChildren<Selectable>(true))
                    if(!selectable.transform.IsChildOf(overlay.transform))
                    {previousNavigation.Add(selectable,selectable.navigation);selectable.navigation=new Navigation{mode=Navigation.Mode.None};}
                if(board) { board.GetComponent<BoardInput>()?.CancelGesture();board.SetPaused(true); }
                Time.timeScale=0;overlay.SetActive(true);
            }
            overlay.transform.SetAsLastSibling();
            CandyUIStyle.Dim(overlay,false);
            if(EventSystem.current)EventSystem.current.SetSelectedGameObject(null);
            foreach(Transform child in content) { child.gameObject.SetActive(false);Destroy(child.gameObject); }
            foreach(Transform child in footer) { child.gameObject.SetActive(false);Destroy(child.gameObject); }
            footer.gameObject.SetActive(false);
            heading.text=Localization.Get(title);
            scroll.StopMovement();content.anchoredPosition=Vector2.zero;
        }
        private void Build()
        {
            var root=Rect("PlayerDialog",transform,Vector2.zero,Vector2.one);
            overlay=root.gameObject;root.gameObject.AddComponent<Image>().color=new Color(.20f,.08f,.26f,.82f);
            card=Rect("Card",root,new Vector2(.5f,.5f),new Vector2(.5f,.5f));card.sizeDelta=new Vector2(660,660);
            var bg=card.gameObject.AddComponent<Image>();bg.sprite=rounded;bg.type=Image.Type.Sliced;bg.color=new Color32(255,247,234,255);
            CandyUIStyle.Popup(overlay,card,rounded);
            heading=Rect("Title",card,Vector2.zero,Vector2.one).gameObject.AddComponent<TextMeshProUGUI>();heading.font=font;heading.raycastTarget=false;CandyUIStyle.Header(heading);
            var viewport=Rect("Viewport",card,Vector2.zero,Vector2.one);viewport.offsetMin=new Vector2(CandyUIStyle.Padding,CandyUIStyle.Padding);viewport.offsetMax=new Vector2(-CandyUIStyle.Padding,-CandyUIStyle.HeaderHeight);
            viewport.gameObject.AddComponent<RectMask2D>();
            content=Rect("Content",viewport,new Vector2(0,1),Vector2.one);content.pivot=new Vector2(.5f,1);
            var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=CandyUIStyle.Gap;layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandHeight=false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            scroll=card.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=30;
            var bar=Rect("ScrollBar",card,new Vector2(1,0),Vector2.one);bar.offsetMin=new Vector2(-14,CandyUIStyle.Padding);bar.offsetMax=new Vector2(-8,-CandyUIStyle.HeaderHeight);var track=bar.gameObject.AddComponent<Image>();track.color=new Color32(228,209,231,255);
            var handle=Rect("Handle",bar,Vector2.zero,Vector2.one);var handleImage=handle.gameObject.AddComponent<Image>();handleImage.color=new Color32(153,102,167,255);
            var scrollbar=bar.gameObject.AddComponent<Scrollbar>();scrollbar.handleRect=handle;scrollbar.targetGraphic=handleImage;scrollbar.direction=Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar=scrollbar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
            footer=Rect("Footer",card,Vector2.zero,Vector2.right);
            footer.offsetMin=new Vector2(CandyUIStyle.Padding,CandyUIStyle.Padding);footer.offsetMax=new Vector2(-CandyUIStyle.Padding,CandyUIStyle.Padding+CandyUIStyle.ButtonHeight);
            var footerLayout=footer.gameObject.AddComponent<HorizontalLayoutGroup>();footerLayout.spacing=CandyUIStyle.Gap;
            footerLayout.childControlWidth=footerLayout.childControlHeight=true;footerLayout.childForceExpandWidth=true;footerLayout.childForceExpandHeight=false;
            footer.gameObject.SetActive(false);
            CandyUIStyle.CloseButton(card,rounded,Close);
            overlay.SetActive(false);
        }
        public void Close()
        {
            if(!IsVisible)return;
            if(GetComponent<SettingsPopup>()?.IsApplying==true)return;
            IsSettings=false;
            overlay.SetActive(false);Time.timeScale=previousTimeScale;
            if(board)board.SetPaused(previousPause);
            foreach(var pair in previousNavigation)if(pair.Key)pair.Key.navigation=pair.Value;
            previousNavigation.Clear();
            if(EventSystem.current && previousSelection && previousSelection.activeInHierarchy) EventSystem.current.SetSelectedGameObject(previousSelection);
        }
        public void Message(string title,string body) { Open(title);Label(body,22);FooterButton("ok",Close); }
        public void Confirm(string title,string body,Action confirm)
        { Open(title);Label(body,22);FooterButton("cancel",Close);FooterButton("reset",()=>{Close();confirm();}); }
        public void ConfirmAbandon(bool restart,Action action)
        {
            if(board && (board.IsBusy || board.IsPaused))return;
            var progress=board?.Session?.Progress;
            if(progress==null || progress.IsFinished || (progress.MovesUsed==0 && board.Session.ItemsUsed==0)){action();return;}
            Open(restart?"restartTitle":"leaveTitle");Label(Localization.Get("leaveBody"),22);
            FooterButton("cancel",Close);FooterButton(restart?"restartAction":"leaveAction",()=>{Close();action();});
        }
        public void Feedback()
        {
            Open("feedback");
            foreach(string reason in new[]{"luck","fewMoves","unclear"})
            { string value=reason;Button(value,()=>{bool saved=PlaytestJournal.Feedback(value);Message("feedback",Localization.Get(saved?"feedbackSaved":"saveFailed"));}); }
            FooterButton("close",Close);
        }
        private TMP_Text Label(string value,float size,float height=0)
        {
            var text=Rect("Text",content,Vector2.zero,Vector2.one).gameObject.AddComponent<TextMeshProUGUI>();
            text.font=font;text.text=value;text.fontSize=size;text.color=CandyUIStyle.Ink;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
            var layout=text.gameObject.AddComponent<LayoutElement>();layout.minHeight=height>0?height:90;layout.preferredHeight=height>0?height:Mathf.Max(90,(value.Split('\n').Length+2)*30);return text;
        }
        private Button Button(string key,Action action) => CreateButton(content,key,action);
        private Button FooterButton(string key,Action action)
        {
            footer.gameObject.SetActive(true);
            var button=CreateButton(footer,key,action);button.GetComponent<LayoutElement>().flexibleWidth=1;return button;
        }
        private Button CreateButton(Transform parent,string key,Action action,string label=null)
        {
            var rect=Rect(key??"Button",parent,Vector2.zero,Vector2.one);var image=rect.gameObject.AddComponent<Image>();image.sprite=rounded;image.type=Image.Type.Sliced;image.color=new Color32(135,76,150,255);
            var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;button.onClick.AddListener(()=>action());
            var layout=rect.gameObject.AddComponent<LayoutElement>();layout.minHeight=layout.preferredHeight=CandyUIStyle.ButtonHeight;
            var text=Rect("Label",rect,new Vector2(.03f,.04f),new Vector2(.97f,.96f)).gameObject.AddComponent<TextMeshProUGUI>();text.font=font;text.fontSize=22;text.color=Color.white;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
            text.enableAutoSizing=true;text.fontSizeMin=16;text.fontSizeMax=22;
            if(key!=null)text.gameObject.AddComponent<LocalizedText>().Bind(key);else text.text=label;
            var role=key=="start" || key=="ok"?CandyButtonRole.Primary:key=="cancel" || key=="close"?CandyButtonRole.Quiet:key=="reset"?CandyButtonRole.Danger:CandyButtonRole.Secondary;
            CandyUIStyle.Button(button,role,rounded);
            if((parent==content || parent==footer) && EventSystem.current && !EventSystem.current.currentSelectedGameObject)EventSystem.current.SetSelectedGameObject(button.gameObject);
            return button;
        }
        private static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max)
        {var r=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        private void OnDisable()=>Close();
    }
}
