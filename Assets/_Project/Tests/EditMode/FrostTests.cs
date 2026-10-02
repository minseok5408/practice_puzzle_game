using System;
using System.Linq;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;

namespace PuzzleGame.Tests
{
    public sealed class FrostTests
    {
        [Test]
        public void OrdinaryRemovalDamagesOnlyRemovedCellsAndNeverFalls()
        {
            var board=new BoardState(3,2);
            for(int y=0;y<2;y++)for(int x=0;x<3;x++)board.SetPiece(new GridPosition(x,y),new PieceState(y*3+x+1,y==0?PieceColor.Red:(PieceColor)(x+2)));
            board.SetCell(new GridPosition(1,0),new CellState(2));board.SetCell(new GridPosition(1,1),new CellState(1));
            var step=new BoardResolver(board,6,new Random(7)).ResolveMatches(board);
            Assert.That(step.FrostDamage.Count,Is.EqualTo(1));
            Assert.That(board.GetCell(new GridPosition(1,0)).FrostHealth,Is.EqualTo(1));
            Assert.That(board.GetCell(new GridPosition(1,1)).FrostHealth,Is.EqualTo(1));
        }
        [Test]
        public void OverlappingSpecialEffectsDamageEachCellOnlyOnce()
        {
            var board=BoardGenerator.Generate(8,8,6,11);
            var a=new GridPosition(2,2);var b=new GridPosition(3,2);
            board.SetPiece(a,new PieceState(100,PieceColor.Red,SpecialPieceType.Bomb));
            board.SetPiece(b,new PieceState(101,PieceColor.Green,SpecialPieceType.Bomb));
            for(int y=0;y<8;y++)for(int x=0;x<8;x++){var p=new GridPosition(x,y);if(p!=a && p!=b)board.SetCell(p,new CellState(2));}
            board.SwapPieces(a,b);
            var step=new BoardResolver(board,6,new Random(1)).ResolveMatches(board,a,b);
            Assert.That(step.RemovedIds.Count,Is.EqualTo(2));Assert.That(step.FrostDamage.Count,Is.GreaterThan(0));
            Assert.That(step.FrostDamage.Select(d=>d.Position).Distinct().Count(),Is.EqualTo(step.FrostDamage.Count));
            Assert.That(step.FrostDamage.All(d=>d.RemainingHealth==1),Is.True);
        }
        [Test]
        public void NewSpecialCanOnlyFormOnAnUnfrozenCandy()
        {
            var board=new BoardState(4,1);
            for(int x=0;x<4;x++){var p=new GridPosition(x,0);board.SetPiece(p,new PieceState(x+1,PieceColor.Red));board.SetCell(p,new CellState(1));}
            board.SetCell(new GridPosition(2,0),new CellState());
            var step=new BoardResolver(board,6,new Random(2)).ResolveMatches(board);
            Assert.That(step.SpecialCreations.Count,Is.EqualTo(1));
            Assert.That(step.SpecialCreations[0].Position,Is.EqualTo(new GridPosition(2,0)));
            Assert.That(step.FrostDamage.Count,Is.EqualTo(3));
        }
        [Test]
        public void ShuffleLeavesFrostAtTheSameCoordinates()
        {
            var board=BoardGenerator.Generate(8,8,6,1);var p=new GridPosition(3,6);board.SetCell(p,new CellState(2));
            var original=board.GetPiece(p);
            Assert.That(BoardShuffler.TryShuffle(board,new Random(9)),Is.True);
            Assert.That(board.GetCell(p).FrostHealth,Is.EqualTo(2));
            Assert.That(board.GetPiece(p),Is.SameAs(original));
        }
        [Test]
        public void FrostIsRequiredAndDuplicateStepCannotDoubleCount()
        {
            var p=new LevelProgress(new LevelRules(1,10,10,null,2));p.TryBeginMove();
            var step=new ResolutionStep();step.RemovedIds.Add(1);step.FrostDamage.Add(new FrostDamage(new GridPosition(0,0),0));
            p.RecordRemoval(step);p.RecordRemoval(step);p.CompleteMove();
            Assert.That(p.FrostCleared,Is.EqualTo(1));Assert.That(p.Outcome,Is.EqualTo(LevelOutcome.Lost));
        }
        [Test]
        public void LastMoveFrostGoalWinsAndCancellationRestoresIt()
        {
            var p=new LevelProgress(new LevelRules(1,10,10,null,1));p.TryBeginMove();
            var step=new ResolutionStep();step.RemovedIds.Add(1);step.FrostDamage.Add(new FrostDamage(new GridPosition(0,0),0));
            p.RecordRemoval(step);p.CancelMove();
            Assert.That(p.FrostCleared,Is.Zero);Assert.That(p.MovesRemaining,Is.EqualTo(1));
            p.TryBeginMove();p.RecordRemoval(step);p.CompleteMove();Assert.That(p.Outcome,Is.EqualTo(LevelOutcome.Won));
        }
    }
}
