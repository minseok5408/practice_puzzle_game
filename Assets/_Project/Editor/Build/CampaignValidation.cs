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
            var report = new StringBuilder("stage,refill_seed,moves,target,frost_target,successful_routes,attempts,best_score,mean_moves_left_on_win\n");
            var routes = new StringBuilder();
            var boards = new HashSet<string>();
            int failed = 0;
            for (int index = 1; index <= LevelCatalog.LevelCount; index++)
            {
                var level = catalog.Get(index);
                var board = level.CreateBoard();
                var hash = new StringBuilder();
                for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) hash.Append((int)board.GetPiece(new GridPosition(x, y)).Color);
                if (level.Number != index || !boards.Add(hash.ToString()) || MatchFinder.FindMatches(board).Count > 0 || !MoveFinder.HasAnyMove(board))
                    throw new InvalidOperationException("Invalid initial board " + index);
                for (int variation = 0; variation < 3; variation++)
                {
                    int wins = 0, attempts = 0, best = 0, remaining = 0;
                    int minimum = variation == 0 ? 16 : 8;
                    for (; attempts < minimum || (wins == 0 && attempts < 128); attempts++)
                    {
                        var result = Play(level, attempts, out string route, variation);
                        best = Math.Max(best, result.Score);
                        if (result.Outcome != LevelOutcome.Won) continue;
                        remaining += result.MovesRemaining;
                        if (wins++ == 0 && variation == 0) routes.Append(index).Append('|').AppendLine(route);
                    }
                    if (wins == 0) failed++;
                    string average = (wins == 0 ? 0 : (float)remaining / wins).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                    report.AppendLine($"{level.DisplayName},{level.Seed + variation * 104729},{level.CreateRules().StartingMoves},{level.CreateRules().TargetScore},{level.CreateRules().FrostTarget},{wins},{attempts},{best},{average}");
                }
                Debug.Log("BALANCE stage " + index + "/50 checked with three refill seeds.");
            }
            Directory.CreateDirectory("Builds/Validation/frozen");
            File.WriteAllText("Builds/Validation/frozen/balance.csv", report.ToString());
            File.WriteAllText("Builds/Validation/frozen/winning-routes.txt", routes.ToString());
            if (failed > 0) throw new InvalidOperationException(failed + " stages need balance review; no winning route found.");
            Debug.Log("CAMPAIGN VALIDATION PASS: 50 unique stable boards, legal initial moves and reproducible winning routes for all 50 stages.");
        }

        private static LevelProgress Play(LevelDefinition level, int strategy, out string route, int variation = 0)
        {
            var board = level.CreateBoard();
            var random = new Random(level.Seed + variation * 104729);
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
                    if (progress.FrostCleared < progress.Rules.FrostTarget) value += step.FrostDamage.Count * (35 + strategy % 5 * 10);
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
                    if (step.RemovedIds.Count == 0 && step.FrostDamage.Count == 0) break;
                    progress.RecordRemoval(step);
                }
                bool blocked = !progress.WillFinish && !MoveFinder.HasAnyMove(board) && !BoardShuffler.TryShuffle(board, random);
                progress.CompleteMove(blocked);
            }
            route = inputs.ToString(); return progress;
        }

        private static BoardState Copy(BoardState original)
        {
            var copy = new BoardState(8, 8);
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
            { var p = new GridPosition(x, y); copy.SetPiece(p, original.GetPiece(p)); copy.SetCell(p, original.GetCell(p)); }
            return copy;
        }
    }
}
