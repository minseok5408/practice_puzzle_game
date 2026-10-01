using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;

namespace PuzzleGame.Tests
{
    public class CollectionGoalTests
    {
        private static ResolutionStep Step(params RemovedPiece[] pieces)
        {
            var step = new ResolutionStep();
            foreach (var piece in pieces) { step.RemovedIds.Add(piece.Id); step.RemovedPieces.Add(piece); }
            return step;
        }

        [Test]
        public void ScoreAloneCannotWinWhenColorGoalIsMissing()
        {
            var progress = new LevelProgress(new LevelRules(1, 20, 10, new[] { 2, 0, 0, 0, 0, 0 }));
            progress.TryBeginMove();
            progress.RecordRemoval(Step(new RemovedPiece(1, PieceColor.Blue), new RemovedPiece(2, PieceColor.Red)));
            progress.CompleteMove();
            Assert.That(progress.Score, Is.EqualTo(20));
            Assert.That(progress.Outcome, Is.EqualTo(LevelOutcome.Lost));
        }

        [Test]
        public void CascadesCountEachIdOnceAndWinOnLastMove()
        {
            var progress = new LevelProgress(new LevelRules(1, 20, 10, new[] { 2, 0, 0, 0, 0, 0 }));
            progress.TryBeginMove();
            progress.RecordRemoval(Step(new RemovedPiece(1, PieceColor.Red), new RemovedPiece(1, PieceColor.Red)));
            progress.RecordRemoval(Step(new RemovedPiece(1, PieceColor.Red), new RemovedPiece(2, PieceColor.Red)));
            Assert.That(progress.Collected(PieceColor.Red), Is.EqualTo(2));
            Assert.That(progress.Score, Is.EqualTo(20));
            progress.CompleteMove();
            Assert.That(progress.Outcome, Is.EqualTo(LevelOutcome.Won));
        }

        [Test]
        public void CancelRestoresCollectionScoreAndMoves()
        {
            var progress = new LevelProgress(new LevelRules(3, 100, 10, new[] { 2, 0, 0, 0, 0, 0 }));
            progress.TryBeginMove(); progress.RecordRemoval(Step(new RemovedPiece(1, PieceColor.Red))); progress.CompleteMove();
            progress.TryBeginMove(); progress.RecordRemoval(Step(new RemovedPiece(2, PieceColor.Red))); progress.CancelMove();
            Assert.That(progress.Collected(PieceColor.Red), Is.EqualTo(1));
            Assert.That(progress.Score, Is.EqualTo(10));
            Assert.That(progress.MovesRemaining, Is.EqualTo(2));
            Assert.That(progress.Outcome, Is.EqualTo(LevelOutcome.Playing));
        }

        [Test]
        public void SurvivingSpecialIsNotCollectedAndColorlessPieceOnlyScores()
        {
            var step = Step(new RemovedPiece(1, PieceColor.None));
            step.RemovedPieces.Add(new RemovedPiece(2, PieceColor.Red));
            var progress = new LevelProgress(new LevelRules(2, 100, 10));
            progress.TryBeginMove(); progress.RecordRemoval(step);
            Assert.That(progress.Score, Is.EqualTo(10));
            Assert.That(progress.Collected(PieceColor.Red), Is.Zero);
            Assert.That(progress.Collected(PieceColor.None), Is.Zero);
        }
    }
}
