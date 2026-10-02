using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Levels;
using UnityEditor;
using UnityEngine;
using Random=System.Random;

namespace PuzzleGame.Editor
{
    // Paired deterministic probes, not estimates of human success rates.
    public static class BalanceAudit
    {
        [MenuItem("Puzzle Game/Audit Item Balance")]
        public static void Run()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<LevelCatalog>(CampaignSetup.CatalogPath);
            string folder="Builds/Validation/frozen";Directory.CreateDirectory(folder);
            var csv=new StringBuilder("stage,scenario,item_budget,attempts,wins,win_rate,mean_moves_left_on_win,mean_items,hammer,bomb,shuffle,extra_moves,score_shortfall,collection_shortfall,frost_shortfall\n");
            for(int number=1;number<=50;number++)
            {
                bool focus=number==46 || number==49;
                foreach(int extra in focus?new[]{0,2,4}:new[]{0})foreach(int budget in new[]{0,2})
                {
                    int trials=focus?32:8,wins=0,left=0,used=0,attempts=trials*3;var items=new int[4];
                    int scoreShort=0,collectionShort=0,frostShort=0;
                    for(int variation=0;variation<3;variation++)for(int strategy=0;strategy<trials;strategy++)
                    {
                        var p=Play(catalog.Get(number),strategy,variation,extra,budget,out var consumed);
                        if(p.Outcome==LevelOutcome.Won){wins++;left+=p.MovesRemaining;}
                        else
                        {
                            if(p.Score<p.Rules.TargetScore)scoreShort++;
                            if(p.FrostCleared<p.Rules.FrostTarget)frostShort++;
                            if(Enumerable.Range(1,6).Any(c=>p.Collected((PieceColor)c)<p.Rules.CollectionTarget((PieceColor)c)))collectionShort++;
                        }
                        for(int i=0;i<4;i++){items[i]+=consumed[i];used+=consumed[i];}
                    }
                    string N(double value)=>value.ToString("F3",CultureInfo.InvariantCulture);
                    csv.AppendLine($"{catalog.Get(number).DisplayName},{(extra==0?"current":"moves+"+extra)},{budget},{attempts},{wins},{N((double)wins/attempts)},{N(wins==0?0:(double)left/wins)},{N((double)used/attempts)},{string.Join(",",items)},{scoreShort},{collectionShort},{frostShort}");
                }
                Debug.Log("ITEM BALANCE: "+number+"/50");
            }
            File.WriteAllText(Path.Combine(folder,"balance-items.csv"),csv.ToString());
            Debug.Log("ITEM BALANCE AUDIT COMPLETE: paired seeds; at most two items, at most one of each per attempt.");
        }

        private static LevelProgress Play(LevelDefinition level,int strategy,int variation,int extra,int budget,out int[] used)
        {
            var rules=level.CreateRules();var progress=new LevelProgress(new LevelRules(rules.StartingMoves+extra,rules.TargetScore,rules.PointsPerPiece,
                Enumerable.Range(1,6).Select(c=>rules.CollectionTarget((PieceColor)c)).ToArray(),rules.FrostTarget));
            var board=level.CreateBoard();var random=new Random(level.Seed+variation*104729);var choices=new Random(strategy*7919+31);
            var resolver=new BoardResolver(board,6,random);used=new int[4];
            while(!progress.IsFinished)
            {
                // Assist near the end of an attempt. Probe a modest budget instead of unlimited inventory.
                bool itemTurn=false;
                if(budget>0 && progress.MovesRemaining<=4)
                {
                    if(used[3]==0 && progress.MovesRemaining==1)
                    {progress.AddBonusMoves(5);used[3]++;budget--;}
                    else if(used[1]==0 && progress.FrostCleared<rules.FrostTarget)
                    {
                        GridPosition target=default;double best=0;
                        for(int y=0;y<8;y++)for(int x=0;x<8;x++)
                        {
                            var copy=Copy(board);var at=new GridPosition(x,y);
                            var hit=new BoardResolver(copy,6,new Random(0)).ResolveItem(copy,at,1);
                            double value=Value(hit,progress,strategy);if(value>best){best=value;target=at;}
                        }
                        progress.TryBeginItem();progress.RecordRemoval(resolver.ResolveItem(board,target,1));used[1]++;budget--;itemTurn=true;
                    }
                    else if(used[0]==0 && progress.MovesRemaining<=2)
                    {
                        GridPosition target=default;double best=0;
                        for(int y=0;y<8;y++)for(int x=0;x<8;x++)
                        {var p=new GridPosition(x,y);double v=board.GetCell(p).FrostHealth>0?100:0;var piece=board.GetPiece(p);if(progress.Collected(piece.Color)<rules.CollectionTarget(piece.Color))v+=30;if(piece.SpecialType!=SpecialPieceType.None)v+=60;if(v>best){best=v;target=p;}}
                        progress.TryBeginItem();progress.RecordRemoval(resolver.ResolveItem(board,target,0));used[0]++;budget--;itemTurn=true;
                    }
                }
                GridPosition first=default,second=default;
                if(!itemTurn)
                {
                    double best=double.MinValue;
                    for(int y=0;y<8;y++)for(int x=0;x<8;x++)for(int direction=0;direction<2;direction++)
                    {
                        var a=new GridPosition(x,y);var b=new GridPosition(x+(direction==0?1:0),y+(direction==1?1:0));
                        if(!MoveFinder.IsValidSwap(board,a,b))continue;
                        var copy=Copy(board);copy.SwapPieces(a,b);var step=new BoardResolver(copy,6,new Random(0)).ResolveMatches(copy,a,b);
                        double value=Value(step,progress,strategy)+(strategy==0?0:choices.NextDouble()*(20+strategy%8*12));
                        if(value>best){best=value;first=a;second=b;}
                    }
                    if(best==double.MinValue)throw new InvalidOperationException("No move: "+level.LevelId);
                    board.SwapPieces(first,second);progress.TryBeginMove();
                }
                for(int cascade=0;;cascade++)
                {
                    if(cascade>=128)throw new InvalidOperationException("Cascade limit");
                    var step=resolver.ResolveMatches(board,!itemTurn && cascade==0?first:(GridPosition?)null,!itemTurn && cascade==0?second:(GridPosition?)null);
                    if(step.RemovedIds.Count==0 && step.FrostDamage.Count==0)break;progress.RecordRemoval(step);
                }
                bool blocked=!progress.WillFinish && !MoveFinder.HasAnyMove(board) && !BoardShuffler.TryShuffle(board,random);
                progress.CompleteMove(blocked);
            }
            return progress;
        }
        private static double Value(ResolutionStep step,LevelProgress p,int strategy)
        {
            double value=step.RemovedIds.Count*10+step.SpecialCreations.Count*55;
            if(p.FrostCleared<p.Rules.FrostTarget)value+=step.FrostDamage.Count*(35+strategy%5*10);
            foreach(var piece in step.RemovedPieces)if(p.Collected(piece.Color)<p.Rules.CollectionTarget(piece.Color))value+=10+strategy%4*5;
            return value;
        }
        private static BoardState Copy(BoardState original)
        {
            var copy=new BoardState(original.Width,original.Height);
            for(int y=0;y<original.Height;y++)for(int x=0;x<original.Width;x++){var p=new GridPosition(x,y);copy.SetPiece(p,original.GetPiece(p));copy.SetCell(p,original.GetCell(p));}
            return copy;
        }
    }
}
