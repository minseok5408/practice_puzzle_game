using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace PuzzleGame.Tests
{
    public sealed class ItemInventoryTests
    {
        private string path;
        [SetUp]public void Setup()
        {Time.timeScale=1;if(CampaignState.Instance)Object.DestroyImmediate(CampaignState.Instance.gameObject);path=Path.Combine(Application.temporaryCachePath,"items-"+Guid.NewGuid()+".json");}
        [TearDown]public void Cleanup()
        {
            Object.FindFirstObjectByType<PlayerDialogs>()?.Close();Object.FindFirstObjectByType<SettingsPopup>()?.Close();Time.timeScale=1;
            if(CampaignState.Instance)typeof(CampaignState).GetField("savePath",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(CampaignState.Instance,null);
            foreach(string file in new[]{path,path+".bak",path+".tmp"})if(File.Exists(file))File.Delete(file);
        }
        [Test]
        public void VersionTwoMigratesOnceAndSpentInventorySurvivesReload()
        {
            var old=new CampaignProgress{version=2};old.RecordWin(1,890);old.selectedLevel=2;
            string json=JsonUtility.ToJson(old);json=json.Substring(0,json.IndexOf(",\"items\""))+"}";File.WriteAllText(path,json);
            var p=CampaignProgress.Load(path);Assert.That(p.version, Is.EqualTo(4));Assert.That(p.selectedLevel,Is.EqualTo(2));Assert.That(p.bestScores[0],Is.EqualTo(890));
            CollectionAssert.AreEqual(new[]{3,3,3,3},p.items);Assert.That(p.TrySpendItem(ItemType.Bomb),Is.True);Assert.That(p.Save(path),Is.True);
            CollectionAssert.AreEqual(new[]{3,2,3,3},CampaignProgress.Load(path).items);
        }
        [Test]
        public void RewardsAreOncePerStageCycleAndMilestonesGrantAllWithCap()
        {
            var p=new CampaignProgress();
            for(int n=1;n<=5;n++)
            {
                p.RecordWin(n,100);var granted=p.ClaimItemReward(n);
                Assert.That(granted.Sum(),Is.EqualTo(n==5?4:1));Assert.That(p.ClaimItemReward(n).Sum(),Is.Zero);
            }
            CollectionAssert.AreEqual(new[]{5,5,5,5},p.items);
            p.items=new[]{99,99,99,99};p.RecordWin(6,100);Assert.That(p.ClaimItemReward(6).Sum(),Is.Zero);Assert.That(p.items.Max(),Is.EqualTo(99));
            p.items[0]=0;Assert.That(p.TrySpendItem(ItemType.Hammer),Is.False);Assert.That(p.TrySpendItem((ItemType)10),Is.False);
        }
        [Test]
        public void CorruptInventoryRecoversBackupWithoutRefilling()
        {
            var p=new CampaignProgress();p.TrySpendItem(ItemType.Hammer);p.Save(path);p.Save(path);
            p.items[0]=-1;File.WriteAllText(path,JsonUtility.ToJson(p));
            var loaded=CampaignProgress.Load(path,out var status);Assert.That(status,Is.EqualTo(ProgressLoadStatus.Recovered));Assert.That(loaded.ItemCount(ItemType.Hammer),Is.EqualTo(2));
        }
        [UnityTest]
        public IEnumerator FourEffectsPersistStockLeaveMovesFreeAndRespectPause()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var board=Object.FindFirstObjectByType<BoardController>();var s=board.Session;s.StartWithRules(new LevelRules(20,100000,10,null,1));
            var target=new GridPosition(3,3);board.Model.SetCell(target,new CellState(1));board.GetComponent<BoardView>().Rebuild(board.Model);
            Assert.That(board.ArmItem(ItemType.Hammer),Is.True);board.CancelItem();Assert.That(s.ItemCount(ItemType.Hammer),Is.EqualTo(3));
            board.SetPaused(true);Assert.That(board.TryUseItem(ItemType.ExtraMoves),Is.False);board.SetPaused(false);
            Assert.That(board.TryUseItem(ItemType.Hammer,target),Is.True);Assert.That(board.TryUseItem(ItemType.Bomb,target),Is.False);yield return Settle(board);
            Assert.That(s.Progress.FrostCleared,Is.GreaterThanOrEqualTo(1));Assert.That(s.Progress.Score,Is.Zero);Assert.That(s.Progress.MovesRemaining,Is.EqualTo(20));
            Assert.That(board.TryUseItem(ItemType.Bomb,target),Is.True);yield return Settle(board);
            var ids=Enumerable.Range(0,64).Select(i=>board.Model.GetPiece(new GridPosition(i%8,i/8)).Id).OrderBy(i=>i).ToArray();
            board.Model.SetCell(target,new CellState(2));Assert.That(board.TryUseItem(ItemType.Shuffle),Is.True);yield return Settle(board);
            CollectionAssert.AreEqual(ids,Enumerable.Range(0,64).Select(i=>board.Model.GetPiece(new GridPosition(i%8,i/8)).Id).OrderBy(i=>i));
            Assert.That(board.Model.GetCell(target).FrostHealth,Is.EqualTo(2));Assert.That(MatchFinder.FindMatches(board.Model),Is.Empty);Assert.That(MoveFinder.HasAnyMove(board.Model),Is.True);
            Assert.That(board.TryUseItem(ItemType.ExtraMoves),Is.True);Assert.That(s.Progress.MovesRemaining,Is.EqualTo(25));Assert.That(s.Progress.MovesUsed,Is.Zero);
            s.RestartLevel();CollectionAssert.AreEqual(new[]{2,2,2,2},s.Campaign.items);Assert.That(s.Progress.MovesRemaining,Is.EqualTo(20));
        }
        [UnityTest]
        public IEnumerator InterruptedItemRefundsAndEasterEggCannotClaimReward()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var board=Object.FindFirstObjectByType<BoardController>();var s=board.Session;var target=new GridPosition(3,3);int id=board.Model.GetPiece(target).Id;
            Assert.That(board.TryUseItem(ItemType.Bomb,target),Is.True);board.enabled=false;yield return null;
            Assert.That(board.Model.GetPiece(target).Id,Is.EqualTo(id));Assert.That(s.ItemCount(ItemType.Bomb),Is.EqualTo(3));Assert.That(s.Progress.Score,Is.Zero);
            board.enabled=true;Assert.That(board.TryUseItem(ItemType.Bomb,target),Is.True);board.CompleteStageForEasterEgg();
            Assert.That(s.ItemCount(ItemType.Bomb),Is.EqualTo(3));Assert.That(s.LastItemRewards.Sum(),Is.Zero);Assert.That(s.Campaign.itemRewardsClaimed[0],Is.False);
            s.RestartLevel();Assert.That(s.BeginMove(),Is.True);var removal=new ResolutionStep();for(int i=1;i<=100;i++)removal.RemovedIds.Add(i);s.ApplyRemoval(removal);s.CompleteMove();
            Assert.That(s.Progress.Outcome,Is.EqualTo(LevelOutcome.Won));Assert.That(s.ItemCount(ItemType.Hammer),Is.EqualTo(4));Assert.That(s.Campaign.itemRewardsClaimed[0],Is.True);
            Assert.That(board.TryUseItem(ItemType.ExtraMoves),Is.False);
        }
        [UnityTest]
        public IEnumerator FailedSavePreventsItemEffectAndConsumption()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var board=Object.FindFirstObjectByType<BoardController>();int moves=board.Session.Progress.MovesRemaining;
            File.WriteAllText(path,"parent is a file");
            typeof(CampaignState).GetField("savePath",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(CampaignState.Instance,Path.Combine(path,"blocked.json"));
            LogAssert.Expect(LogType.Warning,new Regex("Could not save campaign:"));
            Assert.That(board.TryUseItem(ItemType.ExtraMoves),Is.False);Assert.That(board.Session.ItemCount(ItemType.ExtraMoves),Is.EqualTo(3));Assert.That(board.Session.Progress.MovesRemaining,Is.EqualTo(moves));
            Object.FindFirstObjectByType<PlayerDialogs>().Close();
            typeof(CampaignState).GetField("savePath",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(CampaignState.Instance,path);
            Assert.That(board.ArmItem(ItemType.Hammer),Is.True);yield return null;
            Assert.That(board.SelectedItem,Is.EqualTo(ItemType.Hammer),"An earlier save failure must not cancel a new attempt.");
            Assert.That(board.TryUseItem(ItemType.Hammer,new GridPosition(0,0)),Is.True);yield return Settle(board);
            Assert.That(board.Session.SaveFailed,Is.False);Assert.That(CampaignProgress.Load(path).ItemCount(ItemType.Hammer),Is.EqualTo(2));
        }
        private static IEnumerator Settle(BoardController board)
        {float until=Time.realtimeSinceStartup+15;while(board.IsBusy && Time.realtimeSinceStartup<until)yield return null;Assert.That(board.IsBusy,Is.False);}
    }
}
