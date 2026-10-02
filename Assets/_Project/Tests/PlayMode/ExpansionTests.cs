using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace PuzzleGame.Tests
{
    public sealed class ExpansionTests
    {
        [SetUp]public void Setup(){Time.timeScale=1;GamePreferences.Load();if(CampaignState.Instance)Object.DestroyImmediate(CampaignState.Instance.gameObject);}
        [TearDown]public void Cleanup(){Object.FindFirstObjectByType<PlayerDialogs>()?.Close();Object.FindFirstObjectByType<SettingsPopup>()?.Close();Time.timeScale=1;GamePreferences.Load();}
        [Test]
        public void LayoutValidationRejectsDuplicateOutOfRangeAndImpossibleTargets()
        {
            var level=ScriptableObject.CreateInstance<LevelDefinition>();
            try
            {
                Assert.Throws<InvalidOperationException>(()=>level.ConfigureLayout(new[]{new LevelDefinition.FrostCell(0,0,1),new LevelDefinition.FrostCell(0,0,2)},1));
                Assert.Throws<InvalidOperationException>(()=>level.ConfigureLayout(new[]{new LevelDefinition.FrostCell(8,0,1)},1));
                Assert.Throws<InvalidOperationException>(()=>level.ConfigureLayout(new[]{new LevelDefinition.FrostCell(0,0,1)},2));
                level.ConfigureLayout(new[]{new LevelDefinition.FrostCell(0,0,2)},1);
                var first=level.CreateBoard();first.SetCell(new GridPosition(0,0),new PuzzleGame.Core.Board.CellState());
                Assert.That(level.CreateBoard().GetCell(new GridPosition(0,0)).FrostHealth,Is.EqualTo(2));
                Assert.Throws<InvalidOperationException>(()=>level.ConfigureLayout(Array.Empty<LevelDefinition.FrostCell>(),0,new PieceColor[64]));
            }finally{Object.DestroyImmediate(level);}
        }
        [Test]
        public void PreferencesRoundTripAndOldDisplaySettingsKeepDefaults()
        {
            string path=Path.Combine(Application.temporaryCachePath,"preferences-"+Guid.NewGuid()+".json");
            try
            {
                File.WriteAllText(path,"{\"version\":1,\"width\":1280,\"height\":800,\"windowed\":true}");
                var p=DisplaySettings.Load(path);Assert.That(p,Is.Not.Null);Assert.That(p.language,Is.EqualTo("ko"));Assert.That(p.musicVolume,Is.GreaterThan(0));
                p.language="en";p.musicVolume=.2f;p.effectsVolume=.8f;p.musicMuted=true;p.reducedEffects=true;Assert.That(p.Save(path),Is.True);
                var q=DisplaySettings.Load(path);Assert.That(q.language,Is.EqualTo("en"));Assert.That(q.musicVolume,Is.EqualTo(.2f));Assert.That(q.effectsVolume,Is.EqualTo(.8f));Assert.That(q.musicMuted && q.reducedEffects,Is.True);
                File.WriteAllText(path,"broken");Assert.That(DisplaySettings.Load(path).language,Is.EqualTo("ko"));
            }finally{foreach(string file in new[]{path,path+".bak",path+".tmp"})if(File.Exists(file))File.Delete(file);}
        }
        [UnityTest]
        public IEnumerator PreviewRequiresPlayAndCancelKeepsMap()
        {
            yield return SceneManager.LoadSceneAsync("WorldMap");yield return null;
            var map=Object.FindFirstObjectByType<WorldMapView>();var dialogs=map.GetComponent<PlayerDialogs>();
            map.StageButtons[0].onClick.Invoke();yield return null;
            Assert.That(dialogs.IsVisible,Is.True);Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("WorldMap"));
            dialogs.Close();Assert.That(Time.timeScale,Is.EqualTo(1));
            map.StageButtons[0].onClick.Invoke();yield return null;
            dialogs.transform.Find("PlayerDialog/Card/Footer/start").GetComponent<Button>().onClick.Invoke();
            float end=Time.realtimeSinceStartup+10;while(SceneManager.GetActiveScene().name!="Game" && Time.realtimeSinceStartup<end)yield return null;
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("Game"));
        }
        [UnityTest]
        public IEnumerator FrostRollbackRestoresModelAndProgressAndReducedEffectsStillResolve()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var board=Object.FindFirstObjectByType<BoardController>();var session=board.Session;
            for(int i=1;i<5;i++)session.Campaign.RecordWin(i,2000);
            Assert.That(session.SelectLevel(5),Is.True);
            var p=new GridPosition(0,0);board.Model.SetCell(p,new PuzzleGame.Core.Board.CellState(2));
            MoveFinder.TryFindMove(board.Model,out var a,out var b);board.TrySwap(a,b);
            board.Model.SetCell(p,new PuzzleGame.Core.Board.CellState());board.enabled=false;yield return null;
            Assert.That(board.Model.GetCell(p).FrostHealth,Is.EqualTo(2));Assert.That(session.Progress.FrostCleared,Is.Zero);
            board.enabled=true;GamePreferences.Current.reducedEffects=true;
            Assert.That(board.TrySwap(a,b),Is.True);
            float end=Time.realtimeSinceStartup+15;while(board.IsBusy && Time.realtimeSinceStartup<end)yield return null;
            Assert.That(board.IsBusy,Is.False);Assert.That(session.Progress.MovesRemaining,Is.EqualTo(session.Progress.Rules.StartingMoves-1));
            Assert.That(board.GetComponent<BoardView>().Effects.ActiveVisualCount,Is.Zero);
        }
        [UnityTest]
        public IEnumerator HintFindsLegalPairAndInputPauseAndFocusClearIt()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var board=Object.FindFirstObjectByType<BoardController>();var hint=board.GetComponent<BoardGuidance>();
            hint.SendMessage("OnApplicationFocus",true);hint.HintDelay=.05f;
            yield return new WaitForSeconds(.12f);Assert.That(hint.HintVisible,Is.True);
            board.SetSelection(new GridPosition(0,0));Assert.That(hint.HintVisible,Is.False);
            board.SetSelection(null);board.SetPaused(true);yield return new WaitForSeconds(.1f);Assert.That(hint.HintVisible,Is.False);
            board.SetPaused(false);hint.SendMessage("OnApplicationFocus",false);yield return new WaitForSeconds(.1f);Assert.That(hint.HintVisible,Is.False);
        }
        [UnityTest]
        public IEnumerator NestedPreferencesRestorePauseAndLanguageUpdatesLive()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var settings=Object.FindFirstObjectByType<SettingsPopup>();var dialogs=settings.GetComponent<PlayerDialogs>();
            settings.Open();dialogs.Preferences();Assert.That(Time.timeScale,Is.Zero);
            GamePreferences.Current.language="en";GamePreferences.Save();yield return null;
            Assert.That(settings.OpenButton.GetComponentInChildren<TMPro.TMP_Text>().text,Is.EqualTo("Settings"));
            dialogs.Close();Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(settings.IsVisible,Is.False);
        }
    }
}
