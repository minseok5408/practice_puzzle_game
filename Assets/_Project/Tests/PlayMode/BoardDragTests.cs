using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PuzzleGame.Tests
{
    public class BoardDragTests
    {
        private readonly InputTestFixture inputFixture = new InputTestFixture();
        private Mouse mouse;
        private BoardController controller;
        private BoardView view;

        [UnitySetUp]
        public IEnumerator LoadBoard()
        {
            // Initialize here: UnitySetUp runs before NUnit SetUp in this test runner.
            // Unload live input users before replacing the Input System for this fixture.
            foreach (var previous in Object.FindObjectsByType<SettingsPopup>(FindObjectsSortMode.None)) previous.enabled = false;
            foreach (var previous in Object.FindObjectsByType<BoardInput>(FindObjectsSortMode.None))
                previous.gameObject.SetActive(false);
            foreach (var previous in Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
                previous.gameObject.SetActive(false);
            yield return null;
            inputFixture.Setup();
            mouse = InputSystem.AddDevice<Mouse>();
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return null;
            controller = Object.FindFirstObjectByType<BoardController>();
            view = controller.GetComponent<BoardView>();
            Assert.That(controller.GetComponent<BoardInput>(), Is.Not.Null);
        }

        [UnityTearDown]
        public IEnumerator UnloadInput()
        {
            foreach (var settings in Object.FindObjectsByType<SettingsPopup>(FindObjectsSortMode.None)) settings.enabled = false;
            if (controller) controller.gameObject.SetActive(false);
            if (EventSystem.current) EventSystem.current.gameObject.SetActive(false);
            yield return null;
            inputFixture.TearDown();
        }

        [UnityTest]
        public IEnumerator ValidDragFollowsPointerThenResolvesAndAcceptsAnotherMove()
        {
            for (int turn = 0; turn < 2; turn++)
            {
                Assert.That(MoveFinder.TryFindMove(controller.Model, out var a, out var b), Is.True);
                Vector2 start = Screen(a), end = Screen(b);
                yield return MouseAt(start, true);
                Assert.That(controller.SelectedPosition, Is.EqualTo(a));
                PieceView selected = FindPiece(a);
                Assert.That(selected.IsSelected, Is.True);
                yield return MouseAt(Vector2.Lerp(start, end, 0.65f), true);
                Assert.That(Vector3.Distance(selected.transform.localPosition, view.CellToLocal(a)), Is.GreaterThan(0.3f));
                yield return MouseAt(end, false);
                Assert.That(controller.IsBusy, Is.True);
                Assert.That(controller.TrySwap(a, b), Is.False);
                yield return WaitForIdle();
                Assert.That(controller.CompletedMoves, Is.EqualTo(turn + 1));
                AssertStable();
            }
        }

        [UnityTest]
        public IEnumerator FourDirectionsAndLongDragChooseOnlyTheImmediateNeighbour()
        {
            var start = new GridPosition(3, 3);
            foreach (var offset in new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down, Vector2Int.right * 3 })
            {
                controller.GenerateBoard();
                yield return null;
                var target = new GridPosition(start.X + System.Math.Sign(offset.x), start.Y + System.Math.Sign(offset.y));
                var release = new GridPosition(start.X + offset.x, start.Y + offset.y);
                int neighbourId = controller.Model.GetPiece(target).Id;
                yield return MouseAt(Screen(start), true);
                yield return MouseAt(Screen(release), false);
                Assert.That(controller.IsBusy, Is.True);
                Assert.That(controller.Model.GetPiece(start).Id, Is.EqualTo(neighbourId));
                yield return WaitForIdle();
                AssertStable();
            }
        }

        [UnityTest]
        public IEnumerator InvalidDragReturnsEveryPieceToItsOriginalCell()
        {
            FindInvalidPair(out var a, out var b);
            var ids = Ids();
            yield return MouseAt(Screen(a), true);
            yield return MouseAt(Screen(b), false);
            yield return WaitForIdle();
            CollectionAssert.AreEqual(ids, Ids());
            Assert.That(controller.CompletedMoves, Is.Zero);
            AssertStable();
        }

        [UnityTest]
        public IEnumerator SmallMovementSelectsAndSameCellClickCancels()
        {
            var p = new GridPosition(2, 2);
            Vector2 start = Screen(p);
            yield return MouseAt(start, true);
            yield return MouseAt(start + (Screen(new GridPosition(3, 2)) - start) * 0.1f, false);
            Assert.That(controller.SelectedPosition, Is.EqualTo(p));
            Assert.That(controller.CompletedMoves, Is.Zero);
            yield return MouseAt(start, true);
            yield return MouseAt(start, false);
            Assert.That(controller.SelectedPosition, Is.Null);
            AssertStable();
        }

        [UnityTest]
        public IEnumerator OutsideReleaseAndFocusLossCancelWithoutSwapping()
        {
            var ids = Ids();
            yield return MouseAt(Screen(new GridPosition(7, 4)), true);
            yield return MouseAt(new Vector2(-20, -20), false);
            Assert.That(controller.SelectedPosition, Is.Null);
            CollectionAssert.AreEqual(ids, Ids());
            yield return MouseAt(Screen(new GridPosition(2, 2)), true);
            controller.GetComponent<BoardInput>().SendMessage("OnApplicationFocus", false);
            yield return MouseAt(Screen(new GridPosition(3, 2)), false);
            Assert.That(controller.SelectedPosition, Is.Null);
            Assert.That(controller.IsBusy, Is.False);
            CollectionAssert.AreEqual(ids, Ids());
        }

        [UnityTest]
        public IEnumerator UiBlocksBothPressAndRelease()
        {
            Vector2 start = Screen(new GridPosition(2, 2));
            Vector2 end = Screen(new GridPosition(3, 2));
            var ids = Ids();
            yield return MouseAt(start, true);
            GameObject canvas = MakeOverlay();
            yield return null;
            yield return MouseAt(end, false);
            Assert.That(controller.SelectedPosition, Is.Null);
            yield return MouseAt(start, true);
            yield return MouseAt(start, false);
            Assert.That(controller.SelectedPosition, Is.Null);
            CollectionAssert.AreEqual(ids, Ids());
            Object.Destroy(canvas);
        }

        [UnityTest]
        public IEnumerator DisableMidTurnRollsBackAndRegenerationCancelsOldDrag()
        {
            var ids = Ids();
            MoveFinder.TryFindMove(controller.Model, out var a, out var b);
            yield return MouseAt(Screen(a), true);
            yield return MouseAt(Screen(b), false);
            Assert.That(controller.IsBusy, Is.True);
            controller.enabled = false;
            yield return null;
            CollectionAssert.AreEqual(ids, Ids());
            AssertStable();
            controller.enabled = true;
            yield return MouseAt(Screen(a), true);
            controller.GenerateBoard();
            yield return MouseAt(Screen(b), false);
            Assert.That(controller.CompletedMoves, Is.Zero);
            Assert.That(controller.IsBusy, Is.False);
            AssertStable();
        }

        [UnityTest]
        public IEnumerator DistantClicksSelectWithoutSwapAndEscapeCancels()
        {
            var a = new GridPosition(0, 0);
            var b = new GridPosition(7, 7);
            var ids = Ids();
            yield return MouseAt(Screen(a), true);
            yield return MouseAt(Screen(a), false);
            yield return MouseAt(Screen(b), true);
            yield return MouseAt(Screen(b), false);
            Assert.That(controller.SelectedPosition, Is.EqualTo(b));
            CollectionAssert.AreEqual(ids, Ids());
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            try
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
                yield return null;
                yield return null;
                Assert.That(controller.SelectedPosition, Is.Null);
                Assert.That(Object.FindFirstObjectByType<SettingsPopup>().IsVisible, Is.True);
            }
            finally { InputSystem.RemoveDevice(keyboard); }
        }

        private IEnumerator MouseAt(Vector2 position, bool pressed)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position }.WithButton(MouseButton.Left, pressed));
            yield return null;
            yield return null;
        }

        private IEnumerator WaitForIdle()
        {
            float end = Time.realtimeSinceStartup + 20;
            while (controller.IsBusy && Time.realtimeSinceStartup < end) yield return null;
            Assert.That(controller.IsBusy, Is.False, "Turn did not finish.");
            yield return null;
        }

        private Vector2 Screen(GridPosition p) => view.BoardCamera.WorldToScreenPoint(view.transform.TransformPoint(view.CellToLocal(p)));

        private PieceView FindPiece(GridPosition p)
        {
            foreach (var piece in Object.FindObjectsByType<PieceView>(FindObjectsSortMode.None))
                if (piece.Position == p) return piece;
            Assert.Fail("Missing piece.");
            return null;
        }

        private List<int> Ids()
        {
            var ids = new List<int>();
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++) ids.Add(controller.Model.GetPiece(new GridPosition(x, y)).Id);
            return ids;
        }

        private void AssertStable()
        {
            Assert.That(view.PieceCount, Is.EqualTo(64));
            Assert.That(MatchFinder.FindMatches(controller.Model), Is.Empty);
            Assert.That(MoveFinder.HasAnyMove(controller.Model), Is.True);
            var pieces = Object.FindObjectsByType<PieceView>(FindObjectsSortMode.None);
            Assert.That(pieces.Length, Is.EqualTo(64));
            foreach (var piece in pieces)
            {
                Assert.That(controller.Model.GetPiece(piece.Position).Id, Is.EqualTo(piece.PieceId));
                Assert.That(Vector3.Distance(piece.transform.localPosition, view.CellToLocal(piece.Position)), Is.LessThan(0.001f));
            }
        }

        private void FindInvalidPair(out GridPosition a, out GridPosition b)
        {
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 7; x++)
                {
                    a = new GridPosition(x, y); b = new GridPosition(x + 1, y);
                    if (!MoveFinder.IsValidSwap(controller.Model, a, b)) return;
                }
            a = b = default;
            Assert.Fail("No invalid pair in fixture.");
        }

        private GameObject MakeOverlay()
        {
            var canvas = new GameObject("TestCanvas", typeof(Canvas), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.GetComponent<Canvas>().sortingOrder = 100;
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return canvas;
        }
    }
}
