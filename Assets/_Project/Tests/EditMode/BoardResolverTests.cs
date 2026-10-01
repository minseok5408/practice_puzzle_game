using System;
using System.Collections.Generic;
using NUnit.Framework;
using PuzzleGame.Core.Board;

namespace PuzzleGame.Tests
{
    public class BoardResolverTests
    {
        [Test]
        public void RemovalCompactsInOrderAndKeepsFixedCells()
        {
            var board = new BoardState(1, 6);
            var colors = new[] { PieceColor.Red, PieceColor.Red, PieceColor.Red, PieceColor.Green, PieceColor.Blue, PieceColor.Yellow };
            var cells = new CellState[6];
            for (int y = 0; y < 6; y++)
            {
                var p = new GridPosition(0, y);
                board.SetPiece(p, new PieceState(y + 1, colors[y]));
                cells[y] = board.GetCell(p);
            }
            var step = new BoardResolver(board, 6, new Random(12)).ResolveMatches(board);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, step.RemovedIds);
            Assert.That(step.Moves.Count, Is.EqualTo(3));
            Assert.That(step.Spawns.Count, Is.EqualTo(3));
            for (int y = 0; y < 3; y++) Assert.That(board.GetPiece(new GridPosition(0, y)).Id, Is.EqualTo(y + 4));
            for (int y = 0; y < 6; y++) Assert.That(board.GetCell(new GridPosition(0, y)), Is.SameAs(cells[y]));
            foreach (var spawn in step.Spawns)
            {
                Assert.That(spawn.From.Y, Is.GreaterThanOrEqualTo(6));
                Assert.That(board.GetPiece(spawn.To).Id, Is.EqualTo(spawn.PieceId));
                Assert.That(spawn.PieceId, Is.GreaterThan(6));
            }
        }

        [Test]
        public void StableBoardIsUnchanged()
        {
            var board = BoardGenerator.Generate(8, 8, 6, 10);
            var before = Ids(board);
            var step = new BoardResolver(board, 6, new Random(10)).ResolveMatches(board);
            Assert.That(step.RemovedIds, Is.Empty);
            CollectionAssert.AreEqual(before, Ids(board));
        }

        [Test]
        public void FiveHundredTurnsRemainFullStableAndPlayable()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var board = BoardGenerator.Generate(8, 8, 6, seed);
                var random = new Random(seed);
                var resolver = new BoardResolver(board, 6, random);
                for (int turn = 0; turn < 10; turn++)
                {
                    Assert.That(MoveFinder.TryFindMove(board, out var a, out var b), Is.True);
                    board.SwapPieces(a, b);
                    int cascade = 0;
                    bool firstStep = true;
                    while (firstStep || MatchFinder.FindMatches(board).Count > 0)
                    {
                        Assert.That(++cascade, Is.LessThan(128), $"seed={seed}, turn={turn}");
                        var step = resolver.ResolveMatches(board, firstStep ? a : (GridPosition?)null, firstStep ? b : (GridPosition?)null);
                        firstStep = false;
                        Assert.That(new HashSet<int>(step.RemovedIds).Count, Is.EqualTo(step.RemovedIds.Count));
                    }
                    if (!MoveFinder.HasAnyMove(board)) Assert.That(BoardShuffler.TryShuffle(board, random), Is.True);
                    Assert.That(new HashSet<int>(Ids(board)).Count, Is.EqualTo(64));
                    Assert.That(MatchFinder.FindMatches(board), Is.Empty);
                    Assert.That(MoveFinder.HasAnyMove(board), Is.True);
                }
            }
        }

        [Test]
        public void ShufflePreservesPieceAndCellIdentities()
        {
            var board = BoardGenerator.Generate(8, 8, 6, 42);
            var ids = Ids(board);
            var cell = board.GetCell(new GridPosition(0, 0));
            Assert.That(BoardShuffler.TryShuffle(board, new Random(30)), Is.True);
            CollectionAssert.AreEquivalent(ids, Ids(board));
            Assert.That(board.GetCell(new GridPosition(0, 0)), Is.SameAs(cell));
            Assert.That(MatchFinder.FindMatches(board), Is.Empty);
            Assert.That(MoveFinder.HasAnyMove(board), Is.True);
        }

        [Test]
        public void ImpossibleShuffleRestoresExactBoard()
        {
            var board = new BoardState(3, 3);
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                    board.SetPiece(new GridPosition(x, y), new PieceState(y * 3 + x + 1, PieceColor.Red));
            var before = Ids(board);
            Assert.That(BoardShuffler.TryShuffle(board, new Random(1), 3), Is.False);
            CollectionAssert.AreEqual(before, Ids(board));
        }

        private static List<int> Ids(BoardState board)
        {
            var result = new List<int>();
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                    result.Add(board.GetPiece(new GridPosition(x, y)).Id);
            return result;
        }
    }
}
