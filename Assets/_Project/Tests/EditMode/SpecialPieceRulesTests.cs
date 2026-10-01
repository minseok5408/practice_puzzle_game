using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;

namespace PuzzleGame.Tests
{
    public class SpecialPieceRulesTests
    {
        [TestCase(3, false, SpecialPieceType.None)]
        [TestCase(4, false, SpecialPieceType.Row)]
        [TestCase(4, true, SpecialPieceType.Column)]
        [TestCase(5, false, SpecialPieceType.ColorClear)]
        [TestCase(6, true, SpecialPieceType.ColorClear)]
        public void StraightRunsCreateCorrectSpecialAndScoreOnlyRemovedPieces(int length, bool vertical, SpecialPieceType expected)
        {
            var board = new BoardState(vertical ? 1 : length, vertical ? length : 1);
            for (int i = 0; i < length; i++) Put(board, vertical ? 0 : i, vertical ? i : 0, PieceColor.Red);
            MatchResult matches = MatchFinder.FindGroups(board);
            Assert.That(matches.Groups.Count, Is.EqualTo(1));
            Assert.That(matches.Groups[0].Runs[0].Positions.Count, Is.EqualTo(length));
            Assert.That(matches.Groups[0].Runs[0].Direction,
                Is.EqualTo(vertical ? MatchDirection.Vertical : MatchDirection.Horizontal));
            var step = Resolve(board);
            Assert.That(step.SpecialCreations.Count, Is.EqualTo(expected == SpecialPieceType.None ? 0 : 1));
            Assert.That(step.RemovedIds.Count, Is.EqualTo(length - step.SpecialCreations.Count));
            if (expected != SpecialPieceType.None)
            {
                var created = step.SpecialCreations[0];
                Assert.That(created.Position, Is.EqualTo(new GridPosition(0, 0)));
                Assert.That(created.Piece.SpecialType, Is.EqualTo(expected));
                Assert.That(created.Piece.Color, Is.EqualTo(expected == SpecialPieceType.ColorClear ? PieceColor.None : PieceColor.Red));
                Assert.That(step.RemovedIds, Has.No.Member(created.Piece.Id));
                Assert.That(step.SpecialActivations, Is.Empty);
            }
            var progress = new LevelProgress(new LevelRules(1, 30, 10));
            progress.TryBeginMove();
            progress.RecordRemovedPieces(step.RemovedIds);
            Assert.That(progress.Score, Is.EqualTo(step.RemovedIds.Count * 10));
        }

        [TestCase(0, 0)] // L
        [TestCase(1, 0)] // T
        [TestCase(1, 1)] // cross
        public void IntersectingRunsFormOneBombGroup(int column, int row)
        {
            var board = new BoardState(3, 3);
            for (int i = 0; i < 3; i++) { Put(board, i, row, PieceColor.Red); Put(board, column, i, PieceColor.Red); }
            var matches = MatchFinder.FindGroups(board);
            Assert.That(matches.Groups.Count, Is.EqualTo(1));
            Assert.That(matches.Positions.Count, Is.EqualTo(5));
            Assert.That(matches.Groups[0].Runs.Count, Is.EqualTo(2));
            Assert.That(matches.Groups[0].HasIntersection, Is.True);
            var step = Resolve(board);
            Assert.That(step.SpecialCreations.Single().Piece.SpecialType, Is.EqualTo(SpecialPieceType.Bomb));
            Assert.That(step.RemovedIds.Count, Is.EqualTo(4));
        }

        [Test]
        public void FiveRunWinsOverCrossingAndConnectedRunsMergeTransitively()
        {
            var board = new BoardState(5, 3);
            for (int x = 0; x < 5; x++) { Put(board, x, 0, PieceColor.Red); Put(board, x, 2, PieceColor.Red); }
            Put(board, 2, 1, PieceColor.Red);
            var matches = MatchFinder.FindGroups(board);
            Assert.That(matches.Groups.Count, Is.EqualTo(1));
            Assert.That(matches.Groups[0].Runs.Count, Is.EqualTo(3));
            Assert.That(matches.Positions.Count, Is.EqualTo(11));
            var step = Resolve(board);
            Assert.That(step.SpecialCreations.Single().Piece.SpecialType, Is.EqualTo(SpecialPieceType.ColorClear));
        }

