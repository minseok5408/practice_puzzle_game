using System;
using NUnit.Framework;
using PuzzleGame.Core.Levels;

namespace PuzzleGame.Tests
{
    public class LevelProgressTests
    {
        [Test]
        public void NewLevelHasZeroScoreAndFullMoves()
        {
            var p = new LevelProgress(new LevelRules(20, 600, 10));
            Assert.That(p.Score, Is.Zero);
            Assert.That(p.MovesRemaining, Is.EqualTo(20));
            Assert.That(p.Outcome, Is.EqualTo(LevelOutcome.Playing));
            Assert.That(p.IsResolving, Is.False);
        }

        [Test]
        public void CascadeScoresEveryUniquePieceButConsumesOnlyOneMove()
        {
            var p = new LevelProgress(new LevelRules(20, 600, 10));
            Assert.That(p.TryBeginMove(), Is.True);
            Assert.That(p.TryBeginMove(), Is.False);
            Assert.That(p.RecordRemovedPieces(new[] { 1, 2, 3, 3 }), Is.EqualTo(30));
            Assert.That(p.RecordRemovedPieces(new[] { 3, 4, 5, 6 }), Is.EqualTo(30));
            Assert.That(p.MovesRemaining, Is.EqualTo(19));
            Assert.That(p.Score, Is.EqualTo(60));
            p.CompleteMove();
            Assert.That(p.Outcome, Is.EqualTo(LevelOutcome.Playing));
        }

        [Test]
        public void LastMoveCanWinAfterItsFinalCascade()
        {
            var p = new LevelProgress(new LevelRules(1, 60, 10));
            p.TryBeginMove();
            p.RecordRemovedPieces(new[] { 1, 2, 3 });
            Assert.That(p.MovesRemaining, Is.Zero);
            Assert.That(p.Outcome, Is.EqualTo(LevelOutcome.Playing));
            p.RecordRemovedPieces(new[] { 4, 5, 6 });
            Assert.That(p.IsFinished, Is.False);
            p.CompleteMove();
            Assert.That(p.Outcome, Is.EqualTo(LevelOutcome.Won));
            Assert.That(p.TryBeginMove(), Is.False);
            Assert.That(p.CompleteMove(), Is.False);
        }

        [Test]
        public void LastMoveLosesOnlyAfterResolving()
        {
            var p = new LevelProgress(new LevelRules(1, 600, 10));
            p.TryBeginMove();
            p.RecordRemovedPieces(new[] { 1, 2, 3 });
            Assert.That(p.IsFinished, Is.False);
            p.CompleteMove();
            Assert.That(p.Outcome, Is.EqualTo(LevelOutcome.Lost));
            Assert.That(p.TryBeginMove(), Is.False);
        }

        [Test]
        public void CancelRestoresScoreAndMovesBeforeTheTurn()
        {
            var p = new LevelProgress(new LevelRules(3, 600, 10));
            p.TryBeginMove(); p.RecordRemovedPieces(new[] { 1, 2, 3 }); p.CompleteMove();
            p.TryBeginMove(); p.RecordRemovedPieces(new[] { 4, 5, 6 });
            Assert.That(p.CancelMove(), Is.True);
            Assert.That(p.Score, Is.EqualTo(30));
            Assert.That(p.MovesRemaining, Is.EqualTo(2));
            Assert.That(p.CancelMove(), Is.False);
            Assert.That(p.TryBeginMove(), Is.True);
        }

        [Test]
        public void RemovalOutsideATurnAndMalformedIdsAreRejectedWithoutScoring()
        {
            var p = new LevelProgress(new LevelRules(2, 600, 10));
            Assert.Throws<InvalidOperationException>(() => p.RecordRemovedPieces(new[] { 1 }));
            p.TryBeginMove();
            Assert.Throws<ArgumentOutOfRangeException>(() => p.RecordRemovedPieces(new[] { 1, -1 }));
            Assert.That(p.Score, Is.Zero);
            Assert.That(p.RecordRemovedPieces(new[] { 1 }), Is.EqualTo(10));
        }

        [TestCase(0, 600, 10)]
        [TestCase(20, 0, 10)]
        [TestCase(20, 600, 0)]
        public void InvalidRulesAreRejected(int moves, int target, int points)
        { Assert.Throws<ArgumentOutOfRangeException>(() => new LevelRules(moves, target, points)); }
    }
}
