#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Startup;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.Diagnostics
{
    public sealed class LoadingSmokeCheck : MonoBehaviour
    {
        private bool failed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-puzzleLoadingSmokeTest") >= 0)
                new GameObject("LoadingSmokeCheck").AddComponent<LoadingSmokeCheck>();
        }

        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject); Application.runInBackground = true;
            float began = Time.realtimeSinceStartup;
            yield return null; yield return null;
            var loading = FindFirstObjectByType<StartupLoadingScreen>();
            if (!Check(() => {
                if (!loading || !loading.IsLoading || SceneManager.GetActiveScene().name != "Boot")
                    throw new InvalidOperationException("Player did not start on the loading screen.");
                if (FindFirstObjectByType<BoardController>()) throw new InvalidOperationException("Board ran before loading completed.");
            })) yield break;
            yield return Capture(loading.View, "LoadingEarly.png", 1280, 720);
            // Real local work may finish in a fraction of a second. Never require a fake delay
            // or hold a destroyed loading view merely to capture additional aspect ratios.
            while (SceneManager.GetActiveScene().name != "WorldMap" && Time.realtimeSinceStartup - began < 25) yield return null;
            float duration = Time.realtimeSinceStartup - began;
            yield return null; yield return null;
            if (!Check(() => {
                var map = FindFirstObjectByType<WorldMapView>();
                if (!map || FindFirstObjectByType<BoardController>()) throw new InvalidOperationException("Loading did not open the roadmap.");
                map.Enter(1);
            })) yield break;
            while (SceneManager.GetActiveScene().name != "Game" && Time.realtimeSinceStartup - began < 30) yield return null;
            yield return null; yield return null;
            if (!Check(() => {
                var board = FindFirstObjectByType<BoardController>();
                if (SceneManager.GetActiveScene().name != "Game" || !board || !board.CanSelect(new GridPosition(0, 0)))
                    throw new InvalidOperationException("Loading did not enter a playable board.");
                if (duration > 20) throw new InvalidOperationException("Unexpected loading duration: " + duration);
                if (FindFirstObjectByType<StartupLoadingScreen>()) throw new InvalidOperationException("Loading UI survived game entry.");
                board.Session.RestartLevel();
                if (board.Session.Progress.MovesRemaining != 20 || board.Session.Progress.Score != 0)
                    throw new InvalidOperationException("Restart did not preserve normal gameplay behavior.");
                var settings = FindFirstObjectByType<SettingsPopup>(); settings.Open();
                if (!settings.IsVisible || !board.IsPaused) throw new InvalidOperationException("Settings failed after startup.");
                settings.Close();
            })) yield break;
            if (failed) yield break;
            Finish(0, $"PASS: Boot first, local data ready, no early gameplay, {duration:F2}s until WorldMap, selection enters Game, restart and settings after loading.");
        }

        private IEnumerator Capture(LoadingScreenView view, string name, int width, int height)
        {
            string directory = Argument("-puzzleCaptureFolder");
            if (string.IsNullOrEmpty(directory)) yield break;
            Directory.CreateDirectory(directory);
            var canvas = view.GetComponent<Canvas>(); var camera = Camera.main;
            var previousMode = canvas.renderMode; var previousCamera = canvas.worldCamera;
            float distance = canvas.planeDistance, aspect = camera.aspect;
            var previousTarget = camera.targetTexture; var previousActive = RenderTexture.active;
            var target = new RenderTexture(width, height, 24);
            var picture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                target.Create(); camera.targetTexture = target; camera.aspect = (float)width / height;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                Canvas.ForceUpdateCanvases(); view.ApplyLayout();
                foreach (var graphic in canvas.GetComponentsInChildren<Graphic>(true)) graphic.SetAllDirty();
                Canvas.ForceUpdateCanvases();
                yield return null;
                Check(() => {
                    RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                    RenderTexture.active = target; picture.ReadPixels(new Rect(0, 0, width, height), 0, 0); picture.Apply();
                    File.WriteAllBytes(Path.Combine(directory, name), picture.EncodeToPNG());
                });
            }
            finally
            {
                RenderTexture.active = previousActive;
                if (camera) { camera.targetTexture = previousTarget; camera.aspect = aspect; }
                if (canvas) { canvas.renderMode = previousMode; canvas.worldCamera = previousCamera; canvas.planeDistance = distance; }
                if (view) { Canvas.ForceUpdateCanvases(); view.ApplyLayout(); }
                target.Release(); Destroy(target); Destroy(picture);
            }
        }

        private bool Check(Action action)
        {
            try { action(); return true; }
            catch (Exception error) { failed = true; Finish(1, "FAIL: " + error); return false; }
        }

        private static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }

        private static void Finish(int code, string report)
        {
            string path = Argument("-puzzleSmokeReport");
            if (!string.IsNullOrEmpty(path)) File.WriteAllText(path, report);
            Debug.Log(report); Application.Quit(code);
        }
    }
}
#endif
