using System;
using System.Linq;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;

namespace PuzzleGame.Tests
{
    public sealed class ItemEffectTests
    {
        [TestCase(0,0,0,1)]
        [TestCase(0,0,1,4)]
        [TestCase(4,4,1,9)]
        public void ItemFootprintClipsAtEdgesAndRefills(int x,int y,int radius,int expected)
        {
            var board=BoardGenerator.Generate(8,8,6,20);
            for(int row=0;row<8;row++)for(int col=0;col<8;col++)board.SetCell(new GridPosition(col,row),new CellState(2));
            var step=new BoardResolver(board,6,new Random(7)).ResolveItem(board,new GridPosition(x,y),radius);
            Assert.That(step.RemovedIds,Is.Empty);Assert.That(step.SpecialCreations,Is.Empty);
            Assert.That(step.FrostDamage.All(d=>d.RemainingHealth==1),Is.True);
            Assert.That(step.FrostDamage.Count,Is.EqualTo(expected));
            for(int row=0;row<8;row++)for(int col=0;col<8;col++)Assert.That(board.GetPiece(new GridPosition(col,row)),Is.Not.Null);
        }
        [Test]
        public void HammerActivatesExistingSpecialAndOverlappingIceHitsCountOnce()
        {
            var board=BoardGenerator.Generate(8,8,6,20);var target=new GridPosition(3,3);
            board.SetPiece(target,new PieceState(100,PieceColor.Red,SpecialPieceType.Row));
            board.SetPiece(new GridPosition(4,3),new PieceState(101,PieceColor.Blue,SpecialPieceType.Bomb));
            for(int x=0;x<8;x++)if(x!=3 && x!=4)board.SetCell(new GridPosition(x,3),new CellState(2));
            var step=new BoardResolver(board,6,new Random(7)).ResolveItem(board,target,0);
            Assert.That(step.SpecialActivations.Count,Is.EqualTo(2));
            Assert.That(step.RemovedIds.Distinct().Count(),Is.EqualTo(step.RemovedIds.Count));
            Assert.That(step.FrostDamage.Count,Is.EqualTo(6));Assert.That(step.FrostDamage.All(d=>d.RemainingHealth==1),Is.True);
        }
        [Test]
        public void ItemResolutionScoresAndCollectsWithoutMovesAndRollsBack()
        {
            var p=new LevelProgress(new LevelRules(3,10,10,new[]{1,0,0,0,0,0},1));
            var step=new ResolutionStep();step.RemovedIds.Add(1);step.RemovedPieces.Add(new RemovedPiece(1,PieceColor.Red));step.FrostDamage.Add(new FrostDamage(new GridPosition(0,0),0));
            Assert.That(p.TryBeginItem(),Is.True);p.RecordRemoval(step);Assert.That(p.AddBonusMoves(5),Is.False);
            p.CancelMove();Assert.That(p.Score,Is.Zero);Assert.That(p.FrostCleared,Is.Zero);Assert.That(p.Collected(PieceColor.Red),Is.Zero);
            p.TryBeginItem();p.RecordRemoval(step);p.CompleteMove();Assert.That(p.Outcome,Is.EqualTo(LevelOutcome.Won));
            Assert.That(p.MovesRemaining,Is.EqualTo(3));Assert.That(p.MovesUsed,Is.Zero);Assert.That(p.TryBeginItem(),Is.False);
        }
        [Test]
        public void BonusMovesKeepRealMoveCountAndCannotReviveLoss()
        {
            var p=new LevelProgress(new LevelRules(1,1000,10));Assert.That(p.AddBonusMoves(5),Is.True);
            Assert.That(p.MovesRemaining,Is.EqualTo(6));Assert.That(p.MovesUsed,Is.Zero);
            for(int i=0;i<6;i++){p.TryBeginMove();p.CompleteMove();}
            Assert.That(p.Outcome,Is.EqualTo(LevelOutcome.Lost));Assert.That(p.MovesUsed,Is.EqualTo(6));Assert.That(p.AddBonusMoves(5),Is.False);
        }
    }
}
