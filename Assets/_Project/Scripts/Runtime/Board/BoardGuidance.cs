using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.Board
{
    [RequireComponent(typeof(BoardController))]
    public sealed class BoardGuidance : MonoBehaviour
    {
        [SerializeField] private TMP_Text tutorialText, frostText;
        [SerializeField] private Button dismissButton;
        private BoardController board;
        private BoardView view;
        private float idle;
        private bool hintVisible, dismissed;
        private int levelNumber;
        private bool hasFocus = true;
        private PuzzleGame.Core.Levels.LevelProgress attempt;
        private bool topicObserved;
        public bool HintVisible=>hintVisible;
        public bool TutorialVisible=>tutorialText && tutorialText.transform.parent.gameObject.activeSelf;
        // Keep the input coordinate system stable when the first-move tip disappears.
        public bool ReservesTutorialSpace=>board && board.Session && board.Session.Definition && board.Session.Definition.Number<=5;
        public float HintDelay { get; set; } = 6;
        public void Configure(TMP_Text tutorial,TMP_Text frost,Button dismiss)
        {tutorialText=tutorial;frostText=frost;dismissButton=dismiss;}
        private void Awake(){board=GetComponent<BoardController>();view=GetComponent<BoardView>();}
        private void OnEnable(){board.Activity+=ResetHint;board.ResolutionStarted+=Observe;GamePreferences.Changed+=RefreshText;if(dismissButton)dismissButton.onClick.AddListener(Dismiss);}
        private void OnDisable(){board.Activity-=ResetHint;board.ResolutionStarted-=Observe;GamePreferences.Changed-=RefreshText;if(dismissButton)dismissButton.onClick.RemoveListener(Dismiss);ResetHint();}
        private void Observe(ResolutionStep step)
        {
            if(levelNumber==5)topicObserved|=step.FrostDamage.Count>0;
            foreach(var creation in step.SpecialCreations)
                if(levelNumber==3 && (creation.Piece.SpecialType==SpecialPieceType.Row || creation.Piece.SpecialType==SpecialPieceType.Column) ||
                   levelNumber==4 && (creation.Piece.SpecialType==SpecialPieceType.Bomb || creation.Piece.SpecialType==SpecialPieceType.ColorClear))topicObserved=true;
        }
        private void Dismiss(){dismissed=true;RefreshText();}
        private void OnApplicationFocus(bool focused){hasFocus=focused;if(!focused)ResetHint();}
        private void ResetHint(){idle=0;if(hintVisible)view.ShowSelection(board.SelectedPosition);hintVisible=false;}
        public void NotifyInput()=>ResetHint();
        public bool RequestHint()
        {
            if (!board.Session || board.Session.Progress == null || board.IsBusy || board.IsPaused || board.Session.Progress.IsFinished || board.SelectedItem.HasValue) return false;
            board.SetSelection(null);
            if (!GoalMoveFinder.TryFindMove(board.Model, board.Session.Progress, out var first, out var second)) return false;
            view.ShowHint(first,second);hintVisible=true;idle=0;return true;
        }
        private void Update()
        {
            if(!board.Session || board.Session.Progress==null)return;
            int number=board.Session.Definition?board.Session.Definition.Number:0;
            var progress=board.Session.Progress;
            if(progress!=attempt){attempt=progress;levelNumber=number;dismissed=false;topicObserved=false;RefreshText();}
            if(tutorialText && levelNumber>=1 && levelNumber<=5 && !dismissed)
                tutorialText.transform.parent.gameObject.SetActive(!board.SelectedItem.HasValue);
            if(frostText)frostText.gameObject.SetActive(false); // The shared GoalPanel owns ice progress.
            bool learned=levelNumber<=2?board.CompletedMoves>=(levelNumber==1?1:2):topicObserved && !board.IsBusy;
            if(tutorialText && (progress.IsFinished || learned) && tutorialText.transform.parent.gameObject.activeSelf) Dismiss();
            if(board.IsBusy || board.IsPaused || progress.IsFinished || board.SelectedPosition.HasValue || board.SelectedItem.HasValue || !hasFocus){ResetHint();return;}
            idle+=Time.deltaTime;
            float delay=HintDelay==6?GamePreferences.Current.hintDelay:HintDelay;
            if(!GamePreferences.Current.autoHints || idle<delay || hintVisible)return;
            RequestHint();
        }
        private void RefreshText()
        {
            ResetHint();
            if(!tutorialText)return;
            bool show=levelNumber>=1 && levelNumber<=5 && !dismissed;
            tutorialText.transform.parent.gameObject.SetActive(show);
            if(show)tutorialText.text=Localization.Get("tutorial"+levelNumber);
        }
        private void LateUpdate()
        {
            if(!tutorialText || !view.BoardCamera)return;
            var banner=(RectTransform)tutorialText.transform.parent;
            banner.anchorMin=new Vector2(view.BoardCamera.aspect>=1.25f?.34f:.045f,.015f);
            banner.anchorMax=new Vector2(.97f,.14f);banner.offsetMin=banner.offsetMax=Vector2.zero;
        }
    }
}
