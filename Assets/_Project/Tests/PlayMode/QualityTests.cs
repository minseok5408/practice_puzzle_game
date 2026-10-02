using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace PuzzleGame.Tests
{
    public sealed class QualityTests
    {
        [TearDown] public void Cleanup(){Object.FindFirstObjectByType<PlayerDialogs>()?.Close();Time.timeScale=1;GamePreferences.Load();}
        private static string Record(string id,string outcome,string source="player",string[] inputs=null,string reason=null)
            =>JsonUtility.ToJson(new PlaytestRecord{attemptId=id,levelId="level_046",source=source,outcome=outcome,inputs=inputs,reason=reason,movesRemaining=4,activeSeconds=60});
        [Test] public void AnalyticsSeparateSourcesItemsAndAbandonsAndExcludeCheats()
        {
            var result=PlaytestAnalysis.Read(new[]{Record("a","won"),Record("a","won"),Record("b","lost"),Record("c","restart"),
                Record("a","feedback",reason:"fewMoves"),Record("p","easter_egg"),Record("d","won","automated"),
                Record("e","won",inputs:new[]{"item,Bomb,3,4","item,ExtraMoves,0,0"}),"not json"});
            Assert.That(result.Rows.Count,Is.EqualTo(3));Assert.That(result.Duplicates,Is.EqualTo(1));
            Assert.That(result.ExcludedEasterEggs,Is.EqualTo(1));Assert.That(result.InvalidLines,Is.EqualTo(1));
            var human=result.Rows.Single(r=>r.Source=="player" && r.Cohort=="no-items");
            Assert.That(human.Finished,Is.EqualTo(2));Assert.That(human.Wins,Is.EqualTo(1));Assert.That(human.Abandoned,Is.EqualTo(1));
            Assert.That(human.MovesLeft,Is.EqualTo(4));Assert.That(human.FewMoves,Is.EqualTo(1));
            var item=result.Rows.Single(r=>r.Cohort=="items");Assert.That(item.ItemCounts,Is.EqualTo(new[]{0,1,0,1}));
        }
        [Test] public void LegacyDataNeverBecomesHumanAndMalformedMetadataIsIgnored()
        {
            var legacy=new PlaytestRecord{attemptId=null,source=null,recordedUtc="2026-10-02T01:02:03Z",levelId="level_049",rulesVersion="0.10.0",outcome="won"};
            var invalid=new PlaytestRecord{attemptId="bad",levelId="level_999",outcome="won"};
            var result=PlaytestAnalysis.Read(new[]{JsonUtility.ToJson(legacy),JsonUtility.ToJson(invalid),"{\"levelId\":\"level_046\",\"rulesVersion\":\"=1+1\"}"});
            Assert.That(result.Rows.Single().Source,Is.EqualTo("legacy"));Assert.That(result.InvalidLines,Is.EqualTo(2));
        }
        [Test] public void OldDisplaySettingsDefaultNumbersOffAndNewSettingsRoundTrip()
        {
            var old=JsonUtility.FromJson<DisplaySettings>("{\"width\":1280,\"height\":800,\"version\":1}");
            Assert.That(old.colorLabels,Is.False);old.colorLabels=true;
            Assert.That(JsonUtility.FromJson<DisplaySettings>(JsonUtility.ToJson(old)).colorLabels,Is.True);
        }
        [UnityTest] public IEnumerator IllustratedGuideHasArtworkOnAllPagesAndRestoresPause()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var dialogs=Object.FindFirstObjectByType<PlayerDialogs>();var board=Object.FindFirstObjectByType<BoardController>();
            for(int page=0;page<4;page++)
            {
                dialogs.Guide(page);yield return null;Canvas.ForceUpdateCanvases();
                var content=dialogs.transform.Find("PlayerDialog/Card/Viewport/Content");
                foreach(var art in content.GetComponentsInChildren<Image>().Where(i=>i.name=="Artwork"))Assert.That(art.sprite,Is.Not.Null);
                Assert.That(content.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.StartsWith("guide")),Is.False);
                Assert.That(board.IsPaused,Is.True);Assert.That(Time.timeScale,Is.Zero);
            }
            dialogs.Close();Assert.That(board.IsPaused,Is.False);Assert.That(Time.timeScale,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator NumericLabelsToggleWithoutDuplicatingAndMatchCandyColor()
        {
            GamePreferences.Load();yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var pieces=Object.FindObjectsByType<PieceView>(FindObjectsSortMode.None);Assert.That(pieces.Length,Is.EqualTo(64));
            for(int i=0;i<3;i++)
            {
                GamePreferences.Current.colorLabels=true;GamePreferences.Save();yield return null;
                foreach(var piece in pieces)
                {
                    Assert.That(piece.ColorLabelVisible,Is.True);
                    Assert.That(piece.GetComponentsInChildren<TextMeshPro>(true).Length,Is.EqualTo(1));
                    Assert.That(piece.GetComponentInChildren<TextMeshPro>().text,Is.EqualTo(((int)piece.CandyColor).ToString()));
                }
                GamePreferences.Current.colorLabels=false;GamePreferences.Save();yield return null;
                Assert.That(pieces.All(p=>!p.ColorLabelVisible),Is.True);
            }
        }
        [UnityTest] public IEnumerator AudioBoundsOverlapAndPrioritizesOutcomeEvenWhenPaused()
        {
            GamePreferences.Load();var old=Object.FindFirstObjectByType<GameAudio>();if(old)Object.Destroy(old);yield return null;
            var audio=GameAudio.Ensure();Time.timeScale=0;
            GameAudio.Play("swap");GameAudio.Play("match");GameAudio.Play("cascade");
            for(int i=0;i<20;i++)GameAudio.Play("cascade");
            Assert.That(audio.SuppressedCues,Is.GreaterThanOrEqualTo(20));Assert.That(audio.ActiveVoices,Is.LessThanOrEqualTo(3));
            GameAudio.Play("win");GameAudio.Play("match");Assert.That(audio.LastCue,Is.EqualTo("win"));
            Assert.That(audio.ActiveVoices,Is.LessThanOrEqualTo(1));Assert.That(audio.MusicClip.length,Is.EqualTo(144).Within(.01));
            foreach(var clip in new[]{audio.MusicClip,audio.EffectClip("special"),audio.EffectClip("complete")})
            {var samples=new float[clip.samples];clip.GetData(samples,0);Assert.That(samples.Max(v=>Mathf.Abs(v)),Is.LessThanOrEqualTo(clip==audio.MusicClip?.141f:.231f));}
            Time.timeScale=1;
        }
        [UnityTest] public IEnumerator GoalNumbersStayAboveProgressTrackAtWideAndPortraitAspect()
        {
            GamePreferences.Load();GamePreferences.Current.colorLabels=true;
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var board=Object.FindFirstObjectByType<BoardController>();
            board.Session.StartWithRules(new LevelRules(20,9999,10,new[]{2,0,2,0,2,0},1));
            yield return null;var panel=Object.FindFirstObjectByType<GoalPanel>();
            foreach(float aspect in new[]{1.6f,.8f})
            {
                Camera.main.aspect=aspect;Object.FindFirstObjectByType<CandyLayout>().ApplyLayout();Canvas.ForceUpdateCanvases();yield return null;
                foreach(string name in new[]{"Red","Yellow","Blue"})
                {
                    var root=panel.transform.Find(name);var badge=(RectTransform)root.Find("ColorLabel");var track=(RectTransform)root.Find("Track");
                    var a=new Vector3[4];var b=new Vector3[4];badge.GetWorldCorners(a);track.GetWorldCorners(b);
                    Assert.That(badge.gameObject.activeSelf,Is.True);Assert.That(a[0].y,Is.GreaterThan(b[1].y),"Progress track overlaps the color number.");
                    Assert.That(badge.GetComponentInChildren<TMP_Text>().text,Is.EqualTo(((int)Enum.Parse<PieceColor>(name)).ToString()));
                }
            }
            Camera.main.ResetAspect();
        }
        [UnityTest] public IEnumerator FirstMoveTipReturnsOnRestartAndManualDismissDoesNotPersist()
        {
            GamePreferences.Load();yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var board=Object.FindFirstObjectByType<BoardController>();Assert.That(board.Session.SelectLevel(1),Is.True);yield return null;
            var guidance=board.GetComponent<BoardGuidance>();Assert.That(guidance.TutorialVisible,Is.True);
            Assert.That(MoveFinder.TryFindMove(board.Model,out var first,out var second),Is.True);Assert.That(board.TrySwap(first,second),Is.True);
            float until=Time.realtimeSinceStartup+15;while(board.IsBusy && Time.realtimeSinceStartup<until)yield return null;yield return null;
            Assert.That(board.IsBusy,Is.False);Assert.That(guidance.TutorialVisible,Is.False);
            board.Session.RestartLevel();yield return null;Assert.That(guidance.TutorialVisible,Is.True);
            Object.FindFirstObjectByType<PlayerDialogs>().transform.Find("TutorialBanner/Dismiss").GetComponent<Button>().onClick.Invoke();
            Assert.That(guidance.TutorialVisible,Is.False);board.Session.RestartLevel();yield return null;Assert.That(guidance.TutorialVisible,Is.True);
        }
    }
}
