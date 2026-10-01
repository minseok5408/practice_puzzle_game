using System;
using System.Collections.Generic;
using NUnit.Framework;
using PuzzleGame.Core.Board;

namespace PuzzleGame.Tests
{
    public class BoardGeneratorTests
    {
        [TestCase(3, 3, 3)]
        [TestCase(8, 8, 6)]
        [TestCase(12, 7, 4)]
        public void ManySeedsProduceFullStablePlayableBoards(int width, int height, int colors)
        {
            for (int seed = 0; seed < 200; seed++)
            {
                BoardState board = BoardGenerator.Generate(width, height, colors, seed);
                Assert.That(MatchFinder.FindMatches(board), Is.Empty, $"seed {seed}");
                Assert.That(MoveFinder.HasAnyMove(board), Is.True, $"seed {seed}");
                var ids = new HashSet<int>();
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        var position = new GridPosition(x, y);
                        PieceState piece = board.GetPiece(position);
                        Assert.That(piece, Is.Not.Null);
                        Assert.That((int)piece.Color, Is.InRange(1, colors));
                        Assert.That(ids.Add(piece.Id), Is.True);
                        Assert.That(board.GetCell(position).FrostHealth, Is.Zero);
                    }
                Assert.That(ids.Count, Is.EqualTo(width * height));
            }
        }

        [Test]
        public void SameSeedRecreatesColorsAndIds()
        {
            BoardState first = BoardGenerator.Generate(8, 8, 6, -1024);
            BoardState second = BoardGenerator.Generate(8, 8, 6, -1024);
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    var position = new GridPosition(x, y);
                    Assert.That(second.GetPiece(position).Color, Is.EqualTo(first.GetPiece(position).Color));
                    Assert.That(second.GetPiece(position).Id, Is.EqualTo(first.GetPiece(position).Id));
                }
        }

        [TestCase(2, 8, 6, 128)]
        [TestCase(8, 2, 6, 128)]
        [TestCase(8, 8, 2, 128)]
        [TestCase(8, 8, 7, 128)]
        [TestCase(8, 8, 6, 0)]
        public void InvalidConfigurationIsRejected(int width, int height, int colors, int attempts)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BoardGenerator.Generate(width, height, colors, 1, attempts));
        }
    }
}
