using System;
using System.Collections.Generic;

namespace PuzzleGame.Core.Board
{
    public static class MatchFinder
    {
        // Ordered coordinates; each matched cell is returned once, including crossings.
        public static List<GridPosition> FindMatches(BoardState board)
            => new List<GridPosition>(FindGroups(board).Positions);

        public static MatchResult FindGroups(BoardState board)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            var runs = new List<MatchRun>();
            for (int y = 0; y < board.Height; y++)
                FindRuns(board, new GridPosition(0, y), MatchDirection.Horizontal, board.Width, runs);
            for (int x = 0; x < board.Width; x++)
                FindRuns(board, new GridPosition(x, 0), MatchDirection.Vertical, board.Height, runs);

            var groups = new List<MatchGroup>();
            var allPositions = new HashSet<GridPosition>();
            var visited = new HashSet<MatchRun>();
            foreach (var run in runs)
            {
                if (!visited.Add(run)) continue;
                var connected = new List<MatchRun> { run };
                var positions = new HashSet<GridPosition>(run.Positions);
                for (int index = 0; index < connected.Count; index++)
                {
                    foreach (var candidate in runs)
                    {
                        if (visited.Contains(candidate) || !positions.Overlaps(candidate.Positions)) continue;
                        visited.Add(candidate);
                        connected.Add(candidate);
                        positions.UnionWith(candidate.Positions);
                    }
                }
                var ordered = new List<GridPosition>(positions);
                ordered.Sort(MatchResult.Compare);
                groups.Add(new MatchGroup(connected, ordered));
                allPositions.UnionWith(positions);
            }
            groups.Sort((a, b) => MatchResult.Compare(a.Positions[0], b.Positions[0]));
            var matches = new List<GridPosition>(allPositions);
            matches.Sort(MatchResult.Compare);
            return new MatchResult(groups, matches);
        }

        private static void FindRuns(BoardState board, GridPosition start, MatchDirection direction,
            int length, List<MatchRun> runs)
        {
            bool horizontal = direction == MatchDirection.Horizontal;
            int offset = 0;
            while (offset < length)
            {
                var position = new GridPosition(start.X + (horizontal ? offset : 0), start.Y + (horizontal ? 0 : offset));
                PieceColor color = board.GetPiece(position)?.Color ?? PieceColor.None;
                int count = color == PieceColor.None ? 1 : 1 + Count(board, position, horizontal ? 1 : 0, horizontal ? 0 : 1, color);
                if (count >= 3)
                {
                    var positions = new List<GridPosition>();
                    for (int i = 0; i < count; i++)
                        positions.Add(new GridPosition(position.X + (horizontal ? i : 0), position.Y + (horizontal ? 0 : i)));
                    runs.Add(new MatchRun(color, direction, positions));
                }
                offset += count;
            }
        }

        public static bool HasMatchAt(BoardState board, GridPosition position)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (!board.Contains(position)) return false;
            PieceState piece = board.GetPiece(position);
            if (piece == null || piece.Color == PieceColor.None) return false;
            return 1 + Count(board, position, 1, 0, piece.Color) + Count(board, position, -1, 0, piece.Color) >= 3
                || 1 + Count(board, position, 0, 1, piece.Color) + Count(board, position, 0, -1, piece.Color) >= 3;
        }

        private static int Count(BoardState board, GridPosition origin, int dx, int dy, PieceColor color)
        {
            int count = 0;
            var next = new GridPosition(origin.X + dx, origin.Y + dy);
            while (board.Contains(next) && board.GetPiece(next)?.Color == color)
            {
                count++;
                next = new GridPosition(next.X + dx, next.Y + dy);
            }
            return count;
        }
    }
}
