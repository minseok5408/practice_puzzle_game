using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Levels;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace PuzzleGame.Editor
{
    public static class CampaignValidation
    {
        public static void PrepareAndRun() { CampaignSetup.Apply(); Run(); }
        [MenuItem("Puzzle Game/Validate Campaign Routes")]
        public static void Run()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CampaignSetup.CatalogPath);
            var report = new StringBuilder("stage,seed,moves,target,successful_routes,attempts,best_score\n");
            var routes = new StringBuilder();
            var boards = new HashSet<string>();
            int failed = 0;
            for (int index = 1; index <= LevelCatalog.LevelCount; index++)
            {
                var level = catalog.Get(index);
                var board = BoardGenerator.Generate(8, 8, 6, level.Seed);
                var hash = new StringBuilder();
                for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) hash.Append((int)board.GetPiece(new GridPosition(x, y)).Color);
                if (level.Number != index || !boards.Add(hash.ToString()) || MatchFinder.FindMatches(board).Count > 0 || !MoveFinder.HasAnyMove(board))
                    throw new InvalidOperationException("Invalid initial board " + index);
                int wins = 0, attempts = 0, best = 0;
                for (; attempts < 16 || (wins == 0 && attempts < 128); attempts++)
                {
                    var result = Play(level, attempts, out string route);
                    best = Math.Max(best, result.Score);
                    if (result.Outcome != LevelOutcome.Won) continue;
                    if (wins++ == 0) routes.Append(index).Append('|').AppendLine(route);
                }
                if (wins == 0) failed++;
                report.AppendLine($"{level.DisplayName},{level.Seed},{level.CreateRules().StartingMoves},{level.CreateRules().TargetScore},{wins},{attempts},{best}");
            }
            Directory.CreateDirectory("Builds/Validation/campaign");
            File.WriteAllText("Builds/Validation/campaign/balance.csv", report.ToString());
            File.WriteAllText("Builds/Validation/campaign/winning-routes.txt", routes.ToString());
            if (failed > 0) throw new InvalidOperationException(failed + " stages need balance review; no winning route found.");
            Debug.Log("CAMPAIGN VALIDATION PASS: 50 unique stable boards, legal initial moves and reproducible winning routes for all 50 stages.");
        }

        private static LevelProgress Play(LevelDefinition level, int strategy, out string route)
        {
            var board = BoardGenerator.Generate(8, 8, 6, level.Seed);
            var random = new Random(level.Seed);
            var choices = new Random(7919 * strategy + 31);
            var resolver = new BoardResolver(board, 6, random);
            var progress = new LevelProgress(level.CreateRules());
            var inputs = new StringBuilder();
            while (!progress.IsFinished)
            {
                double best = double.MinValue;
                GridPosition first = default, second = default;
                for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) for (int direction = 0; direction < 2; direction++)
                {
                    var from = new GridPosition(x, y);
                    var to = new GridPosition(x + (direction == 0 ? 1 : 0), y + (direction == 1 ? 1 : 0));
                    if (!MoveFinder.IsValidSwap(board, from, to)) continue;
                    var preview = Copy(board); preview.SwapPieces(from, to);
                    var step = new BoardResolver(preview, 6, new Random(0)).ResolveMatches(preview, from, to);
                    double value = step.RemovedIds.Count * 10 + step.SpecialCreations.Count * 55;
                    foreach (var piece in step.RemovedPieces)
                        if (progress.Collected(piece.Color) < progress.Rules.CollectionTarget(piece.Color)) value += 10 + strategy % 4 * 5;
                    if (strategy > 0) value += choices.NextDouble() * (20 + strategy % 8 * 12);
                    if (value <= best) continue;
                    best = value; first = from; second = to;
                }
                if (best == double.MinValue) throw new InvalidOperationException("No move in stable board.");
                if (inputs.Length > 0) inputs.Append(';');
                inputs.Append($"{first.X},{first.Y},{second.X},{second.Y}");
                board.SwapPieces(first, second); progress.TryBeginMove();
                for (int cascade = 0; ; cascade++)
                {
                    if (cascade >= 128) throw new InvalidOperationException("Cascade limit");
                    var step = resolver.ResolveMatches(board, cascade == 0 ? first : (GridPosition?)null, cascade == 0 ? second : (GridPosition?)null);
                    if (step.RemovedIds.Count == 0) break;
                    progress.RecordRemoval(step);
                }
                if (!progress.WillFinish && !MoveFinder.HasAnyMove(board) && !BoardShuffler.TryShuffle(board, random))
                    throw new InvalidOperationException("Shuffle failed");
                progress.CompleteMove();
            }
            route = inputs.ToString(); return progress;
        }

        private static BoardState Copy(BoardState original)
        {
            var copy = new BoardState(8, 8);
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
            { var p = new GridPosition(x, y); copy.SetPiece(p, original.GetPiece(p)); }
            return copy;
        }
    }
}
