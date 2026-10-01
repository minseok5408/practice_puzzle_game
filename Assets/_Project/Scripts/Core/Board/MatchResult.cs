using System.Collections.Generic;

namespace PuzzleGame.Core.Board
{
    public enum MatchDirection { Horizontal, Vertical }

    public sealed class MatchRun
    {
        public PieceColor Color { get; }
        public MatchDirection Direction { get; }
        public IReadOnlyList<GridPosition> Positions { get; }

        internal MatchRun(PieceColor color, MatchDirection direction, List<GridPosition> positions)
        { Color = color; Direction = direction; Positions = positions.AsReadOnly(); }
    }

    // Only runs sharing a cell belong to one group; adjacent parallel runs stay separate.
    public sealed class MatchGroup
    {
        public IReadOnlyList<MatchRun> Runs { get; }
        public IReadOnlyList<GridPosition> Positions { get; }
        public bool HasIntersection => Runs.Count > 1;

        internal MatchGroup(List<MatchRun> runs, List<GridPosition> positions)
        { Runs = runs.AsReadOnly(); Positions = positions.AsReadOnly(); }
    }

    public sealed class MatchResult
    {
        public IReadOnlyList<MatchGroup> Groups { get; }
        public IReadOnlyList<GridPosition> Positions { get; }

        internal MatchResult(List<MatchGroup> groups, List<GridPosition> positions)
        { Groups = groups.AsReadOnly(); Positions = positions.AsReadOnly(); }

        // Deterministic row-major order, starting at the bottom left.
        internal static int Compare(GridPosition a, GridPosition b) =>
            a.Y != b.Y ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X);
    }
}
