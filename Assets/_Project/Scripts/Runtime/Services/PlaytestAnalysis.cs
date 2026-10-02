using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PuzzleGame.Core.Levels;
using UnityEngine;

namespace PuzzleGame.Runtime.Services
{
    // Preserve source/version/item-use cohorts. A bot or P clear is never a human win.
    public sealed class PlaytestAnalysis
    {
        public sealed class Row
        {
            public string Level,Version,Source,Cohort;
            public int Wins,Losses,Abandoned,MovesLeft,Items,Luck,FewMoves,Unclear;
            public double Seconds;
            public readonly int[] ItemCounts=new int[4];
            public int Finished=>Wins+Losses;
        }
        public readonly List<Row> Rows=new List<Row>();
        public int InvalidLines,ExcludedEasterEggs,Duplicates,FeedbackLines;
        public static PlaytestAnalysis Read(IEnumerable<string> lines)
        {
            var result=new PlaytestAnalysis();var attempts=new Dictionary<string,PlaytestRecord>();var feedback=new Dictionary<string,string>();
            foreach(string line in lines)
            {
                if(string.IsNullOrWhiteSpace(line))continue;
                if(line.Length>1024*1024){result.InvalidLines++;continue;}
                PlaytestRecord p;
                try{p=JsonUtility.FromJson<PlaytestRecord>(line);}catch(ArgumentException){result.InvalidLines++;continue;}
                if(p==null || !Regex.IsMatch(p.levelId??"","^level_0(0[1-9]|[1-4][0-9]|50)$") ||
                    !Regex.IsMatch(p.rulesVersion??"","^[A-Za-z0-9_.-]{1,64}$") || p.moves<0 || p.movesRemaining<0 || p.score<0 ||
                    p.activeSeconds<0 || float.IsNaN(p.activeSeconds) || float.IsInfinity(p.activeSeconds)) {result.InvalidLines++;continue;}
                string id=p.attemptId;
                if(string.IsNullOrEmpty(id))
                {
                    if(!DateTime.TryParse(p.recordedUtc,CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind,out _)){result.InvalidLines++;continue;}
                    id=p.levelId+"|"+p.recordedUtc+"|"+p.seed;
                }
                if(p.outcome=="feedback")
                {
                    if(p.reason=="luck" || p.reason=="fewMoves" || p.reason=="unclear"){feedback[id]=p.reason;result.FeedbackLines++;}
                    else result.InvalidLines++;
                    continue;
                }
                if(p.outcome=="easter_egg"){result.ExcludedEasterEggs++;continue;}
                if(p.outcome!="won" && p.outcome!="lost" && p.outcome!="restart" && p.outcome!="exit"){result.InvalidLines++;continue;}
                if(attempts.ContainsKey(id)){result.Duplicates++;continue;}
                attempts.Add(id,p);
            }
            var groups=new Dictionary<string,Row>();
            foreach(var pair in attempts)
            {
                var p=pair.Value;string source=p.source=="player" || p.source=="automated" || p.source=="editor"?p.source:"legacy";
                var counts=new int[4];
                foreach(string input in p.inputs??Array.Empty<string>())
                {
                    var parts=(input??"").Split(',');
                    if(parts.Length==4 && parts[0]=="item" && Enum.TryParse(parts[1],out ItemType item) && ItemRules.IsValid(item))counts[(int)item]++;
                }
                string cohort=counts.Sum()>0?"items":"no-items";
                string key=p.levelId+"|"+p.rulesVersion+"|"+source+"|"+cohort;
                if(!groups.TryGetValue(key,out var row))groups.Add(key,row=new Row{Level=p.levelId,Version=p.rulesVersion,Source=source,Cohort=cohort});
                if(p.outcome=="won"){row.Wins++;row.MovesLeft+=p.movesRemaining;}else if(p.outcome=="lost")row.Losses++;else row.Abandoned++;
                row.Seconds+=p.activeSeconds;
                for(int i=0;i<4;i++){row.ItemCounts[i]+=counts[i];row.Items+=counts[i];}
                if(feedback.TryGetValue(pair.Key,out string reason))
                {if(reason=="luck")row.Luck++;else if(reason=="fewMoves")row.FewMoves++;else if(reason=="unclear")row.Unclear++;}
            }
            result.Rows.AddRange(groups.OrderBy(g=>g.Key,StringComparer.Ordinal).Select(g=>g.Value));return result;
        }
        public void WriteCsv(string path)
        {
            var csv=new StringBuilder("level,version,source,cohort,wins,losses,abandoned,finished_win_rate,mean_moves_left_on_win,mean_active_seconds,mean_items_per_attempt,hammer,bomb,shuffle,extra_moves,luck,few_moves,unclear\n");
            string N(double value)=>value.ToString("F3",CultureInfo.InvariantCulture);
            foreach(var r in Rows)
            {
                int count=r.Finished+r.Abandoned;
                csv.AppendLine($"{r.Level},{r.Version},{r.Source},{r.Cohort},{r.Wins},{r.Losses},{r.Abandoned},{(r.Finished==0?"":N((double)r.Wins/r.Finished))},{(r.Wins==0?"":N((double)r.MovesLeft/r.Wins))},{N(r.Seconds/count)},{N((double)r.Items/count)},{string.Join(",",r.ItemCounts)},{r.Luck},{r.FewMoves},{r.Unclear}");
            }
            File.WriteAllText(path,csv.ToString(),new UTF8Encoding(true));
        }
    }
}
