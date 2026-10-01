using System;

namespace PuzzleGame.Core.Board
{
    // A fixed board cell, independent of the piece that currently occupies it.
    public sealed class CellState
    {
        public int FrostHealth { get; }

        public CellState(int frostHealth = 0)
        {
            if (frostHealth < 0) throw new ArgumentOutOfRangeException(nameof(frostHealth));
            FrostHealth = frostHealth;
        }
    }
}
