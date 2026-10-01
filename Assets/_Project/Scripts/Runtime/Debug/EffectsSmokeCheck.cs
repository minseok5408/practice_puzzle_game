#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.Diagnostics
{
    // Explicit opt-in: capture the real runtime animation with a fixed presentation timestep.
    public sealed class EffectsSmokeCheck : MonoBehaviour
    {
        private BoardController board;
        private BoardView view;
        private Camera camera;
        private Canvas canvas;
        private RenderTexture target;
        private Texture2D picture;
        private string directory;
        private string currentCase;
        private int frame;
        private bool failed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-puzzleEffectsSmokeTest") >= 0)
                new GameObject("EffectsSmokeCheck").AddComponent<EffectsSmokeCheck>();
        }

        private IEnumerator Start()
        {
            Application.runInBackground = true;
            yield return null; yield return null;
            if (!Check(() => {
                board = FindFirstObjectByType<BoardController>();
                view = board.GetComponent<BoardView>(); camera = view.BoardCamera;
                canvas = FindFirstObjectByType<HUDView>().GetComponent<Canvas>();
                directory = Argument("-puzzleCaptureFolder");
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                    Time.captureFramerate = 30;
                    target = new RenderTexture(960,600,24); target.Create();
                    picture = new Texture2D(960,600,TextureFormat.RGB24,false);
                    camera.targetTexture = target; camera.aspect = 1.6f;
                    canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera;
                    canvas.planeDistance = 1; canvas.sortingOrder = 100;
                    canvas.GetComponent<CandyLayout>().ApplyLayout(); view.FitCamera();
                    foreach (var graphic in canvas.GetComponentsInChildren<Graphic>(true)) graphic.SetAllDirty();
                    Canvas.ForceUpdateCanvases();
                }
            })) yield break;
            foreach (string name in new[] { "Normal", "Row", "Column", "Bomb", "Rainbow", "FiveMatch", "DoubleRainbow" })
            {
                currentCase = name; frame = 0;
                if (!Check(() => Prepare(name))) yield break;
                for (int i = 0; i < 12; i++) { yield return null; if (!Check(Capture)) yield break; }
                if (!Check(() => {
                    var from = new GridPosition(3,3);
                    var to = name == "Rainbow" || name == "DoubleRainbow" ? new GridPosition(4,3) : new GridPosition(3,2);
                    if (!MoveFinder.IsValidSwap(board.Model, from, to) || !board.TrySwap(from,to))
                        throw new InvalidOperationException(name + ": fixture swap is invalid.");
                })) yield break;
                bool sawEffects = false;
                float deadline = Time.realtimeSinceStartup + 150;
                while (board.IsBusy && Time.realtimeSinceStartup < deadline)
                {
                    sawEffects |= view.Effects.ActiveVisualCount > 0;
                    yield return null; if (!Check(Capture)) yield break;
                    if (frame > 600) break;
                }
                if (!Check(() => {
                    if (!sawEffects || board.IsBusy || view.Effects.IsPlaying || view.Effects.ActiveVisualCount != 0)
                        throw new InvalidOperationException(name + ": effects did not play/finish/clean up.");
                    if (view.PieceCount != 64 || MatchFinder.FindMatches(board.Model).Count != 0 || board.CompletedMoves != 1 ||
                        board.Session.Progress.MovesRemaining != 19 || board.Session.Progress.Score < 30)
                        throw new InvalidOperationException(name + ": board, move count or score failed.");
                    foreach (var piece in FindObjectsByType<PieceView>(FindObjectsSortMode.None))
                        if (piece.PieceId != board.Model.GetPiece(piece.Position).Id || piece.SpecialType != board.Model.GetPiece(piece.Position).SpecialType ||
                            Vector3.Distance(piece.transform.localPosition,view.CellToLocal(piece.Position)) > .001f)
                            throw new InvalidOperationException(name + ": view/model mismatch after effects.");
                    Debug.Log("EFFECTS PASS: " + name + ", score=" + board.Session.Progress.Score + ", frames=" + frame);
                })) yield break;
                for (int i = 0; i < 12; i++) { yield return null; if (!Check(Capture)) yield break; }
            }
            Finish(0, "PASS: normal pop, horizontal wave, vertical wave, wrapped blast, rainbow links, five-match creation, double-rainbow full board. Each completes one move, stabilizes 64 views, and cleans effects.");
        }

        private void Prepare(string name)
        {
            board.Session.StartWithRules(new LevelRules(20,100000,10));
            for (int y=0;y<8;y++) for (int x=0;x<8;x++) Put(x,y,(PieceColor)(1+(x+2*y)%6));
            if (name == "Rainbow" || name == "DoubleRainbow")
            {
                Put(3,3,PieceColor.None,SpecialPieceType.ColorClear);
                if (name == "DoubleRainbow") Put(4,3,PieceColor.None,SpecialPieceType.ColorClear);
            }
            else
            {
                Put(1,2,PieceColor.Purple); Put(2,2,PieceColor.Purple);
                SpecialPieceType special = name == "Row" ? SpecialPieceType.Row : name == "Column" ? SpecialPieceType.Column :
                    name == "Bomb" ? SpecialPieceType.Bomb : SpecialPieceType.None;
                Put(3,3,PieceColor.Purple,special);
                if (name == "FiveMatch") { Put(4,2,PieceColor.Purple); Put(5,2,PieceColor.Purple); }
            }
            view.Rebuild(board.Model);
            if (MatchFinder.FindMatches(board.Model).Count > 0) throw new InvalidOperationException(name + ": unstable fixture.");
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(Path.Combine(directory,name));
        }
        private void Put(int x,int y,PieceColor color,SpecialPieceType special = SpecialPieceType.None) =>
            board.Model.SetPiece(new GridPosition(x,y),new PieceState(y*8+x+1,color,special));
        private void Capture()
        {
            if (!target) return;
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest {destination=target});
            RenderTexture.active = target;
            picture.ReadPixels(new Rect(0,0,960,600),0,0); picture.Apply();
            File.WriteAllBytes(Path.Combine(directory,currentCase,frame.ToString("D4")+".png"),picture.EncodeToPNG());
            RenderTexture.active = null; frame++;
        }
        private bool Check(Action action)
        {
            try { action(); return true; }
            catch (Exception exception) { failed = true; Finish(1,"FAIL: " + exception); return false; }
        }
        private static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs(); int index=Array.IndexOf(args,name);
            return index>=0 && index+1<args.Length ? args[index+1] : null;
        }
        private void Finish(int code,string report)
        {
            Time.captureFramerate=0;
            if (camera) camera.targetTexture=null;
            if (target) { target.Release(); Destroy(target); }
            if (picture) Destroy(picture);
            string path=Argument("-puzzleSmokeReport");
            if (!string.IsNullOrEmpty(path)) File.WriteAllText(path,report);
            if (failed) Debug.LogError(report); else Debug.Log(report);
            Application.Quit(code);
        }
    }
}
#endif
