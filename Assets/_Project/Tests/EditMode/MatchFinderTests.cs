using NUnit.Framework;
using PuzzleGame.Core.Board;

namespace PuzzleGame.Tests
{
    public class MatchFinderTests
    {
        [Test]
        public void CrossIncludesFiveUniqueCells()
        {
            var board = new BoardState(3, 3);
            int id = 1;
            foreach (GridPosition position in new[] { new GridPosition(1, 0), new GridPosition(0, 1),
                new GridPosition(1, 1), new GridPosition(2, 1), new GridPosition(1, 2) })
                board.SetPiece(position, new PieceState(id++, PieceColor.Red));
            Assert.That(MatchFinder.FindMatches(board).Count, Is.EqualTo(5));
        }

        [Test]
        public void DiagonalAndColorlessPiecesDoNotMatch()
        {
            var board = new BoardState(3, 3);
            for (int x = 0; x < 3; x++)
                board.SetPiece(new GridPosition(x, x), new PieceState(x + 1, PieceColor.Blue));
            Assert.That(MatchFinder.FindMatches(board), Is.Empty);
            for (int x = 0; x < 3; x++)
                board.SetPiece(new GridPosition(x, 0), new PieceState(x + 4, PieceColor.None, SpecialPieceType.ColorClear));
            Assert.That(MatchFinder.FindMatches(board), Is.Empty);
        }

        [Test]
        public void FiveInALineAllBelongToMatch()
        {
            var board = new BoardState(5, 1);
            for (int x = 0; x < 5; x++)
                board.SetPiece(new GridPosition(x, 0), new PieceState(x + 1, PieceColor.Green));
            Assert.That(MatchFinder.FindMatches(board).Count, Is.EqualTo(5));
        }
    }
}
