using System;

namespace PuzzleGame.Core.Board
{
    public sealed class BoardState
    {
        private readonly PieceState[,] pieces;
        private readonly CellState[,] cells;
        public int Width { get; }
        public int Height { get; }

        public BoardState(int width, int height)
        {
            if (width < 1 || width > 64) throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1 || height > 64) throw new ArgumentOutOfRangeException(nameof(height));
            Width = width;
            Height = height;
            pieces = new PieceState[width, height];
            cells = new CellState[width, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    cells[x, y] = new CellState();
        }

        public bool Contains(GridPosition position) =>
            position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;
        public bool IsFrozen(GridPosition position) => Contains(position) && GetCell(position).FrostHealth > 0;

        public PieceState GetPiece(GridPosition position)
        {
            Validate(position);
            return pieces[position.X, position.Y];
        }

        public CellState GetCell(GridPosition position)
        {
            Validate(position);
            return cells[position.X, position.Y];
        }

        public void SetPiece(GridPosition position, PieceState piece)
        {
            Validate(position);
            pieces[position.X, position.Y] = piece;
        }

        public void SetCell(GridPosition position, CellState cell)
        {
            Validate(position);
            cells[position.X, position.Y] = cell ?? throw new ArgumentNullException(nameof(cell));
        }

        public void SwapPieces(GridPosition first, GridPosition second)
        {
            Validate(first);
            Validate(second);
            if (IsFrozen(first) || IsFrozen(second)) throw new InvalidOperationException("Frozen candies cannot be swapped.");
            PieceState previous = pieces[first.X, first.Y];
            pieces[first.X, first.Y] = pieces[second.X, second.Y];
            pieces[second.X, second.Y] = previous;
        }

        private void Validate(GridPosition position)
        {
            if (!Contains(position)) throw new ArgumentOutOfRangeException(nameof(position), position, "Outside the board.");
        }
    }
}
