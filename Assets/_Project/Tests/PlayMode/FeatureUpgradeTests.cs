using System;
using System.Collections;
using System.IO;
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
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace PuzzleGame.Tests
{
    public sealed class FeatureUpgradeTests
    {
        [SetUp] public void Setup(){Time.timeScale=1;GamePreferences.Load();if(CampaignState.Instance)Object.DestroyImmediate(CampaignState.Instance.gameObject);}
        [TearDown] public void Cleanup(){Object.FindFirstObjectByType<PlayerDialogs>()?.Close();Time.timeScale=1;GamePreferences.Load();}
        [Test]
        public void VersionThreeMigratesStockRewardsAndClearsWithoutInventingMedals()
        {
            string path=Path.Combine(Application.temporaryCachePath,"upgrade-"+Guid.NewGuid()+".json");
            try
            {
                var old=new CampaignProgress{version=3};old.RecordWin(1,1200);old.items[0]=7;old.itemRewardsClaimed[0]=true;
                File.WriteAllText(path,JsonUtility.ToJson(old));var loaded=CampaignProgress.Load(path);
                Assert.That(loaded.version,Is.EqualTo(4));Assert.That(loaded.StarsAt(1),Is.EqualTo(1));
                Assert.That(loaded.cleanMedals[0]||loaded.efficientMedals[0],Is.False);
                Assert.That(loaded.items[0],Is.EqualTo(7));Assert.That(loaded.itemRewardsClaimed[0],Is.True);
                var win=new LevelProgress(new LevelRules(20,100,10));win.CompleteImmediately();loaded.RecordRating(1,win,0);
                Assert.That(loaded.Save(path),Is.True);var again=CampaignProgress.Load(path);
                Assert.That(again.TotalStars,Is.EqualTo(3));Assert.That(again.cleanMedals[0]&&again.efficientMedals[0],Is.True);
                loaded.RecordRating(1,win,1);Assert.That(loaded.StarsAt(1),Is.EqualTo(3));
            }
            finally{foreach(string file in new[]{path,path+".bak",path+".tmp"})if(File.Exists(file))File.Delete(file);}
        }
        [Test]
        public void OldSettingsGainHintAndSpeedDefaultsAndInvalidNewSettingsAreRejected()
        {
            string path=Path.Combine(Application.temporaryCachePath,"settings-upgrade-"+Guid.NewGuid()+".json");
            try
            {
                File.WriteAllText(path,"{\"version\":1,\"width\":1280,\"height\":800,\"windowed\":true,\"language\":\"ko\"}");
                var loaded=DisplaySettings.Load(path);Assert.That(loaded,Is.Not.Null);Assert.That(loaded.autoHints,Is.True);
                Assert.That(loaded.hintDelay,Is.EqualTo(6));Assert.That(loaded.animationSpeed,Is.EqualTo(1));
                loaded.autoHints=false;loaded.hintDelay=15;loaded.animationSpeed=2;Assert.That(loaded.Save(path),Is.True);
                Assert.That(DisplaySettings.Load(path).animationSpeed,Is.EqualTo(2));Assert.That(DisplaySettings.Load(path).autoHints,Is.False);
                loaded.animationSpeed=float.NaN;Assert.That(loaded.IsValid,Is.False);
            }
            finally{foreach(string file in new[]{path,path+".bak",path+".tmp"})if(File.Exists(file))File.Delete(file);}
        }
        [UnityTest]
        public IEnumerator ItemCommitAffectsRatingButCancelledItemAndPClearDoNotEarnMedals()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var board=Object.FindFirstObjectByType<BoardController>();var session=board.Session;
            Assert.That(board.TryUseItem(ItemType.Hammer,new GridPosition(0,0)),Is.True);session.RestartLevel();
            Assert.That(session.ItemsUsed,Is.Zero);Assert.That(session.ItemCount(ItemType.Hammer),Is.EqualTo(3));
            Assert.That(board.TryUseItem(ItemType.ExtraMoves),Is.True);Assert.That(session.ItemsUsed,Is.EqualTo(1));
            board.CompleteStageForEasterEgg();Assert.That(session.EarnedStars,Is.Zero);Assert.That(session.Campaign.TotalStars,Is.Zero);
            Assert.That(session.Campaign.cleanMedals[0],Is.False);Assert.That(session.Campaign.itemRewardsClaimed[0],Is.False);
            session.RestartLevel();session.BeginMove();var step=new ResolutionStep();for(int i=1;i<=60;i++)step.RemovedIds.Add(i);
            session.ApplyRemoval(step);session.CompleteMove();Assert.That(session.EarnedStars,Is.EqualTo(3));
            Assert.That(session.Campaign.cleanMedals[0],Is.True);Assert.That(session.NewBest,Is.True);
        }
        [UnityTest]
        public IEnumerator ManualHintWorksWithAutoHintsOffAndAbandonCanBeCancelled()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var board=Object.FindFirstObjectByType<BoardController>();var guidance=board.GetComponent<BoardGuidance>();
            GamePreferences.Current.autoHints=false;GamePreferences.Save();guidance.HintDelay=.01f;
            yield return new WaitForSeconds(.04f);Assert.That(guidance.HintVisible,Is.False);
            int moves=board.Session.Progress.MovesRemaining;Assert.That(guidance.RequestHint(),Is.True);Assert.That(guidance.HintVisible,Is.True);
            Assert.That(board.Session.Progress.MovesRemaining,Is.EqualTo(moves));
            Assert.That(board.TryUseItem(ItemType.ExtraMoves),Is.True);
            var hud=Object.FindFirstObjectByType<HUDView>();hud.RestartButton.onClick.Invoke();var dialogs=hud.GetComponent<PlayerDialogs>();
            Assert.That(dialogs.IsVisible,Is.True);Assert.That(board.IsPaused,Is.True);
            dialogs.transform.Find("PlayerDialog/Card/Footer/cancel").GetComponent<Button>().onClick.Invoke();
            Assert.That(board.Session.Progress.MovesRemaining,Is.EqualTo(moves+5));Assert.That(board.IsPaused,Is.False);
            hud.RestartButton.onClick.Invoke();dialogs.transform.Find("PlayerDialog/Card/Footer/restartAction").GetComponent<Button>().onClick.Invoke();
            Assert.That(board.Session.Progress.MovesRemaining,Is.EqualTo(moves));Assert.That(board.Session.ItemCount(ItemType.ExtraMoves),Is.EqualTo(2));
        }
        [UnityTest]
        public IEnumerator UnifiedTabsKeepOnePauseAndDisplayApplySurvivesTabClicks()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var settings=Object.FindFirstObjectByType<SettingsPopup>();var dialogs=settings.GetComponent<PlayerDialogs>();settings.Open();yield return null;
            const string root="PlayerDialog/Card/Viewport/Content/";
            Assert.That(dialogs.DisplayResolution,Is.Not.Null);Assert.That(settings.IsVisible,Is.True);
            settings.Apply();dialogs.transform.Find(root+"Tabs/audioTab").GetComponent<Button>().onClick.Invoke();
            Assert.That(dialogs.DisplayResolution,Is.Not.Null);dialogs.Close();Assert.That(dialogs.IsVisible,Is.True);
            float deadline=Time.realtimeSinceStartup+5;while(settings.IsApplying&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(settings.IsApplying,Is.False);
            dialogs.transform.Find(root+"Tabs/experienceTab").GetComponent<Button>().onClick.Invoke();yield return null;
            dialogs.transform.Find(root+"AutoHints/Switch").GetComponent<Toggle>().isOn=false;
            dialogs.transform.Find(root+"AnimationSpeed/Choices/2").GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.That(GamePreferences.Current.animationSpeed,Is.EqualTo(2));Assert.That(GamePreferences.Current.autoHints,Is.False);
            foreach(string name in new[]{"HintDelay","AnimationSpeed","Language"})
            {
                var panel=(RectTransform)dialogs.transform.Find(root+name);
                panel.GetComponent<LayoutElement>().ignoreLayout=true;
                foreach(float width in new[]{380f,540f})
                {
                    panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
                    var row=(RectTransform)panel.Find("Choices");var bounds=new Vector3[4];row.GetWorldCorners(bounds);
                    foreach(var button in row.GetComponentsInChildren<Button>())
                    {
                        var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
                        Assert.That(corners[0].x,Is.GreaterThanOrEqualTo(bounds[0].x-.5f),name+" left choice is clipped.");
                        Assert.That(corners[2].x,Is.LessThanOrEqualTo(bounds[2].x+.5f),name+" right choice is clipped.");
                    }
                }
            }
            Assert.That(Time.timeScale,Is.Zero);settings.Close();Assert.That(Time.timeScale,Is.EqualTo(1));
        }
        [UnityTest]
        public IEnumerator FailureDetailsReportTheActualMissingGoals()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var session=Object.FindFirstObjectByType<LevelSession>();session.StartWithRules(new LevelRules(1,100,10,new[]{2,0,0,0,0,0},3));
            session.BeginMove();session.CompleteMove();yield return null;
            var popup=Object.FindFirstObjectByType<ResultPopup>();Assert.That(popup.IsVisible,Is.True);
            Assert.That(popup.GoalSummary,Does.Contain(Localization.Get("goalShort",2)));
            Assert.That(popup.GoalSummary,Does.Contain(Localization.Get("goalShort",3)));
            Assert.That(popup.GoalSummary,Does.Contain(Localization.Get("goalShort",100)));
        }
    }
}
