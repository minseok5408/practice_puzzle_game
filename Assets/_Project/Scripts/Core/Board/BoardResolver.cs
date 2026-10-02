using System;
using System.Collections.Generic;

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
            return RemoveAndRefill(board,step,removed);
        }

        public ResolutionStep ResolveItem(BoardState board,GridPosition target,int radius)
        {
            if(board==null)throw new ArgumentNullException(nameof(board));
            if(!board.Contains(target) || board.GetPiece(target)==null)throw new ArgumentOutOfRangeException(nameof(target));
            if(radius<0 || radius>1)throw new ArgumentOutOfRangeException(nameof(radius));
            var hits=new List<GridPosition>();
            for(int y=target.Y-radius;y<=target.Y+radius;y++)for(int x=target.X-radius;x<=target.X+radius;x++)hits.Add(new GridPosition(x,y));
            var step=new ResolutionStep();
            // Item hits trigger existing special candies but never create a special just for the item footprint.
            var removed=SpecialEffectResolver.Resolve(board,new MatchResult(new List<MatchGroup>(),new List<GridPosition>()),step,null,null,hits);
            return RemoveAndRefill(board,step,removed);
        }

        private ResolutionStep RemoveAndRefill(BoardState board,ResolutionStep step,List<GridPosition> removed)
        {
            foreach (var position in removed)
            {
                int frost = board.GetCell(position).FrostHealth;
                if (frost > 0)
                {
                    board.SetCell(position, new CellState(frost - 1));
                    step.FrostDamage.Add(new FrostDamage(position, frost - 1));
                    // The ice absorbs the hit, including its last layer. The same candy is released.
                    continue;
                }
                step.RemovedIds.Add(board.GetPiece(position).Id);
                step.RemovedPieces.Add(new RemovedPiece(board.GetPiece(position).Id, board.GetPiece(position).Color));
                board.SetPiece(position, null);
            }
            if (removed.Count == 0) return step;
            for (int x = 0; x < board.Width; x++)
            {
                int bottom = 0;
                for (int boundary = 0; boundary <= board.Height; boundary++)
                {
                    if (boundary < board.Height && !board.IsFrozen(new GridPosition(x,boundary))) continue;
                    int writeY = bottom;
                    for (int y = bottom; y < boundary; y++)
                    {
                        var from = new GridPosition(x,y);var piece = board.GetPiece(from);
                        if (piece == null) continue;
                        var to = new GridPosition(x,writeY++);
                        if (from == to) continue;
                        board.SetPiece(to,piece);board.SetPiece(from,null);step.Moves.Add(new PieceMovement(piece.Id,from,to));
                    }
                    int spawnOffset = 0;
                    while (writeY < boundary)
                    {
                        var to = new GridPosition(x,writeY++);
                        // Sealed pockets refill in place, never by falling through their frozen ceiling.
                        var from = boundary == board.Height ? new GridPosition(x,board.Height+spawnOffset++) : to;
                        var piece = new PieceState(checked(++nextId),(PieceColor)random.Next(1,colorCount+1));
                        board.SetPiece(to,piece);step.Spawns.Add(new PieceMovement(piece.Id,from,to));
                    }
                    bottom = boundary+1;
                }
            }
            return step;
        }
    }
}
