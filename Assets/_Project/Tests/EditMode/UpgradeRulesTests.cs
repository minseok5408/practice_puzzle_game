using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;

namespace PuzzleGame.Tests
{
    public sealed class UpgradeRulesTests
    {
        [Test]
        public void RatingRequiresSuccessAndBonusMovesCannotBuyEfficiency()
        {
            var progress=new LevelProgress(new LevelRules(20,100,10));
            Assert.That(StageRating.Stars(progress,0),Is.Zero);
            progress.AddBonusMoves(5);
            for(int i=0;i<17;i++){progress.TryBeginMove();progress.CompleteMove();}
            progress.CompleteImmediately();
            Assert.That(progress.MovesRemaining,Is.EqualTo(8));
            Assert.That(StageRating.IsEfficient(progress),Is.False);
            Assert.That(StageRating.Stars(progress,1),Is.EqualTo(1));
            Assert.That(StageRating.Stars(progress,0),Is.EqualTo(2));
        }
        [TestCase(20,4)] [TestCase(26,6)] [TestCase(31,7)]
        public void ReserveUsesTwentyPercentOfOriginalMoves(int moves,int expected)
        {
            Assert.That(StageRating.ReserveTarget(moves),Is.EqualTo(expected));
            var progress=new LevelProgress(new LevelRules(moves,100,10));
            for(int i=0;i<moves-expected;i++){progress.TryBeginMove();progress.CompleteMove();}
            progress.CompleteImmediately();Assert.That(StageRating.Stars(progress,0),Is.EqualTo(3));
        }
        [Test]
        public void GoalHintLeavesEveryPieceAndIceLayerUntouched()
        {
            var board=BoardGenerator.Generate(8,8,6,5408);board.SetCell(new GridPosition(0,0),new CellState(3));
            var before=new PieceState[8,8];for(int y=0;y<8;y++)for(int x=0;x<8;x++)before[x,y]=board.GetPiece(new GridPosition(x,y));
            var progress=new LevelProgress(new LevelRules(20,1000,10));
            Assert.That(GoalMoveFinder.TryFindMove(board,progress,out var a,out var b),Is.True);
            Assert.That(MoveFinder.IsValidSwap(board,a,b),Is.True);
            Assert.That(a,Is.Not.EqualTo(new GridPosition(0,0)));Assert.That(b,Is.Not.EqualTo(new GridPosition(0,0)));
            for(int y=0;y<8;y++)for(int x=0;x<8;x++)Assert.That(board.GetPiece(new GridPosition(x,y)),Is.SameAs(before[x,y]));
            Assert.That(board.GetCell(new GridPosition(0,0)).FrostHealth,Is.EqualTo(3));Assert.That(progress.MovesUsed,Is.Zero);
            GoalMoveFinder.TryFindMove(board,progress,out var againA,out var againB);Assert.That(againA,Is.EqualTo(a));Assert.That(againB,Is.EqualTo(b));
        }
    }
}
