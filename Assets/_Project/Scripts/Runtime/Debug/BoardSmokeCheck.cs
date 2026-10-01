#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Board;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace PuzzleGame.Runtime.Diagnostics
{
    // Only explicit CLI diagnostics inject events. Normal play never enters this component.
    public sealed class BoardSmokeCheck : MonoBehaviour
    {
        private BoardController controller;
        private BoardView view;
        private Mouse mouse;
        private GridPosition first, second;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-puzzleSmokeTest") < 0) return;
            new GameObject("BoardSmokeCheck").AddComponent<BoardSmokeCheck>();
        }

        private IEnumerator Start()
        {
            yield return null;
            yield return null;
            if (!Check(() =>
            {
                controller = FindFirstObjectByType<BoardController>();
                if (!controller || controller.Model == null) throw new InvalidOperationException("Board did not start.");
                view = controller.GetComponent<BoardView>();
                ValidateBoard();
                if (!MoveFinder.TryFindMove(controller.Model, out first, out second))
                    throw new InvalidOperationException("No valid move.");
                // A hidden CLI test must process its injected mouse even without OS focus.
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                mouse = InputSystem.AddDevice<Mouse>();
                InputSystem.EnableDevice(mouse);
            })) yield break;

            Vector2 start = view.BoardCamera.WorldToScreenPoint(view.transform.TransformPoint(view.CellToLocal(first)));
            Vector2 end = view.BoardCamera.WorldToScreenPoint(view.transform.TransformPoint(view.CellToLocal(second)));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = start }.WithButton(MouseButton.Left, true));
            yield return null;
            yield return null;
            if (!Check(() => {
                if (controller.SelectedPosition != first) throw new InvalidOperationException("Mouse press did not select a piece.");
            })) yield break;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = end }.WithButton(MouseButton.Left, true));
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = end });
            yield return null;
            yield return null;
            float deadline = Time.realtimeSinceStartup + 20;
            while (controller.IsBusy && Time.realtimeSinceStartup < deadline) yield return null;
            if (!Check(() => {
                if (controller.IsBusy || controller.CompletedMoves != 1)
                    throw new InvalidOperationException("Drag did not complete exactly one turn.");
                ValidateBoard();
            })) yield break;
            Finish(0, "PASS: mouse press/drag/release, one completed move, cascade resolved, 64 matching views, stable playable board.");
        }

        private void ValidateBoard()
        {
            BoardState board = controller.Model;
            PieceView[] pieces = FindObjectsByType<PieceView>(FindObjectsSortMode.None);
            if (pieces.Length != 64 || MatchFinder.FindMatches(board).Count != 0 || !MoveFinder.HasAnyMove(board))
                throw new InvalidOperationException("Board stability or view count failed.");
            foreach (PieceView piece in pieces)
                if (board.GetPiece(piece.Position).Id != piece.PieceId
                    || Vector3.Distance(piece.transform.localPosition, view.CellToLocal(piece.Position)) > 0.001f)
                    throw new InvalidOperationException("View/model mapping failed.");
        }

        private bool Check(Action action)
        {
            try { action(); return true; }
            catch (Exception exception) { Finish(1, "FAIL: " + exception); return false; }
        }

        private void Finish(int exitCode, string report)
        {
            if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-puzzleSmokeReport");
            if (index >= 0 && index + 1 < args.Length)
            {
                try { File.WriteAllText(args[index + 1], report); }
                catch (Exception exception) { Debug.LogException(exception); exitCode = 1; }
            }
            Debug.Log(report);
            Application.Quit(exitCode);
        }
    }
}
#endif
