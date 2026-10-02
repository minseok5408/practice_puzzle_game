using System.Collections;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace PuzzleGame.Tests
{
    public sealed class GoalAndHintTests
    {
        [SetUp]public void Setup(){Time.timeScale=1;GamePreferences.Load();}
        [TearDown]public void Cleanup(){Time.timeScale=1;GamePreferences.Load();}
        [UnityTest]
        public IEnumerator CollectionAndIceCardsFollowProgressAndCancelRestoresIncompleteState()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var session=Object.FindFirstObjectByType<BoardController>().Session;
            session.StartWithRules(new LevelRules(20,9999,10,new[]{2,0,0,0,0,0},1));yield return null;
            var panel=Object.FindFirstObjectByType<GoalPanel>();Assert.That(panel.GoalCount,Is.EqualTo(2));
            session.BeginMove();
            var step=new ResolutionStep();step.RemovedIds.AddRange(new[]{10001,10002});
            step.RemovedPieces.Add(new RemovedPiece(10001,PieceColor.Red));step.RemovedPieces.Add(new RemovedPiece(10002,PieceColor.Red));
            step.FrostDamage.Add(new FrostDamage(new GridPosition(0,0),0));session.ApplyRemoval(step);yield return null;
            Assert.That(panel.CompletedGoalCount,Is.EqualTo(2));
            session.CancelMove();yield return null;
            Assert.That(panel.CompletedGoalCount,Is.Zero);
            Assert.That(panel.transform.Find("Red/Count").GetComponent<TMPro.TMP_Text>().text,Is.EqualTo("0 / 2"));
            Assert.That(panel.transform.Find("Frost/Complete").gameObject.activeSelf,Is.False);
            session.StartWithRules(new LevelRules(20,500,10));yield return null;
            Assert.That(panel.gameObject.activeSelf,Is.False);
        }
        [UnityTest]
        public IEnumerator IceUsesThreeArtworkStatesAndRestoresAfterBreaking()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var cell=Object.FindFirstObjectByType<CellView>();
            cell.ShowFrost(3);var intact=cell.FrostSprite;
            cell.ShowFrost(2);var chipped=cell.FrostSprite;
            cell.ShowFrost(1);var cracked=cell.FrostSprite;
            Assert.That(intact,Is.Not.Null);Assert.That(chipped,Is.Not.SameAs(intact));Assert.That(cracked,Is.Not.SameAs(chipped));
            Assert.That(cell.GetComponentsInChildren<LineRenderer>(true),Is.Empty);
            cell.DamageProgress(0,1,false);cell.ShowFrost(0);
            Assert.That(cell.transform.Find("IceGlass").gameObject.activeSelf,Is.False);
            cell.ShowFrost(2);
            Assert.That(cell.FrostSprite,Is.SameAs(chipped));Assert.That(cell.transform.Find("IceGlass").gameObject.activeSelf,Is.True);
            Assert.That(cell.transform.Find("IceBack").GetComponent<SpriteRenderer>().color.a,Is.EqualTo(1));
        }
        [UnityTest]
        public IEnumerator HintShowsSwapWithoutChangingPiecesAndPauseClearsIt()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var board=Object.FindFirstObjectByType<BoardController>();var hint=board.GetComponent<BoardGuidance>();
            var pieces=board.GetComponentsInChildren<PieceView>();var positions=new Vector3[pieces.Length];
            for(int i=0;i<pieces.Length;i++)positions[i]=pieces[i].transform.localPosition;
            hint.SendMessage("OnApplicationFocus",true);hint.HintDelay=.05f;yield return new WaitForSeconds(.12f);
            Assert.That(hint.HintVisible,Is.True);Assert.That(board.GetComponentInChildren<BoardHintView>(),Is.Not.Null);
            for(int i=0;i<pieces.Length;i++){Assert.That(pieces[i].transform.localPosition,Is.EqualTo(positions[i]));Assert.That(pieces[i].IsSelected,Is.False);}
            GamePreferences.Current.reducedEffects=true;yield return null;
            Assert.That(hint.HintVisible,Is.True);board.SetPaused(true);yield return null;
            Assert.That(hint.HintVisible,Is.False);Assert.That(board.GetComponentInChildren<BoardHintView>(),Is.Null);
            board.SetPaused(false);
        }
    }
}
