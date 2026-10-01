using System;

namespace PuzzleGame.Core.Board
{
    public sealed class PieceState
    {
        public int Id { get; }
        public PieceColor Color { get; }
        public SpecialPieceType SpecialType { get; }

        public PieceState(int id, PieceColor color, SpecialPieceType specialType = SpecialPieceType.None)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (!Enum.IsDefined(typeof(PieceColor), color))
                throw new ArgumentOutOfRangeException(nameof(color));
            if (!Enum.IsDefined(typeof(SpecialPieceType), specialType))
                throw new ArgumentOutOfRangeException(nameof(specialType));
            if ((specialType == SpecialPieceType.ColorClear) != (color == PieceColor.None))
                throw new ArgumentException("Only a color-clear piece has no base color.");
            Id = id;
            Color = color;
            SpecialType = specialType;
        }
    }
}
