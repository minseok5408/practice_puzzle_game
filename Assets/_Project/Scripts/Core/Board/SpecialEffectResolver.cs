using System.Collections.Generic;

namespace PuzzleGame.Core.Board
{
    // Effects see the same pre-removal board. A piece can enter the effect queue only once.
    internal static class SpecialEffectResolver
    {
        internal static List<GridPosition> Resolve(BoardState board, MatchResult matches,
            ResolutionStep step, GridPosition? swapFrom, GridPosition? swapTo)
        {
            var protectedPositions = new HashSet<GridPosition>();
            foreach (var creation in step.SpecialCreations) protectedPositions.Add(creation.Position);
            var removed = new HashSet<GridPosition>();
            var queuedIds = new HashSet<int>();
            var queue = new Queue<GridPosition>();
            var targetColors = new Dictionary<int, PieceColor>();
            var fullBoardClears = new HashSet<int>();
            List<int> affected = step.InitialHitIds;

            void Hit(GridPosition position)
            {
                if (!board.Contains(position) || protectedPositions.Contains(position)) return;
                PieceState piece = board.GetPiece(position);
                if (piece == null) return;
                if (!affected.Contains(piece.Id)) affected.Add(piece.Id);
                removed.Add(position);
                if (piece.SpecialType != SpecialPieceType.None && queuedIds.Add(piece.Id)) queue.Enqueue(position);
            }

            if (swapFrom.HasValue && swapTo.HasValue)
            {
                PieceState a = board.GetPiece(swapFrom.Value), b = board.GetPiece(swapTo.Value);
                if (SpecialPieceRules.IsSpecialSwap(a, b))
                {
                    if (a.SpecialType == SpecialPieceType.ColorClear && b.SpecialType == SpecialPieceType.ColorClear)
                    {
                        // Both color clears are consumed by the full-board combination, with no extra color sweep.
                        targetColors[a.Id] = PieceColor.None;
                        targetColors[b.Id] = PieceColor.None;
                        fullBoardClears.Add(a.Id);
                        fullBoardClears.Add(b.Id);
                    }
                    else
                    {
                        if (a.SpecialType == SpecialPieceType.ColorClear) targetColors[a.Id] = b.Color;
                        if (b.SpecialType == SpecialPieceType.ColorClear) targetColors[b.Id] = a.Color;
                    }
                    Hit(swapFrom.Value);
                    Hit(swapTo.Value);
                }
            }
            foreach (var position in matches.Positions) Hit(position);
            while (queue.Count > 0)
            {
                GridPosition origin = queue.Dequeue();
                PieceState piece = board.GetPiece(origin);
                PieceColor target = PieceColor.None;
                if (piece.SpecialType == SpecialPieceType.ColorClear && !targetColors.TryGetValue(piece.Id, out target))
                    target = MostCommonColor(board);
                affected = new List<int>();
                switch (piece.SpecialType)
                {
                    case SpecialPieceType.Row:
                        for (int x = 0; x < board.Width; x++) Hit(new GridPosition(x, origin.Y));
                        break;
                    case SpecialPieceType.Column:
                        for (int y = 0; y < board.Height; y++) Hit(new GridPosition(origin.X, y));
                        break;
                    case SpecialPieceType.Bomb:
                        for (int y = origin.Y - 1; y <= origin.Y + 1; y++)
                            for (int x = origin.X - 1; x <= origin.X + 1; x++) Hit(new GridPosition(x, y));
                        break;
                    case SpecialPieceType.ColorClear:
                        if (target == PieceColor.None && !fullBoardClears.Contains(piece.Id)) break;
                        for (int y = 0; y < board.Height; y++)
                            for (int x = 0; x < board.Width; x++)
                            {
                                var position = new GridPosition(x, y);
                                if (fullBoardClears.Contains(piece.Id) || board.GetPiece(position)?.Color == target) Hit(position);
                            }
                        break;
                }
                step.SpecialActivations.Add(new SpecialActivation(piece.Id, origin, piece.SpecialType, target, affected.AsReadOnly()));
            }
            var ordered = new List<GridPosition>(removed);
            ordered.Sort(MatchResult.Compare);
            return ordered;
        }

        private static PieceColor MostCommonColor(BoardState board)
        {
            var counts = new int[7];
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                    counts[(int)(board.GetPiece(new GridPosition(x, y))?.Color ?? PieceColor.None)]++;
            PieceColor best = PieceColor.None;
            int max = 0;
            for (int color = 1; color < counts.Length; color++)
                if (counts[color] > max) { max = counts[color]; best = (PieceColor)color; }
            return best;
        }
    }
}