        [Test]
        public void AdjacentParallelRunsRemainIndependentGroups()
        {
            var board = new BoardState(4, 2);
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 4; x++) Put(board, x, y, PieceColor.Green);
            Assert.That(MatchFinder.FindGroups(board).Groups.Count, Is.EqualTo(2));
            Assert.That(Resolve(board).SpecialCreations.Count, Is.EqualTo(2));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CreationPrefersSwapDestinationThenOtherSwapCell(bool destinationIsSpecial)
        {
            var board = new BoardState(4, 1);
            for (int x = 0; x < 4; x++) Put(board, x, 0, PieceColor.Blue);
            if (destinationIsSpecial) Put(board, 2, 0, PieceColor.Blue, SpecialPieceType.Row);
            var step = new BoardResolver(board, 6, new Random(1)).ResolveMatches(board, new GridPosition(1, 0), new GridPosition(2, 0));
            var creation = step.SpecialCreations.Single();
            Assert.That(creation.Position, Is.EqualTo(new GridPosition(destinationIsSpecial ? 1 : 2, 0)));
            Assert.That(step.RemovedIds, Has.No.Member(creation.Piece.Id));
            Assert.That(step.SpecialActivations.Any(a => a.PieceId == creation.Piece.Id), Is.False);
            if (destinationIsSpecial) Assert.That(step.SpecialActivations.Count, Is.EqualTo(1));
        }

        [Test]
        public void ExistingSpecialsAreNotReplacedWhenNoNormalCandidateExists()
        {
            var board = new BoardState(4, 1);
            for (int x = 0; x < 4; x++) Put(board, x, 0, PieceColor.Red, SpecialPieceType.Row);
            var step = Resolve(board);
            Assert.That(step.SpecialCreations, Is.Empty);
            Assert.That(step.SpecialActivations.Count, Is.EqualTo(4));
            Assert.That(step.RemovedIds.Count, Is.EqualTo(4));
        }

        [TestCase(SpecialPieceType.Row, 7)]
        [TestCase(SpecialPieceType.Column, 7)]
        [TestCase(SpecialPieceType.Bomb, 9)]
        public void MatchActivatesExistingSpecialWithExpectedFootprint(SpecialPieceType type, int count)
        {
            var board = LatinBoard(5);
            bool verticalMatch = type == SpecialPieceType.Row;
            for (int i = 1; i <= 3; i++) Put(board, verticalMatch ? 2 : i, verticalMatch ? i : 2, PieceColor.Purple);
            Put(board, 2, 2, PieceColor.Purple, type);
            var step = Resolve(board);
            Assert.That(step.SpecialActivations.Single().Type, Is.EqualTo(type));
            Assert.That(step.SpecialCreations, Is.Empty);
            Assert.That(step.RemovedIds.Count, Is.EqualTo(count));
        }

        [Test]
        public void BombAtCornerClipsToBoardAndCanTriggerOtherSpecialOnlyOnce()
        {
            var board = LatinBoard(5);
            Put(board, 0, 0, PieceColor.Red, SpecialPieceType.Bomb);
            Put(board, 1, 0, PieceColor.Red, SpecialPieceType.Row);
            Put(board, 2, 0, PieceColor.Red);
            var step = Resolve(board);
            Assert.That(step.SpecialActivations.Count, Is.EqualTo(2));
            Assert.That(step.RemovedIds.Count, Is.EqualTo(7));
            Assert.That(step.RemovedIds.Distinct().Count(), Is.EqualTo(7));
        }

        [TestCase(SpecialPieceType.None)]
        [TestCase(SpecialPieceType.Row)]
        [TestCase(SpecialPieceType.Column)]
        [TestCase(SpecialPieceType.Bomb)]
        public void ColorClearSwapRemovesPartnerColorAndTriggersPartner(SpecialPieceType partnerType)
        {
            var board = LatinBoard(5);
            Put(board, 0, 0, PieceColor.None, SpecialPieceType.ColorClear);
            Put(board, 1, 0, PieceColor.Purple, partnerType);
            Put(board, 4, 4, PieceColor.Purple);
            int partnerId = board.GetPiece(new GridPosition(1, 0)).Id;
            int distantId = board.GetPiece(new GridPosition(4, 4)).Id;
            Assert.That(MatchFinder.FindMatches(board), Is.Empty);
            var step = SwapAndResolve(board, new GridPosition(0, 0), new GridPosition(1, 0));
            Assert.That(step.RemovedIds, Does.Contain(partnerId));
            Assert.That(step.RemovedIds, Does.Contain(distantId));
            Assert.That(step.SpecialActivations.Count, Is.EqualTo(partnerType == SpecialPieceType.None ? 1 : 2));
            Assert.That(step.SpecialActivations.Single(a => a.Type == SpecialPieceType.ColorClear).TargetColor, Is.EqualTo(PieceColor.Purple));
        }

