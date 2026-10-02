using System;
using System.IO;
using PuzzleGame.Runtime.Services;
using UnityEditor;
using UnityEngine;

namespace PuzzleGame.Editor
{
    public static class PlaytestReport
    {
        [MenuItem("Puzzle Game/Analyze Playtest Log")]
        public static void SelectLog()
        {
            string path=EditorUtility.OpenFilePanel("Playtest JSONL",Application.persistentDataPath,"jsonl");
            if(!string.IsNullOrEmpty(path))Export(path,"Builds/Reports/Playtest");
        }
        public static void Run()
        {
            string[] args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-puzzleJournal");
            if(i<0 || i+1>=args.Length)throw new ArgumentException("Supply -puzzleJournal <JSONL>.");
            Export(args[i+1],"Builds/Reports/Playtest");
        }
        public static void Export(string path,string folder)
        {
            var report=PlaytestAnalysis.Read(File.ReadLines(path));Directory.CreateDirectory(folder);report.WriteCsv(Path.Combine(folder,"stages.csv"));
            File.WriteAllText(Path.Combine(folder,"readme.txt"),$"Source: {path}\nGroups: {report.Rows.Count}\nInvalid rows: {report.InvalidLines}\nExcluded P clears: {report.ExcludedEasterEggs}\nDuplicate attempts: {report.Duplicates}\nFeedback rows: {report.FeedbackLines}\n\nfinished_win_rate = wins / (wins + losses); abandoned attempts are shown separately.\nNever combine player, automated, editor or legacy sources, rule versions, or item/no-item cohorts.\nLegacy source is unknown, not verified human data. Item means include abandoned attempts. Feedback is joined by attempt ID and never counted as another attempt.\n");
            Debug.Log("PLAYTEST REPORT: "+Path.GetFullPath(folder));
        }
    }
}
