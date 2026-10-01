using NUnit.Framework;
using PuzzleGame.Core.Board;

namespace PuzzleGame.Tests
{
    public class MoveFinderTests
    {
        [Test]
        public void SearchRestoresExactPieceAndCellReferences()
        {
            BoardState board = BoardGenerator.Generate(8, 8, 6, 5408);
            var pieces = new PieceState[8, 8];
            var cells = new CellState[8, 8];
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    var position = new GridPosition(x, y);
                    pieces[x, y] = board.GetPiece(position);
                    cells[x, y] = board.GetCell(position);
                }
            Assert.That(MoveFinder.TryFindMove(board, out GridPosition first, out GridPosition second), Is.True);
            Assert.That(first.IsAdjacentTo(second), Is.True);
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    var position = new GridPosition(x, y);
                    Assert.That(board.GetPiece(position), Is.SameAs(pieces[x, y]));
                    Assert.That(board.GetCell(position), Is.SameAs(cells[x, y]));
                }
            board.SwapPieces(first, second);
            Assert.That(MatchFinder.FindMatches(board), Is.Not.Empty);
        }

        [Test]
        public void LatinSquareHasNoValidMove()
        {
            var board = new BoardState(3, 3);
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                    board.SetPiece(new GridPosition(x, y),
                        new PieceState(y * 3 + x + 1, (PieceColor)(1 + (x + y) % 3)));
            Assert.That(MatchFinder.FindMatches(board), Is.Empty);
            Assert.That(MoveFinder.HasAnyMove(board), Is.False);
        }

        [Test]
        public void OutOfBoundsSameCellAndDiagonalAreRejected()
        {
            BoardState board = BoardGenerator.Generate(8, 8, 6, 1);
            Assert.That(MoveFinder.IsValidSwap(board, new GridPosition(-1, 0), new GridPosition(0, 0)), Is.False);
            Assert.That(MoveFinder.IsValidSwap(board, new GridPosition(0, 0), new GridPosition(0, 0)), Is.False);
            Assert.That(MoveFinder.IsValidSwap(board, new GridPosition(0, 0), new GridPosition(1, 1)), Is.False);
        }

        [Test]
        public void ExistingUnrelatedMatchDoesNotValidateAnotherSwap()
        {
            var board = new BoardState(4, 4);
            for (int x = 0; x < 3; x++)
                board.SetPiece(new GridPosition(x, 0), new PieceState(x + 1, PieceColor.Red));
            board.SetPiece(new GridPosition(2, 3), new PieceState(4, PieceColor.Blue));
            board.SetPiece(new GridPosition(3, 3), new PieceState(5, PieceColor.Green));
            Assert.That(MoveFinder.IsValidSwap(board, new GridPosition(2, 3), new GridPosition(3, 3)), Is.False);
        }
    }
}