        [Test]
        public void TwoColorClearsRemoveAllPiecesAndActivateEachOldSpecialOnce()
        {
            var board = LatinBoard(5);
            Put(board, 0, 0, PieceColor.None, SpecialPieceType.ColorClear);
            Put(board, 1, 0, PieceColor.None, SpecialPieceType.ColorClear);
            Put(board, 2, 2, PieceColor.Green, SpecialPieceType.Bomb);
            var step = SwapAndResolve(board, new GridPosition(0, 0), new GridPosition(1, 0));
            Assert.That(step.RemovedIds.Count, Is.EqualTo(25));
            Assert.That(step.SpecialActivations.Count, Is.EqualTo(3));
            Assert.That(step.SpecialActivations.Select(a => a.PieceId).Distinct().Count(), Is.EqualTo(3));
        }

        [TestCase(SpecialPieceType.Row, SpecialPieceType.Column)]
        [TestCase(SpecialPieceType.Row, SpecialPieceType.Bomb)]
        [TestCase(SpecialPieceType.Bomb, SpecialPieceType.Bomb)]
        [TestCase(SpecialPieceType.Column, SpecialPieceType.Column)]
        public void SameColorSpecialSwapIsValidWithoutMatch(SpecialPieceType first, SpecialPieceType second)
        {
            var board = LatinBoard(5);
            Put(board, 0, 0, PieceColor.Purple, first);
            Put(board, 1, 0, PieceColor.Purple, second);
            var step = SwapAndResolve(board, new GridPosition(0, 0), new GridPosition(1, 0));
            Assert.That(step.SpecialActivations.Count, Is.EqualTo(2));
            Assert.That(step.RemovedIds.Distinct().Count(), Is.EqualTo(step.RemovedIds.Count));
        }

        [Test]
        public void ChainColorClearChoosesMostCommonBaseColorWithIdTieBreak()
        {
            var board = new BoardState(5, 3);
            Put(board, 0, 0, PieceColor.Purple, SpecialPieceType.Row);
            Put(board, 0, 1, PieceColor.Purple);
            Put(board, 0, 2, PieceColor.Purple);
            Put(board, 2, 0, PieceColor.None, SpecialPieceType.ColorClear);
            for (int x = 1; x < 5; x++) { Put(board, x, 1, x % 2 == 0 ? PieceColor.Blue : PieceColor.Red); Put(board, x, 2, x % 2 == 0 ? PieceColor.Red : PieceColor.Blue); }
            var step = Resolve(board);
            Assert.That(step.SpecialActivations.Single(a => a.Type == SpecialPieceType.ColorClear).TargetColor, Is.EqualTo(PieceColor.Red));
            Assert.That(step.RemovedIds.Count, Is.EqualTo(8));
        }

