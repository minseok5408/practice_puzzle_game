using System.Collections;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace PuzzleGame.Tests
{
    public sealed class ResultPresentationTests
    {
        [SetUp]public void Setup(){Time.timeScale=1;GamePreferences.Load();if(CampaignState.Instance)Object.DestroyImmediate(CampaignState.Instance.gameObject);}
        [TearDown]public void Cleanup(){Time.timeScale=1;GamePreferences.Load();}

        [UnityTest]
        public IEnumerator ResultUsesDistinctArtworkAndRestartClearsPresentation()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var session=Object.FindFirstObjectByType<BoardController>().Session;
            var popup=Object.FindFirstObjectByType<ResultPopup>();
            session.StartWithRules(new LevelRules(1,30,10));Win(session);
            yield return new WaitForSecondsRealtime(.18f);
            Assert.That(popup.Presentation.BadgeSprite.name,Is.EqualTo("VictoryBadge"));
            Assert.That(popup.Presentation.ActiveConfettiCount,Is.GreaterThan(0));
            popup.RestartButton.onClick.Invoke();yield return null;
            Assert.That(popup.IsVisible,Is.False);Assert.That(popup.Presentation.IsAnimating,Is.False);
            Assert.That(popup.Presentation.ActiveConfettiCount,Is.Zero);
            session.StartWithRules(new LevelRules(1,9999,10));session.BeginMove();session.CompleteMove();yield return null;
            Assert.That(popup.Presentation.BadgeSprite.name,Is.EqualTo("RetryBadge"));
            Assert.That(popup.Presentation.ActiveConfettiCount,Is.Zero);
            for(int i=1;i<50;i++)session.Campaign.RecordWin(i,2000);
            Assert.That(session.SelectLevel(50),Is.True);Win(session);yield return null;
            Assert.That(popup.Presentation.BadgeSprite.name,Is.EqualTo("CompletionCrown"));
            Assert.That(popup.NextButton.gameObject.activeSelf,Is.False);
        }

        [UnityTest]
        public IEnumerator ReducedEffectsStopsImmediatelyAndPreferenceRefreshDoesNotReplay()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var session=Object.FindFirstObjectByType<BoardController>().Session;
            var popup=Object.FindFirstObjectByType<ResultPopup>();
            session.StartWithRules(new LevelRules(1,30,10));Win(session);
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(popup.Presentation.IsAnimating,Is.True);
            GamePreferences.Current.reducedEffects=true;GamePreferences.Save();
            Assert.That(popup.Presentation.IsAnimating,Is.False);
            Assert.That(popup.Presentation.ActiveConfettiCount,Is.Zero);
            Assert.That(popup.Presentation.transform.localScale,Is.EqualTo(Vector3.one));
            Assert.That(popup.Presentation.GetComponent<CanvasGroup>().alpha,Is.EqualTo(1));
            GamePreferences.Current.reducedEffects=false;GamePreferences.Current.language="en";GamePreferences.Save();
            yield return null;
            Assert.That(popup.Title,Is.EqualTo(Localization.Get("won")));
            Assert.That(popup.Presentation.IsAnimating,Is.False);
            session.RestartLevel();Win(session);yield return null;
            Assert.That(popup.Presentation.IsAnimating,Is.True);
            yield return new WaitForSecondsRealtime(1.25f);
            Assert.That(popup.Presentation.IsAnimating,Is.False);
            Assert.That(popup.Presentation.ActiveConfettiCount,Is.Zero);
        }

        private static void Win(LevelSession session)
        {
            Assert.That(session.BeginMove(),Is.True);
            var step=new ResolutionStep();int id=10000;
            for(int color=1;color<=6;color++)
            {
                int count=Mathf.Max(session.Progress.Rules.CollectionTarget((PieceColor)color),50);
                for(int i=0;i<count;i++){step.RemovedIds.Add(++id);step.RemovedPieces.Add(new RemovedPiece(id,(PieceColor)color));}
            }
            for(int i=0;i<session.Progress.Rules.FrostTarget;i++)step.FrostDamage.Add(new FrostDamage(new GridPosition(i%8,i/8),0));
            session.ApplyRemoval(step);session.CompleteMove();
            Assert.That(session.Progress.Outcome,Is.EqualTo(LevelOutcome.Won));
        }
    }
}
