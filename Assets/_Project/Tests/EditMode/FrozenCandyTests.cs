using System;
using System.Linq;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;

namespace PuzzleGame.Tests
{
    public sealed class FrozenCandyTests
    {
        [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void EveryIceLayerAbsorbsOneHitAndKeepsTheSameCandy(int layers)
        {
            var board=BoardGenerator.Generate(8,8,6,20);var at=new GridPosition(3,3);var original=board.GetPiece(at);
            board.SetCell(at,new CellState(layers));var resolver=new BoardResolver(board,6,new Random(4));
            var progress=new LevelProgress(new LevelRules(20,9999,10,null,1));
            for(int remaining=layers-1;remaining>=0;remaining--)
            {
                progress.TryBeginItem();var step=resolver.ResolveItem(board,at,0);progress.RecordRemoval(step);progress.CompleteMove();
                Assert.That(step.RemovedIds,Is.Empty);Assert.That(step.Moves,Is.Empty);Assert.That(step.Spawns,Is.Empty);
                Assert.That(step.FrostDamage.Single().RemainingHealth,Is.EqualTo(remaining));Assert.That(board.GetPiece(at),Is.SameAs(original));
                Assert.That(progress.Score,Is.Zero);Assert.That(progress.FrostCleared,Is.EqualTo(remaining==0?1:0));
            }
            var freed=resolver.ResolveItem(board,at,0);Assert.That(freed.RemovedIds,Has.Member(original.Id));
        }
        [Test] public void FrozenSpecialCannotSwapOrActivateUntilAfterThawing()
        {
            var board=BoardGenerator.Generate(8,8,6,20);var at=new GridPosition(3,3);var other=new GridPosition(4,3);
            board.SetPiece(at,new PieceState(100,PieceColor.None,SpecialPieceType.ColorClear));board.SetCell(at,new CellState(1));
            Assert.That(MoveFinder.IsValidSwap(board,at,other),Is.False);Assert.That(MoveFinder.IsValidSwap(board,other,at),Is.False);
            Assert.Throws<InvalidOperationException>(()=>board.SwapPieces(at,other));
            var resolver=new BoardResolver(board,6,new Random(4));var thaw=resolver.ResolveItem(board,at,0);
            Assert.That(thaw.SpecialActivations,Is.Empty);Assert.That(thaw.RemovedIds,Is.Empty);
            Assert.That(MoveFinder.IsValidSwap(board,at,other),Is.True);
            var hit=resolver.ResolveItem(board,at,0);Assert.That(hit.SpecialActivations.Count,Is.EqualTo(1));Assert.That(hit.RemovedIds,Has.Member(100));
        }
        [Test] public void GravityPartitionsAtIceAndRefillsSealedPocketsInPlace()
        {
            var board=ColumnBoard();var ice=new GridPosition(0,2);var below=board.GetPiece(new GridPosition(0,1));var above=board.GetPiece(new GridPosition(0,3));var frozen=board.GetPiece(ice);
            board.SetCell(ice,new CellState(2));var step=new BoardResolver(board,6,new Random(6)).ResolveItem(board,new GridPosition(0,0),0);
            Assert.That(board.GetPiece(ice),Is.SameAs(frozen));Assert.That(board.GetPiece(new GridPosition(0,3)),Is.SameAs(above));
            Assert.That(board.GetPiece(new GridPosition(0,0)),Is.SameAs(below));
            Assert.That(step.Moves.Any(m=>m.PieceId==frozen.Id || m.PieceId==above.Id),Is.False);
            var spawn=step.Spawns.Single();Assert.That(spawn.To,Is.EqualTo(new GridPosition(0,1)));Assert.That(spawn.From,Is.EqualTo(spawn.To));
        }
        [Test] public void LastLayerOpensGravityPassageInThatResolution()
        {
            var board=ColumnBoard();var at=new GridPosition(0,2);var original=board.GetPiece(at);board.SetCell(at,new CellState(1));
            var step=new BoardResolver(board,6,new Random(3)).ResolveItem(board,new GridPosition(0,1),1);
            Assert.That(board.GetCell(at).FrostHealth,Is.Zero);Assert.That(board.GetPiece(new GridPosition(0,0)),Is.SameAs(original));
            Assert.That(step.RemovedIds,Has.No.Member(original.Id));Assert.That(step.Moves.Any(m=>m.PieceId==original.Id && m.From==at),Is.True);
        }
        [Test] public void FailedShuffleRestoresFreeCandiesAndCannotRelocateFrozenOnes()
        {
            var board=BoardGenerator.Generate(8,8,6,3);var ids=new int[64];
            for(int i=0;i<64;i++){var p=new GridPosition(i%8,i/8);ids[i]=board.GetPiece(p).Id;if(i!=0 && i!=63)board.SetCell(p,new CellState(2));}
            Assert.That(BoardShuffler.TryShuffle(board,new Random(9),4),Is.False);Assert.That(MoveFinder.HasAnyMove(board),Is.False);
            for(int i=0;i<64;i++){var p=new GridPosition(i%8,i/8);Assert.That(board.GetPiece(p).Id,Is.EqualTo(ids[i]));Assert.That(board.IsFrozen(p),Is.EqualTo(i!=0 && i!=63));}
        }
        [Test] public void FullyFrozenMatchesDoNotRepeatOrCreateSpecialCandies()
        {
            var board=new BoardState(4,1);
            for(int x=0;x<4;x++){var at=new GridPosition(x,0);board.SetPiece(at,new PieceState(x+1,PieceColor.Red));board.SetCell(at,new CellState(3));}
            Assert.That(MatchFinder.FindMatches(board),Is.Empty);
            var step=new BoardResolver(board,6,new Random(7)).ResolveMatches(board);
            Assert.That(step.FrostDamage,Is.Empty);Assert.That(step.SpecialCreations,Is.Empty);Assert.That(step.RemovedIds,Is.Empty);
        }
        [Test] public void BlockedBoardEndsAttemptButAlreadyMetGoalsStillWin()
        {
            var progress=new LevelProgress(new LevelRules(5,1000,10));progress.TryBeginMove();progress.CompleteMove(true);
            Assert.That(progress.Outcome,Is.EqualTo(LevelOutcome.Lost));Assert.That(progress.IsBlocked,Is.True);Assert.That(progress.MovesRemaining,Is.EqualTo(4));
            var won=new LevelProgress(new LevelRules(5,10,10));won.TryBeginMove();won.RecordRemovedPieces(new[]{1});won.CompleteMove(true);
            Assert.That(won.Outcome,Is.EqualTo(LevelOutcome.Won));Assert.That(won.IsBlocked,Is.False);
        }
        private static BoardState ColumnBoard()
        {
            var board=new BoardState(3,6);
            for(int y=0;y<6;y++)for(int x=0;x<3;x++)board.SetPiece(new GridPosition(x,y),new PieceState(y*3+x+1,(PieceColor)((x+y)%6+1)));
            return board;
        }
    }
}
