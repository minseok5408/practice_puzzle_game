using System;
using System.Collections.Generic;
using System.IO;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Levels;
using UnityEngine;

namespace PuzzleGame.Runtime.Services
{
    public static class PlaytestJournal
    {
        private static PlaytestRecord active, last;
        private static readonly List<string> inputs=new List<string>();
        public static string PathForJournal => string.IsNullOrEmpty(CampaignProgress.SavePath) ? null
            : Path.Combine(Path.GetDirectoryName(Path.GetFullPath(CampaignProgress.SavePath)),"playtest.jsonl");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset(){active=last=null;inputs.Clear();}
        public static void Begin(LevelDefinition level)
        {
            bool automated=Application.isBatchMode || Array.Exists(Environment.GetCommandLineArgs(),a=>a.StartsWith("-puzzle",StringComparison.Ordinal));
            active=new PlaytestRecord{attemptId=Guid.NewGuid().ToString("N"),source=Application.isEditor?"editor":automated?"automated":"player",recordedUtc=DateTime.UtcNow.ToString("O"),levelId=level.LevelId,seed=level.Seed};inputs.Clear();
        }
        public static void Tick(float delta){if(active!=null)active.activeSeconds+=delta;}
        public static void Move(int x,int y,int toX,int toY,bool valid)
        {if(active!=null)inputs.Add($"{x},{y},{toX},{toY},{(valid?1:0)}");}
        public static void Item(ItemType type,int x=-1,int y=-1)
        { if(active!=null)inputs.Add($"item,{type},{x},{y}"); }
        public static void Finish(LevelProgress progress,string outcome)
        {
            if(active==null || progress==null)return;
            active.outcome=outcome;active.score=progress.Score;active.movesRemaining=progress.MovesRemaining;
            if(progress.IsBlocked)active.reason="noMoves";
            active.moves=progress.MovesUsed;active.bonusMoves=progress.BonusMoves;active.frostCleared=progress.FrostCleared;active.inputs=inputs.ToArray();
            Write(active);last=active;active=null;
        }
        public static bool Feedback(string reason)
        {
            if(last==null)return false;
            var feedback=JsonUtility.FromJson<PlaytestRecord>(JsonUtility.ToJson(last));feedback.reason=reason;feedback.outcome="feedback";return Write(feedback);
        }
        private static bool Write(PlaytestRecord attempt)
        {
            string path=PathForJournal;if(path==null)return true;
            try {Directory.CreateDirectory(Path.GetDirectoryName(path));File.AppendAllText(path,JsonUtility.ToJson(attempt)+Environment.NewLine);return true;}
            catch(Exception e)when(e is IOException || e is UnauthorizedAccessException){Debug.LogWarning("Playtest record could not be saved: "+e.Message);return false;}
        }
    }
}
