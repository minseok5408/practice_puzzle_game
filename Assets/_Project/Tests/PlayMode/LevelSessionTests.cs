using System.Collections;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PuzzleGame.Tests
{
    public class LevelSessionTests
    {
        private readonly InputTestFixture input = new InputTestFixture();
        private Mouse mouse;
        private InputActionAsset uiActions;
        private BoardController board;
        private LevelSession session;
        private HUDView hud;
        private ResultPopup popup;

        [UnitySetUp]
        public IEnumerator Load()
        {
            // Unload live input users before replacing the Input System for this fixture.
            foreach (var previous in Object.FindObjectsByType<SettingsPopup>(FindObjectsSortMode.None)) previous.enabled = false;
            foreach (var previous in Object.FindObjectsByType<BoardInput>(FindObjectsSortMode.None))
                previous.gameObject.SetActive(false);
            foreach (var previous in Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
                previous.gameObject.SetActive(false);
            yield return null;
            input.Setup();
            // The CLI test window has no OS focus; allow only this isolated fixture to process UI clicks.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            mouse = InputSystem.AddDevice<Mouse>();
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return null;
            // Shared project-wide assets can retain bindings to a device from a prior fixture.
            // Clone UI actions just as gameplay actions are cloned, resolving this fixture's devices.
            var module = EventSystem.current.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            module.enabled = false;
            uiActions = Object.Instantiate(module.actionsAsset);
            module.actionsAsset = uiActions;
            module.enabled = true;
            board = Object.FindFirstObjectByType<BoardController>();
            session = board.Session;
            hud = Object.FindFirstObjectByType<HUDView>();
            popup = Object.FindFirstObjectByType<ResultPopup>();
            Assert.That(session, Is.Not.Null);
        }

        [UnityTearDown]
        public IEnumerator Unload()
        {
            foreach (var settings in Object.FindObjectsByType<SettingsPopup>(FindObjectsSortMode.None)) settings.enabled = false;
            if (board) board.gameObject.SetActive(false);
            if (EventSystem.current) EventSystem.current.gameObject.SetActive(false);
            if (uiActions) Object.Destroy(uiActions);
            yield return null;
            input.TearDown();
        }

        [UnityTest]
        public IEnumerator SceneShowsInitialRulesInKoreanHud()
        {
            Assert.That(session.Progress.Score, Is.Zero);
            Assert.That(session.Progress.MovesRemaining, Is.EqualTo(20));
            Assert.That(session.Progress.Rules.TargetScore, Is.EqualTo(1000));
            Assert.That(hud.ScoreText, Is.EqualTo("0"));
            Assert.That(hud.MovesText, Is.EqualTo("20"));
            Assert.That(popup.IsVisible, Is.False);
            Assert.That(hud.RestartButton.interactable, Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator InvalidSwapDoesNotConsumeMovesOrAwardPoints()
        {
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 7; x++)
                {
                    var a = new GridPosition(x, y); var b = new GridPosition(x + 1, y);
                    if (MoveFinder.IsValidSwap(board.Model, a, b)) continue;
                    Assert.That(board.TrySwap(a, b), Is.True);
                    yield return Idle();
                    Assert.That(session.Progress.Score, Is.Zero);
                    Assert.That(session.Progress.MovesRemaining, Is.EqualTo(20));
                    Assert.That(popup.IsVisible, Is.False);
                    yield break;
                }
            Assert.Fail("No invalid swap fixture.");
        }

        [UnityTest]
        public IEnumerator LastMoveScoresEntireCascadeBeforeShowingSuccessAndLocksBoard()
        {
            session.StartWithRules(new LevelRules(1, 30, 10));
            int expected = PredictFirstMoveScore(out var a, out var b);
            Assert.That(board.TrySwap(a, b), Is.True);
            Assert.That(session.Progress.MovesRemaining, Is.Zero);
            Assert.That(popup.IsVisible, Is.False);
            while (board.IsBusy)
            {
                Assert.That(session.Progress.Outcome, Is.EqualTo(LevelOutcome.Playing));
                Assert.That(popup.IsVisible, Is.False);
                yield return null;
            }
            Assert.That(session.Progress.Score, Is.EqualTo(expected));
            Assert.That(session.Progress.Outcome, Is.EqualTo(LevelOutcome.Won));
            Assert.That(popup.IsVisible, Is.True);
            Assert.That(popup.Title, Does.Contain("성공"));
            Assert.That(board.CanSelect(a), Is.False);
            Assert.That(board.TrySwap(a, b), Is.False);
            Assert.That(hud.ScoreText, Is.EqualTo(expected.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)));
            Assert.That(hud.MovesText, Is.EqualTo("0"));
            Assert.That(MatchFinder.FindMatches(board.Model), Is.Empty);
        }

        [UnityTest]
        public IEnumerator FailureAndRealRestartButtonWorkRepeatedly()
        {
            session.StartWithRules(new LevelRules(1, 100000, 10));
            for (int cycle = 0; cycle < 2; cycle++)
            {
                MoveFinder.TryFindMove(board.Model, out var a, out var b);
                board.TrySwap(a, b);
                yield return Idle();
                Assert.That(session.Progress.Outcome, Is.EqualTo(LevelOutcome.Lost));
                Assert.That(popup.Title, Does.Contain("실패"));
                Assert.That(popup.IsVisible, Is.True);
                BoardState oldBoard = board.Model;
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, popup.RestartButton.transform.position);
                yield return Click(screen);
                Assert.That(board.Model, Is.Not.SameAs(oldBoard));
                Assert.That(session.Progress.Outcome, Is.EqualTo(LevelOutcome.Playing));
                Assert.That(session.Progress.MovesRemaining, Is.EqualTo(1));
                Assert.That(session.Progress.Score, Is.Zero);
                Assert.That(popup.IsVisible, Is.False);
                Assert.That(board.CanSelect(new GridPosition(0, 0)), Is.True);
                Assert.That(Object.FindObjectsByType<PieceView>(FindObjectsSortMode.None).Length, Is.EqualTo(64));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            }
        }

        [UnityTest]
        public IEnumerator RestartDuringResolutionDiscardsOldTurnAndHudRestartWorks()
        {
            MoveFinder.TryFindMove(board.Model, out var a, out var b);
            board.TrySwap(a, b);
            Assert.That(session.Progress.MovesRemaining, Is.EqualTo(19));
            session.RestartLevel();
            yield return new WaitForSeconds(.8f);
            Assert.That(board.IsBusy, Is.False);
            Assert.That(session.Progress.Score, Is.Zero);
            Assert.That(session.Progress.MovesRemaining, Is.EqualTo(20));
            board.TrySwap(a, b);
            yield return Idle();
            Assert.That(session.Progress.Score, Is.GreaterThan(0));
            yield return Click(RectTransformUtility.WorldToScreenPoint(null, hud.RestartButton.transform.position));
            Assert.That(session.Progress.Score, Is.Zero);
            Assert.That(session.Progress.MovesRemaining, Is.EqualTo(20));
            Assert.That(board.SelectedPosition, Is.Null);
        }

        [UnityTest]
        public IEnumerator DisableDuringResolutionRefundsMoveAndScore()
        {
            MoveFinder.TryFindMove(board.Model, out var a, out var b);
            board.TrySwap(a, b);
            float deadline = Time.realtimeSinceStartup + 10;
            while (session.Progress.Score == 0 && board.IsBusy && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(board.IsBusy, Is.True);
            Assert.That(session.Progress.Score, Is.GreaterThan(0));
            board.enabled = false;
            yield return null;
            Assert.That(session.Progress.Score, Is.Zero);
            Assert.That(session.Progress.MovesRemaining, Is.EqualTo(20));
            Assert.That(session.Progress.Outcome, Is.EqualTo(LevelOutcome.Playing));
            Assert.That(MatchFinder.FindMatches(board.Model), Is.Empty);
            board.enabled = true;
            Assert.That(board.CanSelect(a), Is.True);
        }

        private int PredictFirstMoveScore(out GridPosition a, out GridPosition b)
        {
            var copy = new BoardState(8, 8);
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                { var p = new GridPosition(x, y); copy.SetPiece(p, board.Model.GetPiece(p)); }
            Assert.That(MoveFinder.TryFindMove(copy, out a, out b), Is.True);
            var resolver = new BoardResolver(copy, 6, new System.Random(board.Seed));
            copy.SwapPieces(a, b);
            int score = 0, steps = 0;
            score += resolver.ResolveMatches(copy, a, b).RemovedIds.Count * 10;
            while (MatchFinder.FindMatches(copy).Count > 0)
            {
                Assert.That(++steps, Is.LessThan(128));
                score += resolver.ResolveMatches(copy).RemovedIds.Count * 10;
            }
            return score;
        }

        [UnityTest]
        public IEnumerator FourMatchUpdatesExistingViewAndRefundsSpecialCreationWhenInterrupted()
        {
            FillStableBoard();
            foreach (int x in new[] { 1, 2, 4 }) Put(x, 0, PieceColor.Purple);
            Put(3, 1, PieceColor.Purple);
            var from = new GridPosition(3, 1); var to = new GridPosition(3, 0);
            PieceState original = board.Model.GetPiece(from);
            var view = board.GetComponent<BoardView>();
            view.Rebuild(board.Model);
            Assert.That(MatchFinder.FindMatches(board.Model), Is.Empty);
            Assert.That(board.TrySwap(from, to), Is.True);
            float deadline = Time.realtimeSinceStartup + 10;
            while (board.Model.GetPiece(to)?.SpecialType != SpecialPieceType.Row && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(board.Model.GetPiece(to).Id, Is.EqualTo(original.Id));
            Assert.That(board.Model.GetPiece(to).SpecialType, Is.EqualTo(SpecialPieceType.Row));
            PieceView transformed = null;
            foreach (var piece in Object.FindObjectsByType<PieceView>(FindObjectsSortMode.None))
                if (piece.PieceId == original.Id) transformed = piece;
            Assert.That(transformed, Is.Not.Null);
            Assert.That(transformed.SpecialType, Is.EqualTo(SpecialPieceType.Row));
            Assert.That(transformed.GetComponent<SpriteRenderer>().sprite.name, Is.EqualTo("RowPurpleFlower"));
            Assert.That(transformed.GetComponentsInChildren<LineRenderer>(), Is.Empty);
            board.enabled = false;
            yield return null;
            Assert.That(board.Model.GetPiece(from), Is.SameAs(original));
            Assert.That(board.Model.GetPiece(from).SpecialType, Is.EqualTo(SpecialPieceType.None));
            Assert.That(session.Progress.MovesRemaining, Is.EqualTo(20));
            Assert.That(session.Progress.Score, Is.Zero);
            board.enabled = true;
        }

        [UnityTest]
        public IEnumerator ColorClearMouseDragWithoutMatchScoresOneTurnAndKeepsViewsInSync()
        {
            FillStableBoard();
            Put(0, 0, PieceColor.None, SpecialPieceType.ColorClear);
            Put(3, 0, PieceColor.Green, SpecialPieceType.Row);
            Put(4, 0, PieceColor.Blue, SpecialPieceType.Column);
            Put(5, 0, PieceColor.Red, SpecialPieceType.Bomb);
            var view = board.GetComponent<BoardView>();
            view.Rebuild(board.Model);
            Assert.That(MatchFinder.FindMatches(board.Model), Is.Empty);
            foreach (var piece in Object.FindObjectsByType<PieceView>(FindObjectsSortMode.None))
                if (piece.SpecialType != SpecialPieceType.None)
                {
                    var face = piece.GetComponent<SpriteRenderer>();
                    Assert.That(face.sprite.name, Does.StartWith(piece.SpecialType == SpecialPieceType.ColorClear ? "CandyRainbow" : piece.SpecialType.ToString()));
                    Assert.That(face.sharedMaterial.mainTexture, Is.SameAs(face.sprite.texture));
                    Assert.That(piece.GetComponentsInChildren<LineRenderer>(), Is.Empty);
                    Assert.That(board.CanSelect(piece.Position), Is.True);
                }
            Vector2 start = view.BoardCamera.WorldToScreenPoint(view.transform.TransformPoint(view.CellToLocal(new GridPosition(0, 0))));
            Vector2 end = view.BoardCamera.WorldToScreenPoint(view.transform.TransformPoint(view.CellToLocal(new GridPosition(1, 0))));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = start }.WithButton(MouseButton.Left, true));
            yield return null; yield return null;
            Assert.That(board.SelectedPosition, Is.EqualTo(new GridPosition(0, 0)));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = end }.WithButton(MouseButton.Left, true));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = end });
            yield return null; yield return null;
            yield return Idle();
            Assert.That(session.Progress.MovesRemaining, Is.EqualTo(19));
            Assert.That(session.Progress.Score, Is.GreaterThanOrEqualTo(110));
            Assert.That(board.CompletedMoves, Is.EqualTo(1));
            Assert.That(view.PieceCount, Is.EqualTo(64));
            Assert.That(MatchFinder.FindMatches(board.Model), Is.Empty);
            foreach (var piece in Object.FindObjectsByType<PieceView>(FindObjectsSortMode.None))
            {
                Assert.That(piece.PieceId, Is.EqualTo(board.Model.GetPiece(piece.Position).Id));
                Assert.That(piece.SpecialType, Is.EqualTo(board.Model.GetPiece(piece.Position).SpecialType));
            }
        }

        [UnityTest]
        public IEnumerator SpecialCombinationOnLastMoveCompletesBeforeWinning()
        {
            session.StartWithRules(new LevelRules(1, 100, 10));
            FillStableBoard();
            Put(0, 0, PieceColor.Purple, SpecialPieceType.Row);
            Put(1, 0, PieceColor.Purple, SpecialPieceType.Column);
            board.GetComponent<BoardView>().Rebuild(board.Model);
            Assert.That(MatchFinder.FindMatches(board.Model), Is.Empty);
            Assert.That(board.TrySwap(new GridPosition(0, 0), new GridPosition(1, 0)), Is.True);
            Assert.That(session.Progress.MovesRemaining, Is.Zero);
            Assert.That(popup.IsVisible, Is.False);
            yield return Idle();
            Assert.That(session.Progress.Outcome, Is.EqualTo(LevelOutcome.Won));
            Assert.That(session.Progress.Score, Is.GreaterThanOrEqualTo(150));
            Assert.That(board.CompletedMoves, Is.EqualTo(1));
            Assert.That(popup.IsVisible, Is.True);
        }

        [UnityTest]
        public IEnumerator NormalPopHasVisibleEffectsAndInterruptionRestoresTheTurn()
        {
            FillStableBoard();
            Put(1,2,PieceColor.Purple); Put(2,2,PieceColor.Purple); Put(3,3,PieceColor.Purple);
            var view = board.GetComponent<BoardView>(); view.Rebuild(board.Model);
            PieceState original = board.Model.GetPiece(new GridPosition(3,3));
            Assert.That(board.TrySwap(new GridPosition(3,3),new GridPosition(3,2)), Is.True);
            float deadline = Time.realtimeSinceStartup + 5;
            while (view.Effects.ActiveVisualCount == 0 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(view.Effects.IsPlaying, Is.True);
            Assert.That(view.Effects.ActiveVisualCount, Is.GreaterThan(0));
            Assert.That(board.IsBusy, Is.True);
            Assert.That(board.CanSelect(new GridPosition(0,0)), Is.False);
            board.enabled = false;
            Assert.That(view.Effects.ActiveVisualCount, Is.Zero);
            Assert.That(view.Effects.IsPlaying, Is.False);
            Assert.That(board.Model.GetPiece(new GridPosition(3,3)), Is.SameAs(original));
            Assert.That(session.Progress.Score, Is.Zero);
            Assert.That(session.Progress.MovesRemaining, Is.EqualTo(20));
            Assert.That(view.PieceCount, Is.EqualTo(64));
            board.enabled = true;
        }

        [UnityTest]
        public IEnumerator DirectionalWavesReachNearCandiesBeforeFarCandiesAndCleanUp()
        {
            foreach (var type in new[] { SpecialPieceType.Row, SpecialPieceType.Column })
            {
                session.RestartLevel(); FillStableBoard();
                Put(1,2,PieceColor.Purple); Put(2,2,PieceColor.Purple); Put(3,3,PieceColor.Purple,type);
                var view = board.GetComponent<BoardView>(); view.Rebuild(board.Model);
                var near = type == SpecialPieceType.Row ? new GridPosition(0,2) : new GridPosition(3,0);
                var far = type == SpecialPieceType.Row ? new GridPosition(7,2) : new GridPosition(3,7);
                int nearId = board.Model.GetPiece(near).Id, farId = board.Model.GetPiece(far).Id;
                Assert.That(board.TrySwap(new GridPosition(3,3),new GridPosition(3,2)), Is.True);
                float deadline = Time.realtimeSinceStartup + 5;
                while (!view.Effects.IsPlaying && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(view.Effects.IsPlaying, Is.True);
                Assert.That(view.Effects.HitTime(nearId), Is.LessThan(view.Effects.HitTime(farId)));
                yield return Idle();
                Assert.That(view.Effects.ActiveVisualCount, Is.Zero);
                Assert.That(view.Effects.IsPlaying, Is.False);
                Assert.That(session.Progress.MovesRemaining, Is.EqualTo(19));
                Assert.That(view.PieceCount, Is.EqualTo(64));
                Assert.That(MatchFinder.FindMatches(board.Model), Is.Empty);
                foreach (var piece in Object.FindObjectsByType<PieceView>(FindObjectsSortMode.None))
                {
                    Assert.That(piece.transform.localRotation, Is.EqualTo(Quaternion.identity));
                    Assert.That(Vector3.Distance(piece.transform.localPosition, view.CellToLocal(piece.Position)), Is.LessThan(.001f));
                }
            }
        }

        [UnityTest]
        public IEnumerator SettingsButtonOpensModalAndEscapeRestoresInput()
        {
            var settings = Object.FindFirstObjectByType<SettingsPopup>();
            Assert.That(settings.IsVisible, Is.False);
            yield return Click(RectTransformUtility.WorldToScreenPoint(null,settings.OpenButton.transform.position));
            Assert.That(settings.IsVisible, Is.True);
            Assert.That(board.IsPaused, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(board.CanSelect(new GridPosition(0,0)), Is.False);
            Assert.That(settings.AvailableResolutions.Count, Is.GreaterThan(1));
            Assert.That(settings.QuitButton.interactable, Is.True);
            var view = board.GetComponent<BoardView>();
            yield return Click(view.BoardCamera.WorldToScreenPoint(view.transform.TransformPoint(view.CellToLocal(new GridPosition(0,0)))));
            Assert.That(board.SelectedPosition, Is.Null);
            yield return Escape();
            Assert.That(settings.IsVisible, Is.False);
            Assert.That(board.IsPaused, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(session.Progress.MovesRemaining, Is.EqualTo(20));
            Assert.That(board.CanSelect(new GridPosition(0,0)), Is.True);
        }

        [UnityTest]
        public IEnumerator EscapeDuringDragCancelsGestureAndDropdownConsumesFirstEscape()
        {
            var settings = Object.FindFirstObjectByType<SettingsPopup>();
            var view = board.GetComponent<BoardView>(); var cell = new GridPosition(2,2);
            Vector2 start = view.BoardCamera.WorldToScreenPoint(view.transform.TransformPoint(view.CellToLocal(cell)));
            InputSystem.QueueStateEvent(mouse,new MouseState {position=start}.WithButton(MouseButton.Left,true));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState {position=start+Vector2.right*20}.WithButton(MouseButton.Left,true));
            yield return null; yield return null;
            yield return Escape();
            Assert.That(settings.IsVisible, Is.True);
            Assert.That(board.SelectedPosition, Is.Null);
            InputSystem.QueueStateEvent(mouse,new MouseState {position=start});
            yield return null; yield return null;
            settings.ResolutionDropdown.Show();
            yield return null;
            Assert.That(settings.ResolutionDropdown.IsExpanded, Is.True);
            yield return Escape();
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(settings.ResolutionDropdown.IsExpanded, Is.False);
            Assert.That(settings.IsVisible, Is.True);
            yield return Escape();
            Assert.That(settings.IsVisible, Is.False);
            Assert.That(board.CompletedMoves, Is.Zero);
            Assert.That(session.Progress.MovesRemaining, Is.EqualTo(20));
        }

        [UnityTest]
        public IEnumerator SettingsPauseFreezesEffectsThenResumesTheSameTurn()
        {
            var settings = Object.FindFirstObjectByType<SettingsPopup>();
            var view = board.GetComponent<BoardView>();
            MoveFinder.TryFindMove(board.Model,out var from,out var to);
            board.TrySwap(from,to);
            float deadline = Time.realtimeSinceStartup+5;
            while (!view.Effects.IsPlaying && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(view.Effects.IsPlaying, Is.True);
            settings.Open();
            float elapsed = view.Effects.Elapsed; int score=session.Progress.Score;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(view.Effects.Elapsed, Is.EqualTo(elapsed));
            Assert.That(session.Progress.Score, Is.EqualTo(score));
            Assert.That(board.IsBusy, Is.True);
            settings.Close();
            yield return Idle();
            Assert.That(board.CompletedMoves, Is.EqualTo(1));
            Assert.That(session.Progress.MovesRemaining, Is.EqualTo(19));
            Assert.That(view.PieceCount, Is.EqualTo(64));
            Assert.That(MatchFinder.FindMatches(board.Model), Is.Empty);
        }

        [UnityTest]
        public IEnumerator ClosingSettingsDiscardsUnappliedChoicesAndDisableReleasesPause()
        {
            var settings = Object.FindFirstObjectByType<SettingsPopup>();
            Time.timeScale=.75f;
            settings.Open();
            int original=settings.ResolutionDropdown.value;
            bool window=settings.WindowedToggle.isOn;
            settings.ResolutionDropdown.value=(original+1)%settings.AvailableResolutions.Count;
            settings.WindowedToggle.isOn=!window;
            Assert.That(settings.ApplyButton.interactable, Is.True);
            settings.Close();
            Assert.That(Time.timeScale, Is.EqualTo(.75f));
            settings.Open();
            Assert.That(settings.ResolutionDropdown.value, Is.EqualTo(original));
            Assert.That(settings.WindowedToggle.isOn, Is.EqualTo(window));
            settings.enabled=false;
            Assert.That(Time.timeScale, Is.EqualTo(.75f));
            Assert.That(board.IsPaused, Is.False);
            Assert.That(settings.IsVisible, Is.False);
            Time.timeScale=1;
            yield return null;
        }

        private IEnumerator Escape()
        {
            var keyboard=InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            yield return null; yield return null;
            InputSystem.RemoveDevice(keyboard);
        }

        private void FillStableBoard()
        {
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) Put(x, y, (PieceColor)(1 + (x + y) % 5));
        }

        private void Put(int x, int y, PieceColor color, SpecialPieceType special = SpecialPieceType.None) =>
            board.Model.SetPiece(new GridPosition(x, y), new PieceState(y * 8 + x + 1, color, special));

        private IEnumerator Click(Vector2 position)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position }.WithButton(MouseButton.Left, true));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
            yield return null; yield return null;
        }

        private IEnumerator Idle()
        {
            float deadline = Time.realtimeSinceStartup + 20;
            while (board.IsBusy && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(board.IsBusy, Is.False);
            yield return null;
        }
    }
}
