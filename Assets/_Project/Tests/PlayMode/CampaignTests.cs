using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PuzzleGame.Tests
{
    public class CampaignTests
    {
        private string directory;

        [SetUp]
        public void Setup()
        {
            directory = Path.Combine(Application.temporaryCachePath, "CampaignTests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            if (CampaignState.Instance) Object.DestroyImmediate(CampaignState.Instance.gameObject);
            Time.timeScale = 1;
        }

        [TearDown]
        public void Cleanup()
        {
            if (CampaignState.Instance) Object.DestroyImmediate(CampaignState.Instance.gameObject);
            Directory.Delete(directory, true);
            Time.timeScale = 1;
        }

        [Test]
        public void SaveReloadKeepsClearsBestScoreAndSelection()
        {
            var progress = new CampaignProgress();
            Assert.That(progress.RecordWin(2, 9999), Is.False);
            Assert.That(progress.RecordWin(1, 1500), Is.True);
            progress.RecordWin(1, 1000); progress.selectedLevel = 2;
            string path = Path.Combine(directory, "progress.json");
            Assert.That(progress.Save(path), Is.True);
            var loaded = CampaignProgress.Load(path);
            Assert.That(loaded.UnlockedThrough, Is.EqualTo(2));
            Assert.That(loaded.bestScores[0], Is.EqualTo(1500));
            Assert.That(loaded.selectedLevel, Is.EqualTo(2));
            Assert.That(loaded.CompletedCount, Is.EqualTo(1));
        }

        [Test]
        public void CorruptPrimaryRecoversBackupAndBothCorruptStartFresh()
        {
            string path = Path.Combine(directory, "progress.json");
            var progress = new CampaignProgress(); progress.RecordWin(1, 1000); progress.Save(path);
            progress.RecordWin(2, 1200); progress.Save(path);
            File.WriteAllText(path, "{broken");
            Assert.That(CampaignProgress.Load(path).UnlockedThrough, Is.EqualTo(2));
            File.WriteAllText(path + ".bak", "{broken");
            Assert.That(CampaignProgress.Load(path).UnlockedThrough, Is.EqualTo(1));
        }

        [Test]
        public void InvalidVersionAndGappedClearsAreRejected()
        {
            string path = Path.Combine(directory, "progress.json");
            var progress = new CampaignProgress { version = 99 };
            File.WriteAllText(path, JsonUtility.ToJson(progress));
            Assert.That(CampaignProgress.Load(path).version, Is.EqualTo(2));
            progress.version = 2; progress.completed[25] = true;
            File.WriteAllText(path, JsonUtility.ToJson(progress));
            Assert.That(CampaignProgress.Load(path).CompletedCount, Is.Zero);
        }

        [Test]
        public void WorldBoundaryAndFinalStageNeverUnlock51()
        {
            var progress = new CampaignProgress();
            for (int i = 1; i <= 10; i++) Assert.That(progress.RecordWin(i, 2000), Is.True);
            Assert.That(progress.UnlockedThrough, Is.EqualTo(11));
            for (int i = 11; i <= 50; i++) Assert.That(progress.RecordWin(i, 2000), Is.True);
            Assert.That(progress.CompletedCount, Is.EqualTo(50));
            Assert.That(progress.UnlockedThrough, Is.EqualTo(50));
            Assert.That(progress.RecordWin(51, 2000), Is.False);
        }

        [TestCase(0, 0)]
        [TestCase(9, 9)]
        [TestCase(10, 10)]
        [TestCase(19, 10)]
        [TestCase(25, 15)]
        [TestCase(99, 50)]
        [TestCase(100, 50)]
        public void OriginalTwentyStageWorldsMigrateWithoutLosingRetainedClears(int oldClears, int newClears)
        {
            var old = new CampaignProgress { version = 1, completed = new bool[100], bestScores = new int[100], selectedLevel = Math.Min(100, oldClears + 1) };
            for (int i = 0; i < oldClears; i++) { old.completed[i] = true; old.bestScores[i] = 1000 + i; }
            string path = Path.Combine(directory, "progress.json");
            File.WriteAllText(path, JsonUtility.ToJson(old));
            var migrated = CampaignProgress.Load(path);
            Assert.That(migrated.version, Is.EqualTo(2));
            Assert.That(migrated.completed.Length, Is.EqualTo(50));
            Assert.That(migrated.CompletedCount, Is.EqualTo(newClears));
            Assert.That(migrated.IsUnlocked(migrated.selectedLevel), Is.True);
            if (newClears > 10) Assert.That(migrated.bestScores[10], Is.EqualTo(1020));
            Assert.That(migrated.Save(path), Is.True);
            Assert.That(CampaignProgress.Load(path).CompletedCount, Is.EqualTo(newClears));
        }

        [UnityTest]
        public IEnumerator MapLocksStagesAndSelectionEntersFirstStage()
        {
            yield return SceneManager.LoadSceneAsync("WorldMap"); yield return null;
            var map = Object.FindFirstObjectByType<WorldMapView>();
            Assert.That(map.StageButtons.Length, Is.EqualTo(10));
            Assert.That(map.StageButtons[0].interactable, Is.True);
            Assert.That(map.StageButtons[1].interactable, Is.False);
            map.Enter(2); yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("WorldMap"));
            map.ShowWorld(5);
            foreach (var button in map.StageButtons) Assert.That(button.interactable, Is.False);
            var settings = Object.FindFirstObjectByType<SettingsPopup>(); settings.Open();
            Assert.That(settings.IsVisible, Is.True); settings.Close();
            map.ShowWorld(1); map.StageButtons[0].onClick.Invoke();
            yield return WaitForGame();
            var session = Object.FindFirstObjectByType<LevelSession>();
            Assert.That(session.DisplayName, Is.EqualTo("1-1"));
            Assert.That(session.SelectLevel(2), Is.False);
            session.BeginMove();
            var step = new ResolutionStep(); for (int i = 1; i <= 100; i++) step.RemovedIds.Add(i);
            session.ApplyRemoval(step); session.CompleteMove();
            Assert.That(session.Campaign.UnlockedThrough, Is.EqualTo(2));
            var popup = Object.FindFirstObjectByType<ResultPopup>();
            Assert.That(popup.NextButton.gameObject.activeInHierarchy, Is.True);
            popup.NextButton.onClick.Invoke();
            Assert.That(session.DisplayName, Is.EqualTo("1-2"));
            Assert.That(session.Progress.Score, Is.Zero);
            Object.FindFirstObjectByType<CampaignHUD>().OpenMap();
            float until = Time.realtimeSinceStartup + 10;
            while (SceneManager.GetActiveScene().name != "WorldMap" && Time.realtimeSinceStartup < until) yield return null;
            yield return null;
            map = Object.FindFirstObjectByType<WorldMapView>();
            Assert.That(map.StageButtons[1].interactable, Is.True);
            Assert.That(map.StageButtons[2].interactable, Is.False);
        }

        [UnityTest]
        public IEnumerator CatalogDifficultyAndFinalResultUseAllGoals()
        {
            yield return SceneManager.LoadSceneAsync("Game"); yield return null;
            var session = Object.FindFirstObjectByType<LevelSession>();
            float previous = 0;
            for (int i = 1; i <= 50; i++)
            {
                var level = session.Catalog.Get(i); var rules = level.CreateRules();
                Assert.That(level.DisplayName, Is.EqualTo(((i - 1) / 10 + 1) + "-" + ((i - 1) % 10 + 1)));
                float pressure = (float)rules.TargetScore / rules.StartingMoves;
                Assert.That(pressure, Is.GreaterThan(previous)); previous = pressure;
                Assert.That(session.Catalog.Background(level.World), Is.Not.Null);
                if (i < 50) session.Campaign.RecordWin(i, 2000);
            }
            Assert.That(session.SelectLevel(50), Is.True);
            Assert.That(session.BeginMove(), Is.True);
            var step = new ResolutionStep();
            for (int i = 1; i <= 300; i++) { step.RemovedIds.Add(i); step.RemovedPieces.Add(new RemovedPiece(i, (PieceColor)(i % 6 + 1))); }
            session.ApplyRemoval(step); session.CompleteMove();
            Assert.That(session.Progress.Outcome, Is.EqualTo(LevelOutcome.Won));
            Assert.That(session.Campaign.CompletedCount, Is.EqualTo(50));
            Assert.That(Object.FindFirstObjectByType<ResultPopup>().NextButton.gameObject.activeSelf, Is.False);
            Assert.That(session.NextLevel(), Is.False);
        }

        private static IEnumerator WaitForGame()
        {
            float until = Time.realtimeSinceStartup + 10;
            while (SceneManager.GetActiveScene().name != "Game" && Time.realtimeSinceStartup < until) yield return null;
            yield return null; yield return null;
            Assert.That(Object.FindFirstObjectByType<BoardController>()?.Model, Is.Not.Null);
        }
    }
}
