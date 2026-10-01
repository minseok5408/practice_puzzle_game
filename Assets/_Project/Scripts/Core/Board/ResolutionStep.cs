using System.Collections.Generic;

namespace PuzzleGame.Core.Board
{
    public sealed class ResolutionStep
    {
        public List<int> RemovedIds { get; } = new List<int>();
        public List<int> InitialHitIds { get; } = new List<int>();
        public List<PieceMovement> Moves { get; } = new List<PieceMovement>();
        public List<PieceMovement> Spawns { get; } = new List<PieceMovement>();
        public List<SpecialCreation> SpecialCreations { get; } = new List<SpecialCreation>();
        public List<SpecialActivation> SpecialActivations { get; } = new List<SpecialActivation>();
    }

    public readonly struct SpecialCreation
    {
        public GridPosition Position { get; }
        public PieceState Piece { get; }
        public SpecialCreation(GridPosition position, PieceState piece) { Position = position; Piece = piece; }
    }

    public readonly struct SpecialActivation
    {
        public int PieceId { get; }
        public GridPosition Position { get; }
        public SpecialPieceType Type { get; }
        public PieceColor TargetColor { get; }
        public IReadOnlyList<int> AffectedIds { get; }
        public SpecialActivation(int id, GridPosition position, SpecialPieceType type, PieceColor target,
            IReadOnlyList<int> affectedIds)
        { PieceId = id; Position = position; Type = type; TargetColor = target; AffectedIds = affectedIds; }
    }

    public readonly struct PieceMovement
    {
        public int PieceId { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }
        public PieceMovement(int id, GridPosition from, GridPosition to)
        { PieceId = id; From = from; To = to; }
    }
}
