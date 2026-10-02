#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.Diagnostics
{
    // Opt-in built-player validation uses an isolated progress file, never the user's save.
    public sealed class CampaignSmokeCheck : MonoBehaviour
    {
        private Mouse mouse;
        private Keyboard keyboard;
        private bool failed;
        private float began;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Has("-puzzleCampaignSmokeTest") || Has("-puzzleCampaignReloadTest") || Has("-puzzleCampaignPreview"))
                new GameObject("CampaignSmokeCheck").AddComponent<CampaignSmokeCheck>();
        }

        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject); Application.runInBackground = true; began = Time.realtimeSinceStartup;
            if (!Check(() => Require(!string.IsNullOrEmpty(Argument("-puzzleProgressPath")), "Use an isolated progress path."))) yield break;
            yield return WaitScene("WorldMap"); if (failed) yield break;
            var map = FindFirstObjectByType<WorldMapView>();
            if (!Check(() => { Require(CampaignState.Instance, "Local campaign was not prepared."); Require(!FindFirstObjectByType<BoardController>(), "Board loaded before map selection."); })) yield break;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            mouse = InputSystem.AddDevice<Mouse>(); keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.EnableDevice(mouse); InputSystem.EnableDevice(keyboard);
            yield return Escape();
            if (!Check(() => Require(FindFirstObjectByType<SettingsPopup>().IsVisible, "ESC did not open map settings."))) yield break;
            yield return Capture("MapSettings.png");
            yield return Escape();
            if (!Check(() => Require(!FindFirstObjectByType<SettingsPopup>().IsVisible && Time.timeScale == 1, "ESC did not close settings."))) yield break;

            if (Has("-puzzleCampaignPreview"))
            {
                for (int world = 1; world <= 5; world++) { map.ShowWorld(world); yield return Capture("WorldMap" + world + ".png"); }
                yield return Capture("MapPortrait.png", 800, 1000);
                map.Enter(1); yield return WaitScene("Game"); if (failed) yield break;
                var session = FindFirstObjectByType<LevelSession>();
                for (int world = 1; world <= 5; world++) { session.SelectLevel((world - 1) * LevelCatalog.StagesPerWorld + 1); yield return Capture("WorldGame" + world + ".png"); }
                Finish(0, "PASS: five world maps and gameplay backgrounds, collection HUD, portrait map and ESC settings captured."); yield break;
            }

            if (Has("-puzzleCampaignReloadTest"))
            {
                if (!Check(() => {
                    Require(CampaignState.Instance.Progress.CompletedCount == 1 && CampaignState.Instance.Progress.UnlockedThrough == 2, "Restart lost progress.");
                    Require(CampaignState.Instance.Progress.bestScores[0] >= CampaignState.Instance.Catalog.Get(1).CreateRules().TargetScore && CampaignState.Instance.Progress.selectedLevel == 2, "Restart lost score/selection.");
                    Require(map.StageButtons[1].interactable && !map.StageButtons[2].interactable, "Restart locks incorrect.");
                })) yield break;
                yield return Capture("RestoredMap.png");
                yield return Click(map.ContinueButton); yield return ConfirmPreview(); yield return WaitScene("Game"); if (failed) yield break;
                if (!Check(() => Require(FindFirstObjectByType<LevelSession>().DisplayName == "1-2", "Continue did not enter 1-2."))) yield break;
                Finish(0, "PASS: independent process restored completed stage 1, best score, selected stage 2; Continue entered 1-2."); yield break;
            }

            if (!Check(() => Require(CampaignState.Instance.Progress.CompletedCount == 0 && map.StageButtons[0].interactable && !map.StageButtons[1].interactable, "Fresh campaign locks incorrect."))) yield break;
            yield return Capture("FreshMap.png");
            yield return Click(map.StageButtons[1]);
            if (!Check(() => Require(SceneManager.GetActiveScene().name == "WorldMap", "Locked stage entered."))) yield break;
            // The visible stone is narrower than this offset; its padded hit area must still work.
            yield return Click(map.StageButtons[0], new Vector2(45, 0)); yield return ConfirmPreview(); yield return WaitScene("Game"); if (failed) yield break;
            var board = FindFirstObjectByType<BoardController>();
            yield return Capture("Stage1-1.png");
            string route = null;
            if (!Check(() => {
                foreach (string line in File.ReadAllLines(Argument("-puzzleWinningRoutes"))) if (line.StartsWith("1|")) route = line.Substring(2);
                Require(route != null, "Missing winning route.");
            })) yield break;
            foreach (string move in route.Split(';'))
            {
                var values = Array.ConvertAll(move.Split(','), int.Parse);
                if (!Check(() => Require(board.TrySwap(new GridPosition(values[0], values[1]), new GridPosition(values[2], values[3])), "Winning route input rejected."))) yield break;
                float until = Time.realtimeSinceStartup + 20;
                while (board.IsBusy && Time.realtimeSinceStartup < until) yield return null;
                if (!Check(() => Require(!board.IsBusy, "Cascade timed out."))) yield break;
            }
            if (!Check(() => {
                Require(board.Session.Progress.Outcome == LevelOutcome.Won, "Route did not win.");
                Require(CampaignProgress.Load(CampaignProgress.SavePath).UnlockedThrough == 2, "Clear was not saved.");
                Require(!board.Session.SaveFailed, "Save failed.");
            })) yield break;
            yield return Capture("Stage1-1Won.png");
            yield return Click(FindFirstObjectByType<ResultPopup>().NextButton);
            if (!Check(() => Require(board.Session.DisplayName == "1-2" && board.Session.Progress.Score == 0, "Next stage failed."))) yield break;
            FindFirstObjectByType<CampaignHUD>().OpenMap(); yield return WaitScene("WorldMap"); if (failed) yield break;
            map = FindFirstObjectByType<WorldMapView>();
            if (!Check(() => Require(map.StageButtons[1].interactable && !map.StageButtons[2].interactable, "Clear did not unlock next node."))) yield break;
            yield return Capture("UnlockedMap.png");
            Finish(0, "PASS: Boot -> map, ESC settings, locked node ignores real mouse, 1-1 mouse selection, actual winning route, save, next stage, unlocked map.");
        }

        private IEnumerator ConfirmPreview()
        {
            yield return null;
            var dialogs = FindFirstObjectByType<PlayerDialogs>();
            if (!Check(() => Require(dialogs && dialogs.IsVisible, "Stage preview did not open."))) yield break;
            yield return Capture("StagePreview.png");
            yield return Click(dialogs.transform.Find("PlayerDialog/Card/Footer/start").GetComponent<Button>());
        }

        private IEnumerator WaitScene(string name)
        {
            float until = Time.realtimeSinceStartup + 30;
            while (SceneManager.GetActiveScene().name != name && Time.realtimeSinceStartup < until) yield return null;
            yield return null; yield return null;
            Check(() => Require(SceneManager.GetActiveScene().name == name, "Scene timeout: " + name));
        }
        private IEnumerator Escape()
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape)); yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null; yield return null;
        }
        private IEnumerator Click(Button button, Vector2 localOffset = default)
        {
            var rect = (RectTransform)button.transform;
            Vector2 point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center + localOffset));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point }); yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left, true)); yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point }); yield return null; yield return null;
        }

        private IEnumerator Capture(string name, int width = 1280, int height = 800)
        {
            string directory = Argument("-puzzleCaptureFolder"); if (string.IsNullOrEmpty(directory)) yield break;
            Directory.CreateDirectory(directory);
            var canvas = FindFirstObjectByType<WorldMapView>()?.GetComponent<Canvas>() ?? FindFirstObjectByType<HUDView>().GetComponent<Canvas>();
            var camera = Camera.main; var target = new RenderTexture(width, height, 24); var picture = new Texture2D(width, height, TextureFormat.RGB24, false);
            var mode = canvas.renderMode; var priorCamera = canvas.worldCamera; float distance = canvas.planeDistance, aspect = camera.aspect;
            var priorTarget = camera.targetTexture; var priorActive = RenderTexture.active; int order = canvas.sortingOrder;
            try
            {
                target.Create(); camera.targetTexture = target; camera.aspect = (float)width / height;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; canvas.sortingOrder = 100;
                Canvas.ForceUpdateCanvases();
                FindFirstObjectByType<CandyLayout>()?.ApplyLayout(); FindFirstObjectByType<BoardView>()?.FitCamera();
                foreach (var graphic in canvas.GetComponentsInChildren<Graphic>(true)) graphic.SetAllDirty();
                Canvas.ForceUpdateCanvases(); yield return null; yield return null;
                Check(() => {
                    RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                    RenderTexture.active = target; picture.ReadPixels(new Rect(0, 0, width, height), 0, 0); picture.Apply();
                    File.WriteAllBytes(Path.Combine(directory, name), picture.EncodeToPNG());
                });
            }
            finally
            {
                RenderTexture.active = priorActive; camera.targetTexture = priorTarget; camera.aspect = aspect;
                canvas.renderMode = mode; canvas.worldCamera = priorCamera; canvas.planeDistance = distance; canvas.sortingOrder = order;
                Canvas.ForceUpdateCanvases(); FindFirstObjectByType<CandyLayout>()?.ApplyLayout(); FindFirstObjectByType<BoardView>()?.FitCamera();
                target.Release(); Destroy(target); Destroy(picture);
            }
        }
        private bool Check(Action action) { try { action(); return true; } catch (Exception e) { failed = true; Finish(1, "FAIL: " + e); return false; } }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static bool Has(string name) => Array.IndexOf(Environment.GetCommandLineArgs(), name) >= 0;
        private static string Argument(string name) { var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, name); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }
        private static void Finish(int code, string message)
        {
            string report = Argument("-puzzleSmokeReport"); if (!string.IsNullOrEmpty(report)) File.WriteAllText(report, message);
            Debug.Log(message); Application.Quit(code);
        }
    }
}
#endif
