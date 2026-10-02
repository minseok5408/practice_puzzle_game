using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEngine;

namespace PuzzleGame.Runtime.UI
{
    public sealed class GameplayCelebration : MonoBehaviour
    {
        [SerializeField] private BoardController board;
        [SerializeField] private LevelSession session;
        [SerializeField] private TMP_Text combo, stars;
        private string comboKey;
        private float remaining;
        public void Configure(BoardController controller,LevelSession level,TMP_Text comboText,TMP_Text completionStars)
        {board=controller;session=level;combo=comboText;stars=completionStars;}
        private void OnEnable(){board.ResolutionStarted+=ShowCombo;session.Changed+=Refresh;GamePreferences.Changed+=Refresh;Refresh();}
        private void OnDisable(){if(board)board.ResolutionStarted-=ShowCombo;if(session)session.Changed-=Refresh;GamePreferences.Changed-=Refresh;}
        private void ShowCombo(ResolutionStep step)
        {
            if(step.SpecialActivations.Count<2)return;
            bool rainbow=false,bomb=false;
            foreach(var activation in step.SpecialActivations){rainbow|=activation.Type==SpecialPieceType.ColorClear;bomb|=activation.Type==SpecialPieceType.Bomb;}
            comboKey=rainbow?"comboRainbow":bomb?"comboBomb":"comboLines";remaining=1.2f;combo.gameObject.SetActive(true);combo.text=Localization.Get(comboKey);
        }
        private void Refresh()
        {
            // The result presentation now owns the completion crown and celebration.
            if(stars)stars.gameObject.SetActive(false);
            if(session.Progress==null || session.Progress.IsFinished){remaining=0;combo.gameObject.SetActive(false);}
            if(remaining>0)combo.text=Localization.Get(comboKey);
        }
        private void Update()
        {
            bool reduced=GamePreferences.Current.reducedEffects;
            if(remaining>0)
            {
                remaining-=Time.deltaTime;
                combo.alpha=Mathf.Clamp01(remaining/.3f);
                combo.transform.localScale=Vector3.one*(reduced?1:1+.06f*Mathf.Sin((1.2f-remaining)*Mathf.PI));
                if(remaining<=0)combo.gameObject.SetActive(false);
            }
        }
    }
}
