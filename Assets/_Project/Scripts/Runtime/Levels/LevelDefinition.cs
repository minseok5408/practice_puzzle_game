using PuzzleGame.Core.Levels;
using PuzzleGame.Core.Board;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Runtime.Levels
{
    [CreateAssetMenu(menuName = "Puzzle Game/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string levelId = "level_001";
        [SerializeField] private string displayName = "1-1";
        [SerializeField, Range(1, LevelCatalog.LevelCount)] private int number = 1;
        [SerializeField] private int seed = 5408;
        [SerializeField] private int[] collectionTargets = new int[6];
        [SerializeField, Min(1)] private int startingMoves = 20;
        [SerializeField, Min(1)] private int targetScore = 1000;
        [SerializeField, Min(1)] private int pointsPerPiece = 10;
        [Serializable] public struct FrostCell { public int x, y, health; public FrostCell(int x, int y, int health) { this.x=x; this.y=y; this.health=health; } }
        [SerializeField] private FrostCell[] frostCells = Array.Empty<FrostCell>();
        [SerializeField] private PieceColor[] initialColors = Array.Empty<PieceColor>();
        [SerializeField, Min(0)] private int frostTarget;
        public int FrostCount => frostCells?.Length ?? 0;
        public string LevelId => levelId;
        public string DisplayName => displayName;
        public int Number => number;
        public int World => (number - 1) / LevelCatalog.StagesPerWorld + 1;
        public int Stage => (number - 1) % LevelCatalog.StagesPerWorld + 1;
        public int Seed => seed;
        public LevelRules CreateRules() => new LevelRules(startingMoves, targetScore, pointsPerPiece, collectionTargets, frostTarget);

        public BoardState CreateBoard()
        {
            Validate();
            var board = BoardGenerator.Generate(8, 8, 6, seed);
            if (initialColors != null && initialColors.Length != 0)
                for (int y=0; y<8; y++) for (int x=0; x<8; x++)
                    board.SetPiece(new GridPosition(x,y), new PieceState(y*8+x+1, initialColors[y*8+x]));
            foreach (var frost in frostCells ?? Array.Empty<FrostCell>())
                board.SetCell(new GridPosition(frost.x,frost.y), new CellState(frost.health));
            if (MatchFinder.FindMatches(board).Count != 0 || !MoveFinder.HasAnyMove(board))
                throw new InvalidOperationException(LevelId + ": initial board must be stable and playable.");
            return board;
        }

        public void ConfigureLayout(FrostCell[] frost, int target, PieceColor[] colors = null)
        {
            frostCells = frost == null ? Array.Empty<FrostCell>() : (FrostCell[])frost.Clone();
            initialColors = colors == null ? Array.Empty<PieceColor>() : (PieceColor[])colors.Clone();
            frostTarget = target;
            Validate();
        }

        public void Validate()
        {
            _ = CreateRules();
            if (number < 1 || number > LevelCatalog.LevelCount || levelId != "level_" + number.ToString("000"))
                throw new InvalidOperationException("Level number and permanent ID must agree.");
            var positions = new HashSet<GridPosition>();
            foreach (var frost in frostCells ?? Array.Empty<FrostCell>())
                if (frost.x<0 || frost.x>=8 || frost.y<0 || frost.y>=8 || frost.health<1 || frost.health>3 || !positions.Add(new GridPosition(frost.x,frost.y)))
                    throw new InvalidOperationException(LevelId + ": invalid or duplicate Frost cell.");
            if (frostTarget > positions.Count) throw new InvalidOperationException(LevelId + ": Frost target exceeds placement count.");
            if (initialColors == null || initialColors.Length == 0) return;
            if (initialColors.Length != 64) throw new InvalidOperationException("Initial layout must contain 64 colors, from bottom left by row.");
            foreach (var color in initialColors)
                if (color < PieceColor.Red || color > PieceColor.Purple) throw new InvalidOperationException("Invalid initial color.");
        }

        public void ConfigureCampaign(int index, int boardSeed, int moves, int score, int[] targets)
        {
            if (index < 1 || index > LevelCatalog.LevelCount) throw new System.ArgumentOutOfRangeException(nameof(index));
            _ = new LevelRules(moves, score, 10, targets);
            number = index; seed = boardSeed; startingMoves = moves; targetScore = score; pointsPerPiece = 10;
            levelId = "level_" + index.ToString("000"); displayName = World + "-" + Stage;
            collectionTargets = (int[])targets.Clone();
        }
    }
}
