using System.Collections.Generic;

namespace PuzzleGame.Core.Board
{
    public static class SpecialPieceRules
    {
        public static bool IsSpecialSwap(PieceState a, PieceState b) => a != null && b != null &&
            (a.SpecialType == SpecialPieceType.ColorClear || b.SpecialType == SpecialPieceType.ColorClear ||
             (a.SpecialType != SpecialPieceType.None && b.SpecialType != SpecialPieceType.None));

        public static SpecialPieceType CreationType(MatchGroup group)
        {
            foreach (var run in group.Runs)
                if (run.Positions.Count >= 5) return SpecialPieceType.ColorClear;
            if (group.HasIntersection) return SpecialPieceType.Bomb;
            foreach (var run in group.Runs)
                if (run.Positions.Count == 4)
                    return run.Direction == MatchDirection.Horizontal ? SpecialPieceType.Row : SpecialPieceType.Column;
            return SpecialPieceType.None;
        }

        internal static List<SpecialCreation> FindCreations(BoardState board, MatchResult matches,
            GridPosition? swapFrom, GridPosition? swapTo)
        {
            var result = new List<SpecialCreation>();
            foreach (var group in matches.Groups)
            {
                SpecialPieceType type = CreationType(group);
                if (type == SpecialPieceType.None) continue;
                GridPosition? chosen = Eligible(board, group, swapTo) ? swapTo :
                    Eligible(board, group, swapFrom) ? swapFrom : null;
                if (!chosen.HasValue)
                    foreach (var position in group.Positions)
                        if (Eligible(board, group, position)) { chosen = position; break; }
                if (!chosen.HasValue) continue;
                PieceState previous = board.GetPiece(chosen.Value);
                var piece = new PieceState(previous.Id, type == SpecialPieceType.ColorClear ? PieceColor.None : previous.Color, type);
                result.Add(new SpecialCreation(chosen.Value, piece));
            }
            return result;
        }

        private static bool Eligible(BoardState board, MatchGroup group, GridPosition? position)
        {
            if (!position.HasValue) return false;
            foreach (var matched in group.Positions)
                if (matched == position.Value) return board.GetPiece(matched)?.SpecialType == SpecialPieceType.None;
            return false;
        }
    }
}
