using System.Collections;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Board;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PuzzleGame.Tests
{
    public class BoardInteractionTests
    {
        [UnityTest]
        public IEnumerator GameSceneCreatesAPlayableBoardAndFitsDifferentAspects()
        {
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return null;
            var controller = Object.FindFirstObjectByType<BoardController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.Model.Width, Is.EqualTo(8));
            Assert.That(controller.Model.Height, Is.EqualTo(8));
            Assert.That(MatchFinder.FindMatches(controller.Model), Is.Empty);
            Assert.That(MoveFinder.HasAnyMove(controller.Model), Is.True);
            var view = controller.GetComponent<BoardView>();
            Assert.That(view.PieceCount, Is.EqualTo(64));
            foreach (PieceView piece in Object.FindObjectsByType<PieceView>(FindObjectsSortMode.None))
            {
                SpriteRenderer face = piece.GetComponent<SpriteRenderer>();
                Assert.That(face.sprite, Is.Not.Null);
                Assert.That(face.sharedMaterial.mainTexture, Is.EqualTo(face.sprite.texture));
            }
            foreach (float aspect in new[] { 16f / 9f, 16f / 10f, 4f / 3f, 0.75f })
            {
                view.BoardCamera.aspect = aspect;
                view.FitCamera();
                foreach (var position in new[] { new GridPosition(0, 0), new GridPosition(7, 7) })
                {
                    Vector3 corner = view.transform.TransformPoint(view.CellToLocal(position));
                    Vector3 viewport = view.BoardCamera.WorldToViewportPoint(corner);
                    Assert.That(viewport.x, Is.InRange(0.04f, 0.96f));
                    Assert.That(viewport.y, Is.InRange(0.04f, 0.96f));
                }
            }
            view.BoardCamera.ResetAspect();
            controller.GenerateBoard();
            yield return null;
            Assert.That(view.PieceCount, Is.EqualTo(64));
            Assert.That(Object.FindObjectsByType<PieceView>(FindObjectsSortMode.None).Length, Is.EqualTo(64));
        }
    }
}
