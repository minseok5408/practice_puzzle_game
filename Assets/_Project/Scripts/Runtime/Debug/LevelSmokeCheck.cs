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
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace PuzzleGame.Runtime.Diagnostics
{
    public sealed class LevelSmokeCheck : MonoBehaviour
    {
        private BoardController board;
        private LevelSession session;
        private HUDView hud;
        private ResultPopup popup;
        private Mouse mouse;
        private bool failed;
        private Keyboard keyboard;
        private bool expectSettingsQuit;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-puzzleLevelSmokeTest") >= 0 ||
                Array.IndexOf(Environment.GetCommandLineArgs(), "-puzzleSpecialSmokeTest") >= 0 ||
                Array.IndexOf(Environment.GetCommandLineArgs(), "-puzzleSettingsSmokeTest") >= 0 ||
                Array.IndexOf(Environment.GetCommandLineArgs(), "-puzzleSettingsReloadTest") >= 0)
                new GameObject("LevelSmokeCheck").AddComponent<LevelSmokeCheck>();
        }

        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
            float startupDeadline = Time.realtimeSinceStartup + 30;
            while (!FindFirstObjectByType<BoardController>() && Time.realtimeSinceStartup < startupDeadline)
            {
                var map = FindFirstObjectByType<PuzzleGame.Runtime.UI.WorldMapView>();
                if (map) map.Enter(1);
                yield return null;
            }
            yield return null; yield return null;
            if (!Check(() => {
                board = FindFirstObjectByType<BoardController>();
                session = board.Session;
                hud = FindFirstObjectByType<HUDView>();
                popup = FindFirstObjectByType<ResultPopup>();
                if (session.Progress.MovesRemaining != session.Definition.CreateRules().StartingMoves || session.Progress.Score != 0 || session.Progress.Rules.TargetScore != session.Definition.CreateRules().TargetScore)
                    throw new InvalidOperationException("Initial level rules are incorrect.");
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                mouse = InputSystem.AddDevice<Mouse>();
                InputSystem.EnableDevice(mouse);
            })) yield break;
            yield return Capture("LevelPlaying.png");
            if (failed) yield break;

            bool settingsRun = Array.IndexOf(Environment.GetCommandLineArgs(), "-puzzleSettingsSmokeTest") >= 0;
            bool settingsReload = Array.IndexOf(Environment.GetCommandLineArgs(), "-puzzleSettingsReloadTest") >= 0;
            if (settingsRun || settingsReload)
            {
                keyboard = InputSystem.AddDevice<Keyboard>();
                yield return CheckSettings(settingsReload);
                if (failed) yield break;
                expectSettingsQuit = true;
                yield return ClickButton(FindFirstObjectByType<SettingsPopup>().QuitButton);
                yield return new WaitForSecondsRealtime(3);
                Finish(1,"FAIL: the actual settings Quit button did not exit the player.");
                yield break;
            }

            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-puzzleSpecialSmokeTest") >= 0)
            {
                yield return CheckSpecialPieces();
                if (failed) yield break;
            }

            foreach (bool winning in new[] { true, false })
            {
                session.StartWithRules(new LevelRules(1, winning ? 30 : 100000, 10));
                yield return null;
                MoveFinder.TryFindMove(board.Model, out GridPosition a, out GridPosition b);
                BoardView view = board.GetComponent<BoardView>();
                Vector2 start = view.BoardCamera.WorldToScreenPoint(view.transform.TransformPoint(view.CellToLocal(a)));
                Vector2 end = view.BoardCamera.WorldToScreenPoint(view.transform.TransformPoint(view.CellToLocal(b)));
                QueueMouse(start, true);
                yield return null; yield return null;
                if (winning)
                {
                    QueueMouse(Vector2.Lerp(start, end, .55f), true);
                    yield return null; yield return null;
                    yield return Capture("CandyDragging.png");
                    if (failed) yield break;
                }
                QueueMouse(end, true);
                yield return null; yield return null;
                QueueMouse(end, false);
                yield return null; yield return null;
                float deadline = Time.realtimeSinceStartup + 20;
                while (board.IsBusy && Time.realtimeSinceStartup < deadline)
                {
                    if (!Check(() => {
                        if (popup.IsVisible) throw new InvalidOperationException("Result appeared before the cascade ended.");
                    })) yield break;
                    yield return null;
                }
                if (!Check(() => {
                    LevelOutcome expected = winning ? LevelOutcome.Won : LevelOutcome.Lost;
                    if (board.IsBusy || session.Progress.Outcome != expected || session.Progress.MovesRemaining != 0
                        || session.Progress.Score < 30 || !popup.IsVisible || board.CanSelect(a))
                        throw new InvalidOperationException("Outcome, score, moves, or input lock failed.");
                    if (MatchFinder.FindMatches(board.Model).Count != 0 || view.PieceCount != 64)
                        throw new InvalidOperationException("Result board is not stable.");
                })) yield break;
                yield return new WaitForSecondsRealtime(.18f);
                if(winning)yield return Capture("VictoryEntering.png");
                yield return new WaitForSecondsRealtime(1.1f);
                yield return Capture(winning ? "LevelWon.png" : "LevelLost.png");
                if (failed) yield break;
                Vector2 button = RectTransformUtility.WorldToScreenPoint(null, popup.RestartButton.transform.position);
                QueueMouse(button, true);
                yield return null; yield return null;
                QueueMouse(button, false);
                yield return null; yield return null;
                if (!Check(() => {
                    if (popup.IsVisible || session.Progress.Score != 0 || session.Progress.MovesRemaining != 1
                        || session.Progress.IsFinished || !board.CanSelect(a))
                        throw new InvalidOperationException("Restart button did not reset the level.");
                })) yield break;
            }
            Finish(0, "PASS: Korean HUD, mouse drag, score, last-move win, loss, result input lock, real restart button twice."
                + (Array.IndexOf(Environment.GetCommandLineArgs(), "-puzzleSpecialSmokeTest") >= 0
                    ? " Special candy sprites and color-clear mouse drag without a match also passed." : ""));
        }

        private IEnumerator CheckSettings(bool reload)
        {
            var settings=FindFirstObjectByType<SettingsPopup>();
            yield return ClickButton(settings.OpenButton);
            if (!Check(() => {
                if (!settings.IsVisible || !board.IsPaused || Time.timeScale != 0 || board.CanSelect(new GridPosition(0,0)))
                    throw new InvalidOperationException("Settings button did not open and pause the game.");
            })) yield break;
            if (reload)
            {
                yield return new WaitForSecondsRealtime(.3f);
                if (!Check(() => {
                    DisplaySettings saved=DisplaySettings.Load(settings.SettingsPath);
                    if (saved == null || Screen.width != saved.width || Screen.height != saved.height ||
                        (Screen.fullScreenMode == FullScreenMode.Windowed) != saved.windowed ||
                        settings.WindowedToggle.isOn != saved.windowed)
                        throw new InvalidOperationException("Saved settings were not restored on the next launch.");
                    Debug.Log($"SETTINGS RESTORED: {Screen.width}x{Screen.height}, {Screen.fullScreenMode}");
                })) yield break;
                yield return Capture("SettingsRestored.png");
                yield break;
            }
            yield return Capture("Settings.png");
            yield return ClickAt(RectTransformUtility.WorldToScreenPoint(null,settings.ResolutionDropdown.transform.position));
            if (!Check(() => { if (!settings.ResolutionDropdown.IsExpanded) throw new InvalidOperationException("Resolution dropdown did not open."); })) yield break;
            yield return new WaitForSecondsRealtime(.2f);
            yield return Capture("Resolutions.png");
            yield return EscapeKey();
            yield return new WaitForSecondsRealtime(.2f);
            if (!Check(() => { if (!settings.IsVisible || settings.ResolutionDropdown.IsExpanded) throw new InvalidOperationException("ESC did not dismiss only the resolution list."); })) yield break;
            foreach (var request in new[] {new Vector3Int(960,600,1),new Vector3Int(1280,720,1),new Vector3Int(1280,800,0),new Vector3Int(960,600,1)})
            {
                int index=-1;
                for (int i=0;i<settings.AvailableResolutions.Count;i++)
                    if (settings.AvailableResolutions[i] == new Vector2Int(request.x,request.y)) index=i;
                if (!Check(() => { if (index<0) throw new InvalidOperationException("Missing standard resolution choice."); })) yield break;
                settings.ResolutionDropdown.value=index;
                if (settings.WindowedToggle.isOn != (request.z==1))
                    yield return ClickAt(RectTransformUtility.WorldToScreenPoint(null,settings.WindowedToggle.transform.position));
                yield return ClickButton(settings.ApplyButton);
                float deadline=Time.realtimeSinceStartup+5;
                while ((settings.IsApplying || Screen.width != request.x || Screen.height != request.y) && Time.realtimeSinceStartup<deadline) yield return null;
                if (!Check(() => {
                    bool asWindow=Screen.fullScreenMode==FullScreenMode.Windowed;
                    if (Screen.width!=request.x || Screen.height!=request.y || asWindow!=(request.z==1))
                        throw new InvalidOperationException($"Resolution request {request} produced {Screen.width}x{Screen.height}, {Screen.fullScreenMode}.");
                    DisplaySettings saved=DisplaySettings.Load(settings.SettingsPath);
                    if(saved==null || saved.width!=request.x || saved.height!=request.y || saved.windowed!=asWindow)
                        throw new InvalidOperationException("Applied resolution was not saved correctly.");
                    if(session.Progress.Score!=0 || session.Progress.MovesRemaining!=20 || !board.IsPaused)
                        throw new InvalidOperationException("Display changes altered the paused game.");
                    Debug.Log($"SETTINGS APPLIED: {Screen.width}x{Screen.height}, {Screen.fullScreenMode}");
                })) yield break;
                yield return Capture(request.z==1 ? "Windowed"+request.x+".png" : "Fullscreen.png");
            }
            yield return EscapeKey();
            if (!Check(() => { if (settings.IsVisible || board.IsPaused || Time.timeScale!=1) throw new InvalidOperationException("ESC did not resume the game."); })) yield break;
            MoveFinder.TryFindMove(board.Model,out var from,out var to);
            board.TrySwap(from,to);
            var view=board.GetComponent<BoardView>();
            float wait=Time.realtimeSinceStartup+5;
            while(!view.Effects.IsPlaying && Time.realtimeSinceStartup<wait) yield return null;
            yield return EscapeKey();
            float effectTime=view.Effects.Elapsed;
            yield return new WaitForSecondsRealtime(.15f);
            if (!Check(() => {
                if(!settings.IsVisible || !board.IsBusy || view.Effects.Elapsed!=effectTime)
                    throw new InvalidOperationException("ESC did not freeze an active cascade.");
            })) yield break;
            yield return ClickButton(settings.CloseButton);
            wait=Time.realtimeSinceStartup+20;
            while(board.IsBusy && Time.realtimeSinceStartup<wait) yield return null;
            if (!Check(() => {
                if(board.IsBusy || board.CompletedMoves!=1 || session.Progress.MovesRemaining!=19 || view.PieceCount!=64 ||
                    MatchFinder.FindMatches(board.Model).Count>0) throw new InvalidOperationException("Paused turn did not resume correctly.");
            })) yield break;
            yield return EscapeKey();
            yield return Capture("SettingsAfterTurn.png");
        }

        private IEnumerator EscapeKey()
        {
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            yield return null; yield return null;
        }
        private IEnumerator ClickButton(Button button) => ClickAt(RectTransformUtility.WorldToScreenPoint(null,button.transform.position));
        private IEnumerator ClickAt(Vector2 position)
        {
            QueueMouse(position,true); yield return null; yield return null;
            QueueMouse(position,false); yield return null; yield return null;
        }
        private void OnApplicationQuit()
        {
            if (!expectSettingsQuit || failed) return;
            string report=Array.IndexOf(Environment.GetCommandLineArgs(),"-puzzleSettingsReloadTest")>=0
                ? "PASS: saved resolution and window mode restored after relaunch; settings button and real Quit button also passed."
                : "PASS: settings button, ESC, resolution dropdown, actual window/fullscreen changes, display settings persistence, pause/resume, and real Quit button exited the player.";
            string path=Argument("-puzzleSmokeReport");
            if(!string.IsNullOrEmpty(path)) File.WriteAllText(path,report);
            Debug.Log(report);
        }

        private IEnumerator CheckSpecialPieces()
        {
            BoardView view = board.GetComponent<BoardView>();
            if (!Check(() => {
                for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
                    board.Model.SetPiece(new GridPosition(x, y), new PieceState(y * 8 + x + 1, (PieceColor)(1 + (x + 2 * y) % 6)));
                foreach (int y in new[] { 1, 3, 5 })
                    for (int x = 1; x <= 6; x++)
                    {
                        var position = new GridPosition(x, y);
                        PieceState previous = board.Model.GetPiece(position);
                        board.Model.SetPiece(position, new PieceState(previous.Id, previous.Color,
                            y == 1 ? SpecialPieceType.Row : y == 3 ? SpecialPieceType.Column : SpecialPieceType.Bomb));
                    }
                board.Model.SetPiece(new GridPosition(3, 6), new PieceState(52, PieceColor.None, SpecialPieceType.ColorClear));
                view.Rebuild(board.Model);
                if (MatchFinder.FindMatches(board.Model).Count != 0) throw new InvalidOperationException("Special fixture must be stable.");
            })) yield break;
            yield return null;
            yield return Capture("SpecialPieces.png");
            if (failed) yield break;
            var from = new GridPosition(3, 6); var to = new GridPosition(4, 6);
            Vector2 start = view.BoardCamera.WorldToScreenPoint(view.transform.TransformPoint(view.CellToLocal(from)));
            Vector2 end = view.BoardCamera.WorldToScreenPoint(view.transform.TransformPoint(view.CellToLocal(to)));
            QueueMouse(start, true);
            yield return null; yield return null;
            if (!Check(() => { if (board.SelectedPosition != from) throw new InvalidOperationException("Cannot select a color-clear piece."); })) yield break;
            QueueMouse(end, true);
            yield return null; yield return null;
            QueueMouse(end, false);
            yield return null; yield return null;
            float deadline = Time.realtimeSinceStartup + 20;
            while (board.IsBusy && Time.realtimeSinceStartup < deadline) yield return null;
            if (!Check(() => {
                if (board.IsBusy || board.CompletedMoves != 1 || session.Progress.MovesRemaining != 19 || session.Progress.Score < 100)
                    throw new InvalidOperationException("Special swap did not complete and score exactly one turn.");
                if (view.PieceCount != 64 || MatchFinder.FindMatches(board.Model).Count != 0)
                    throw new InvalidOperationException("Special swap left an unstable board.");
                foreach (var piece in FindObjectsByType<PieceView>(FindObjectsSortMode.None))
                    if (piece.PieceId != board.Model.GetPiece(piece.Position).Id || piece.SpecialType != board.Model.GetPiece(piece.Position).SpecialType)
                        throw new InvalidOperationException("Special view/model mismatch.");
            })) yield break;
            yield return Capture("SpecialAfterSwap.png");
            session.RestartLevel();
            yield return null;
        }

        private void QueueMouse(Vector2 position, bool pressed) =>
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position }.WithButton(MouseButton.Left, pressed));

        private IEnumerator Capture(string name)
        {
            string directory = Argument("-puzzleCaptureFolder");
            if (string.IsNullOrEmpty(directory)) yield break;
            string path = Path.Combine(directory, name);
            Directory.CreateDirectory(directory);
            Canvas canvas = hud.GetComponent<Canvas>();
            BoardView view = board.GetComponent<BoardView>();
            Camera camera = view.BoardCamera;
            RenderMode previousMode = canvas.renderMode;
            Camera previousCamera = canvas.worldCamera;
            float previousDistance = canvas.planeDistance;
            int previousOrder = canvas.sortingOrder;
            float previousAspect = camera.aspect;
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            int width = int.TryParse(Argument("-puzzleCaptureWidth"), out int w) ? Mathf.Clamp(w, 600, 2560) : 1280;
            int height = int.TryParse(Argument("-puzzleCaptureHeight"), out int h) ? Mathf.Clamp(h, 600, 1600) : 800;
            var target = new RenderTexture(width, height, 24);
            var picture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                target.Create();
                camera.targetTexture = target;
                camera.aspect = (float)width / height;
                hud.GetComponent<CandyLayout>()?.ApplyLayout();
                view.FitCamera();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1;
                canvas.sortingOrder = 100;
                RebuildCanvas(canvas);
                yield return null;
                Check(() => {
                    RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                    RenderTexture.active = target;
                    picture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    picture.Apply();
                    File.WriteAllBytes(path, picture.EncodeToPNG());
                });
            }
            finally
            {
                RenderTexture.active = previousActive;
                camera.targetTexture = previousTarget;
                camera.aspect = previousAspect;
                hud.GetComponent<CandyLayout>()?.ApplyLayout();
                canvas.renderMode = previousMode;
                canvas.worldCamera = previousCamera;
                canvas.planeDistance = previousDistance;
                canvas.sortingOrder = previousOrder;
                view.FitCamera();
                RebuildCanvas(canvas);
                target.Release(); Destroy(target); Destroy(picture);
            }
            yield return null;
        }

        private static void RebuildCanvas(Canvas canvas)
        {
            foreach (var graphic in canvas.GetComponentsInChildren<Graphic>(true)) graphic.SetAllDirty();
            foreach (var text in canvas.GetComponentsInChildren<TMPro.TMP_Text>()) text.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
        }

        private bool Check(Action action)
        {
            try { action(); return true; }
            catch (Exception exception) { failed = true; Finish(1, "FAIL: " + exception); return false; }
        }

        private static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }

        private void Finish(int code, string report)
        {
            failed |= code != 0;
            if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
            string path = Argument("-puzzleSmokeReport");
            if (!string.IsNullOrEmpty(path))
            {
                try { File.WriteAllText(path, report); }
                catch (Exception e) { Debug.LogException(e); code = 1; }
            }
            Debug.Log(report);
            Application.Quit(code);
        }
    }
}
#endif
