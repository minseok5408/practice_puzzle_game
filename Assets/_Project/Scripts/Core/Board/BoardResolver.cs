using System;

namespace PuzzleGame.Core.Board
{
    // Mutates only the model; Unity replays the returned step before requesting another.
    public sealed class BoardResolver
    {
        private readonly Random random;
        private readonly int colorCount;
        private int nextId;

        public BoardResolver(BoardState board, int colors, Random random)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (colors < 3 || colors > 6) throw new ArgumentOutOfRangeException(nameof(colors));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            colorCount = colors;
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                    nextId = Math.Max(nextId, board.GetPiece(new GridPosition(x, y))?.Id ?? 0);
        }

        // Swap coordinates describe an already-swapped board, and apply only to its first step.
        public ResolutionStep ResolveMatches(BoardState board, GridPosition? swapFrom = null, GridPosition? swapTo = null)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (swapFrom.HasValue != swapTo.HasValue || (swapFrom.HasValue &&
                (!board.Contains(swapFrom.Value) || !board.Contains(swapTo.Value) || !swapFrom.Value.IsAdjacentTo(swapTo.Value))))
                throw new ArgumentException("Supply both adjacent swap coordinates, or neither for a cascade.");
            var step = new ResolutionStep();
            var matches = MatchFinder.FindGroups(board);
            step.SpecialCreations.AddRange(SpecialPieceRules.FindCreations(board, matches, swapFrom, swapTo));
            var removed = SpecialEffectResolver.Resolve(board, matches, step, swapFrom, swapTo);
            foreach (var creation in step.SpecialCreations) board.SetPiece(creation.Position, creation.Piece);
            foreach (var position in removed)
            {
                step.RemovedIds.Add(board.GetPiece(position).Id);
                board.SetPiece(position, null);
            }
            if (removed.Count == 0) return step;
            for (int x = 0; x < board.Width; x++)
            {
                int writeY = 0;
                for (int y = 0; y < board.Height; y++)
                {
                    var from = new GridPosition(x, y);
                    PieceState piece = board.GetPiece(from);
                    if (piece == null) continue;
                    var to = new GridPosition(x, writeY++);
                    if (from == to) continue;
                    board.SetPiece(to, piece);
                    board.SetPiece(from, null);
                    step.Moves.Add(new PieceMovement(piece.Id, from, to));
                }
                int spawnOffset = 0;
                while (writeY < board.Height)
                {
                    var to = new GridPosition(x, writeY++);
                    var from = new GridPosition(x, board.Height + spawnOffset++);
                    var piece = new PieceState(checked(++nextId), (PieceColor)random.Next(1, colorCount + 1));
                    board.SetPiece(to, piece);
                    step.Spawns.Add(new PieceMovement(piece.Id, from, to));
                }
            }
            return step;
        }
    }
}
