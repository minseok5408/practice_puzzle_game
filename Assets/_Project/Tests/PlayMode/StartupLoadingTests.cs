using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Startup;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PuzzleGame.Tests
{
    public class ControlledStartupLoader : StartupDataLoader
    {
        public TaskCompletionSource<bool> Completion = new TaskCompletionSource<bool>();
        public IProgress<float> Progress;
        public CancellationToken Token;
        public int Calls;
        public override Task LoadAsync(IProgress<float> progress, CancellationToken cancellationToken)
        { Calls++; Progress = progress; Token = cancellationToken; return Completion.Task; }
    }

    public class StartupLoadingTests
    {
        private StartupLoadingScreen screen;
        private ControlledStartupLoader loader;

        private IEnumerator Boot(bool controlled)
        {
            Time.timeScale = 1;
            void Loaded(Scene scene, LoadSceneMode mode)
            {
                if (scene.name != "Boot") return;
                screen = Object.FindFirstObjectByType<StartupLoadingScreen>();
                if (!controlled) return;
                loader = screen.gameObject.AddComponent<ControlledStartupLoader>();
                screen.Configure(loader, screen.View);
            }
            SceneManager.sceneLoaded += Loaded;
            try { yield return SceneManager.LoadSceneAsync("Boot", LoadSceneMode.Single); }
            finally { SceneManager.sceneLoaded -= Loaded; }
            yield return null; yield return null;
            Assert.That(screen.IsLoading, Is.True);
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Time.timeScale = 1;
            if (screen) screen.enabled = false;
            var empty = SceneManager.CreateScene("LoadingTestCleanup");
            SceneManager.SetActiveScene(empty);
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name == "Boot" || scene.name == "Game" || scene.name == "WorldMap") yield return SceneManager.UnloadSceneAsync(scene);
            }
        }

        [UnityTest]
        public IEnumerator DefaultPrototypeWaitsFiveRealSecondsAndRestartDoesNotReload()
        {
            float began = Time.realtimeSinceStartup;
            yield return Boot(false);
            Assert.That(((PrototypeStartupLoader)screen.DataLoader).DurationSeconds, Is.EqualTo(5));
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(screen.View.Progress, Is.GreaterThan(0));
            Assert.That(Object.FindFirstObjectByType<BoardController>(), Is.Null);
            yield return WaitForMap();
            Assert.That(Time.realtimeSinceStartup - began, Is.GreaterThanOrEqualTo(5));
            Time.timeScale = 1;
            Object.FindFirstObjectByType<WorldMapView>().Enter(1);
            float gameDeadline = Time.realtimeSinceStartup + 10;
            while (SceneManager.GetActiveScene().name != "Game" && Time.realtimeSinceStartup < gameDeadline) yield return null;
            yield return null; yield return null;
            var board = Object.FindFirstObjectByType<BoardController>();
            Assert.That(board.CanSelect(new GridPosition(0, 0)), Is.True);
            board.Session.RestartLevel(); yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Game"));
            Assert.That(Object.FindFirstObjectByType<StartupLoadingScreen>(), Is.Null);
            Assert.That(board.Session.Progress.MovesRemaining, Is.EqualTo(20));
            var settings = Object.FindFirstObjectByType<SettingsPopup>();
            settings.Open(); Assert.That(settings.IsVisible, Is.True); settings.Close();
        }

        [UnityTest]
        public IEnumerator FastProviderCanEnterTheGameBeforeFiveSeconds()
        {
            yield return Boot(true);
            float began = Time.realtimeSinceStartup;
            loader.Completion.SetResult(true);
            yield return WaitForMap();
            Assert.That(Time.realtimeSinceStartup - began, Is.LessThan(5));
        }

        [UnityTest]
        public IEnumerator FullProgressDoesNotBypassASlowerProvidersCompletion()
        {
            yield return Boot(true);
            loader.Progress.Report(1);
            yield return new WaitForSecondsRealtime(5.15f);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Boot"));
            Assert.That(screen.View.Progress, Is.EqualTo(.9f).Within(.001f));
            Assert.That(Object.FindFirstObjectByType<BoardController>(), Is.Null);
            loader.Completion.SetResult(true);
            yield return WaitForMap();
        }

        [UnityTest]
        public IEnumerator FailureKeepsLoadingScreenAndRetryWaitsForNewData()
        {
            yield return Boot(true);
            loader.Completion.SetException(new InvalidOperationException("profile unavailable"));
            yield return null; yield return null;
            Assert.That(screen.HasFailed, Is.True);
            Assert.That(screen.View.StatusText, Does.Contain("다시 시도"));
            Assert.That(screen.View.RetryButton.gameObject.activeInHierarchy, Is.True);
            Assert.That(Object.FindFirstObjectByType<BoardController>(), Is.Null);
            var previousProgress = loader.Progress;
            loader.Completion = new TaskCompletionSource<bool>();
            screen.View.RetryButton.onClick.Invoke();
            yield return null; yield return null;
            Assert.That(loader.Calls, Is.EqualTo(2));
            Assert.That(screen.HasFailed, Is.False);
            previousProgress.Report(1);
            yield return null; yield return null;
            Assert.That(screen.View.Progress, Is.Zero);
            loader.Completion.SetResult(true);
            yield return WaitForMap();
        }

        [UnityTest]
        public IEnumerator ClosingStartupCancelsPendingWorkAndIgnoresLateCompletion()
        {
            yield return Boot(true);
            var token = loader.Token;
            var completion = loader.Completion;
            var progress = loader.Progress;
            screen.gameObject.SetActive(false);
            Assert.That(token.IsCancellationRequested, Is.True);
            progress.Report(.75f); completion.SetResult(true);
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Boot"));
            Assert.That(Object.FindFirstObjectByType<BoardController>(), Is.Null);
        }

        private static IEnumerator WaitForMap()
        {
            float deadline = Time.realtimeSinceStartup + 20;
            while (SceneManager.GetActiveScene().name != "WorldMap" && Time.realtimeSinceStartup < deadline) yield return null;
            yield return null; yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("WorldMap"));
            Assert.That(Object.FindFirstObjectByType<WorldMapView>(), Is.Not.Null);
        }
    }
}
