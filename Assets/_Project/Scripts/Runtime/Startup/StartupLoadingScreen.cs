using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace PuzzleGame.Runtime.Startup
{
    public sealed class StartupLoadingScreen : MonoBehaviour
    {
        [SerializeField] private StartupDataLoader dataLoader;
        [SerializeField] private LoadingScreenView view;
        [SerializeField] private string gameScene = "WorldMap";
        private CancellationTokenSource cancellation;
        public bool IsLoading { get; private set; }
        public bool HasFailed { get; private set; }
        public LoadingScreenView View => view;
        public StartupDataLoader DataLoader => dataLoader;
        public void ConfigureDestination(string sceneName) => gameScene = sceneName;

        public void Configure(StartupDataLoader loader, LoadingScreenView loadingView)
        { dataLoader = loader; view = loadingView; }

        private void Awake()
        {
            view.ResetLoading(); view.ApplyLayout();
        }

        private void OnEnable()
        {
            view.RetryButton.onClick.AddListener(Retry);
            view.QuitButton.onClick.AddListener(Quit);
        }

        private void Start() => Retry();

        public void Retry()
        {
            if (IsLoading || !isActiveAndEnabled) return;
            cancellation?.Cancel();
            cancellation?.Dispose();
            cancellation = new CancellationTokenSource();
            HasFailed = false; IsLoading = true; view.ResetLoading();
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
            StartCoroutine(Load(cancellation.Token));
        }

        private IEnumerator Load(CancellationToken token)
        {
            // Give the artwork a frame to render before beginning any startup work.
            yield return null;
            Task task = null;
            Exception failure = null;
            try
            {
                if (!dataLoader) throw new InvalidOperationException("No startup data loader is configured.");
                var progress = new Progress<float>(value => {
                    if (this && IsLoading && !token.IsCancellationRequested && !float.IsNaN(value))
                        view.SetProgress(Mathf.Max(view.Progress, Mathf.Clamp01(value) * .9f));
                });
                task = dataLoader.LoadAsync(progress, token);
                if (task == null) throw new InvalidOperationException("Startup loader returned no task.");
            }
            catch (Exception error) { failure = error; }
            if (failure != null) { Fail(failure); yield break; }
            // Always observe faults, including a provider that faults after this scene is closed.
            _ = task.ContinueWith(completed => { _ = completed.Exception; },
                CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
            while (!task.IsCompleted) yield return null;
            if (token.IsCancellationRequested) yield break;
            if (task.IsFaulted || task.IsCanceled)
            {
                Fail(task.Exception?.GetBaseException() ?? new OperationCanceledException("Startup was cancelled."));
                yield break;
            }

            AsyncOperation sceneLoad = null;
            try
            {
                if (!Application.CanStreamedLevelBeLoaded(gameScene))
                    throw new InvalidOperationException("Game scene is not in the build scene list.");
                view.SetProgress(.9f);
                sceneLoad = SceneManager.LoadSceneAsync(gameScene, LoadSceneMode.Single);
                if (sceneLoad == null) throw new InvalidOperationException("Game scene could not be loaded.");
            }
            catch (Exception error) { failure = error; }
            if (failure != null) { Fail(failure); yield break; }
            while (!sceneLoad.isDone)
            {
                view.SetProgress(.9f + .1f * Mathf.Clamp01(sceneLoad.progress / .9f));
                yield return null;
            }
        }

        private void Fail(Exception error)
        {
            IsLoading = false; HasFailed = true; view.ShowFailure();
            Debug.LogWarning("Startup loading failed: " + error.Message);
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(view.RetryButton.gameObject);
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDisable()
        {
            IsLoading = false;
            cancellation?.Cancel(); cancellation?.Dispose(); cancellation = null;
            StopAllCoroutines();
            if (view)
            {
                view.RetryButton.onClick.RemoveListener(Retry);
                view.QuitButton.onClick.RemoveListener(Quit);
            }
        }
    }
}