        [Test]
        public void NormalAndColoredSpecialStillNeedAMatchAndSearchRestoresState()
        {
            var board = LatinBoard(3);
            Put(board, 0, 0, PieceColor.Red, SpecialPieceType.Row);
            var original = board.GetPiece(new GridPosition(0, 0));
            Assert.That(MoveFinder.IsValidSwap(board, new GridPosition(0, 0), new GridPosition(1, 0)), Is.False);
            Assert.That(board.GetPiece(new GridPosition(0, 0)), Is.SameAs(original));
            Put(board, 1, 0, PieceColor.Blue, SpecialPieceType.Bomb);
            Assert.That(MoveFinder.TryFindMove(board, out var a, out var b), Is.True);
            Assert.That(a, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(b, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(board.GetPiece(new GridPosition(0, 0)), Is.SameAs(original));
            Put(board, 1, 0, PieceColor.None, SpecialPieceType.ColorClear);
            Assert.That(MoveFinder.IsValidSwap(board, new GridPosition(0, 0), new GridPosition(1, 0)), Is.True);
        }

        [Test]
        public void ShufflePreservesEveryPieceColorTypeAndFixedCell()
        {
            var board = BoardGenerator.Generate(8, 8, 6, 22);
            Put(board, 0, 0, PieceColor.None, SpecialPieceType.ColorClear);
            Put(board, 1, 0, PieceColor.Red, SpecialPieceType.Row);
            Put(board, 2, 0, PieceColor.Blue, SpecialPieceType.Column);
            Put(board, 3, 0, PieceColor.Green, SpecialPieceType.Bomb);
            var originals = new List<PieceState>();
            var cell = board.GetCell(new GridPosition(0, 0));
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) originals.Add(board.GetPiece(new GridPosition(x, y)));
            Assert.That(BoardShuffler.TryShuffle(board, new Random(8)), Is.True);
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) Assert.That(originals, Does.Contain(board.GetPiece(new GridPosition(x, y))));
            Assert.That(board.GetCell(new GridPosition(0, 0)), Is.SameAs(cell));
            Assert.That(MatchFinder.FindMatches(board), Is.Empty);
            Assert.That(MoveFinder.HasAnyMove(board), Is.True);
        }

        [Test]
        public void VisualTargetsDescribeChainHitsWithoutRemovingNewCreations()
        {
            var board = LatinBoard(8);
            Put(board, 0, 0, PieceColor.Red, SpecialPieceType.Row);
            Put(board, 1, 0, PieceColor.Blue, SpecialPieceType.Bomb);
            Put(board, 6, 0, PieceColor.Green, SpecialPieceType.Column);
            for (int x = 3; x < 7; x++) Put(board, x, 4, PieceColor.Purple);
            var step = SwapAndResolve(board, new GridPosition(0,0), new GridPosition(1,0));
            var rootIds = new HashSet<int>(step.InitialHitIds);
            Assert.That(rootIds, Does.Contain(1)); Assert.That(rootIds, Does.Contain(2));
            Assert.That(rootIds, Has.No.Member(7));
            Assert.That(step.SpecialActivations.Single(a => a.PieceId == 1).AffectedIds, Does.Contain(7));
            var visualIds = new HashSet<int>(rootIds);
            foreach (var activation in step.SpecialActivations)
            {
                Assert.That(activation.AffectedIds.Distinct().Count(), Is.EqualTo(activation.AffectedIds.Count));
                visualIds.UnionWith(activation.AffectedIds);
            }
            Assert.That(visualIds, Is.EquivalentTo(step.RemovedIds));
            foreach (var creation in step.SpecialCreations) Assert.That(visualIds, Has.No.Member(creation.Piece.Id));
        }

        [Test]
        public void DoubleRainbowRecordsTwoRootsAndFullBoardTargets()
        {
            var board = LatinBoard(8);
            Put(board, 0,0,PieceColor.None,SpecialPieceType.ColorClear);
            Put(board, 1,0,PieceColor.None,SpecialPieceType.ColorClear);
            var step = SwapAndResolve(board,new GridPosition(0,0),new GridPosition(1,0));
            Assert.That(step.InitialHitIds, Is.EquivalentTo(new[] {1,2}));
            Assert.That(step.RemovedIds.Count, Is.EqualTo(64));
            foreach (var activation in step.SpecialActivations)
                Assert.That(activation.AffectedIds, Is.EquivalentTo(step.RemovedIds));
        }

        private static BoardState LatinBoard(int size)
        {
            var board = new BoardState(size, size);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++) Put(board, x, y, (PieceColor)(1 + (x + y) % 5));
            return board;
        }

        private static void Put(BoardState board, int x, int y, PieceColor color, SpecialPieceType special = SpecialPieceType.None) =>
            board.SetPiece(new GridPosition(x, y), new PieceState(y * board.Width + x + 1, color, special));

        private static ResolutionStep Resolve(BoardState board) => new BoardResolver(board, 6, new Random(42)).ResolveMatches(board);

        private static ResolutionStep SwapAndResolve(BoardState board, GridPosition a, GridPosition b)
        {
            Assert.That(MoveFinder.IsValidSwap(board, a, b), Is.True);
            board.SwapPieces(a, b);
            return new BoardResolver(board, 6, new Random(42)).ResolveMatches(board, a, b);
        }
    }
}
