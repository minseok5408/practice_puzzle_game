#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.Diagnostics
{
    // Development-only, opt-in soak test. Never touches the normal player's save files.
    public sealed class QualitySmokeCheck : MonoBehaviour
    {
        private bool failed,measureFrames;
        private BoardController board;
        private readonly List<float> frameTimes=new List<float>();
        private int width,height;private FullScreenMode windowMode;
        private long baselineManaged,baselineUnity;
        private string output;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {if(Has("-puzzleQualitySmokeTest") || Has("-puzzleQualityReloadTest"))new GameObject("QualitySmokeCheck").AddComponent<QualitySmokeCheck>();}
        private void Update(){if(measureFrames && frameTimes.Count<200000)frameTimes.Add(Time.unscaledDeltaTime*1000);}
        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);Application.runInBackground=true;
            if(!Check(()=>Require(Arg("-puzzleProgressPath")!=null && Arg("-puzzleSettingsPath")!=null,"Isolated save paths required.")))yield break;
            output=Path.GetDirectoryName(Arg("-puzzleSmokeReport"));Directory.CreateDirectory(output);
            yield return WaitScene("WorldMap");if(failed)yield break;
            var state=CampaignState.Instance;
            if(Has("-puzzleQualityReloadTest"))
            {
                if(!Check(()=>{
                    Require(GamePreferences.Current.colorLabels && GamePreferences.Current.language=="en","Preferences did not survive process restart.");
                    Require(state.Progress.CompletedCount==50 && state.Progress.items.All(n=>n==7),"Progress or item inventory failed independent reload.");
                    Require(Screen.width==GamePreferences.Current.width && Screen.height==GamePreferences.Current.height,"Saved resolution did not restore.");
                }))yield break;
                Finish(0,"PASS: independent process restored color labels, English, resolution, 50 stages and four item inventories.");yield break;
            }
            if(!Check(VerifyMigrationAndRecovery))yield break;
            for(int n=1;n<=50;n++)state.Progress.RecordWin(n,2000);state.Save();
            FindFirstObjectByType<WorldMapView>().Enter(46);yield return WaitScene("Game");if(failed)yield break;
            board=FindFirstObjectByType<BoardController>();
            if(!Check(()=>Require(board.Session.Progress.MovesRemaining==31,"5-6 move tuning missing.")))yield break;
            yield return Replay(46);if(failed)yield break;
            board.Session.SelectLevel(49);yield return null;
            if(!Check(()=>Require(board.Session.Progress.MovesRemaining==31,"5-9 move tuning missing.")))yield break;
            yield return Replay(49);if(failed)yield break;
            board.Session.RestartLevel();yield return null;
            var dialogs=FindFirstObjectByType<PlayerDialogs>();
            foreach(string language in new[]{"ko","en"})
            {
                GamePreferences.Current.language=language;GamePreferences.Current.colorLabels=true;GamePreferences.Save();
                yield return Capture("Numbers-"+language+"-small.png",960,600);
                yield return Capture("Numbers-"+language+"-portrait.png",800,1000);
                for(int page=0;page<4;page++)
                {
                    dialogs.Guide(page);yield return null;yield return Capture("Guide-"+language+"-"+page+".png",960,600);
                    if(page==3){dialogs.transform.Find("PlayerDialog/Card").GetComponent<ScrollRect>().verticalNormalizedPosition=0;yield return Capture("Guide-"+language+"-items-bottom.png",800,1000);}
                }
                dialogs.Close();dialogs.Preferences();
                dialogs.transform.Find("PlayerDialog/Card/Viewport/Content/Tabs/experienceTab").GetComponent<Button>().onClick.Invoke();
                yield return Capture("Accessibility-"+language+".png",960,600);dialogs.Close();
            }
            if(!Check(ExportAudio))yield break;
            width=Screen.width;height=Screen.height;windowMode=Screen.fullScreenMode;
            File.WriteAllText(Path.Combine(output,"stability.csv"),"cycle,seconds,managed_mb,unity_mb,objects,frame_p95_ms,frame_max_ms\n");
            float began=Time.realtimeSinceStartup;
            int cycles=int.TryParse(Arg("-puzzleStressCycles"),out int c)?Mathf.Clamp(c,20,2000):120;
            float minimumSeconds=float.TryParse(Arg("-puzzleStressSeconds"),NumberStyles.Float,CultureInfo.InvariantCulture,out float s)?Mathf.Clamp(s,0,7200):600;
            int cycle=0;
            while(cycle<cycles || Time.realtimeSinceStartup-began<minimumSeconds)
            {
                cycle++;measureFrames=true;GamePreferences.Current.colorLabels=cycle%2==0;GamePreferences.Current.reducedEffects=cycle%3==0;GamePreferences.Save();
                for(int i=0;i<4;i++)state.Progress.items[i]=7;state.Save();
                if(!Check(()=>{
                    Require(FindObjectsByType<BoardController>(FindObjectsSortMode.None).Length==1,"Duplicate board.");
                    Require(FindObjectsByType<GameAudio>(FindObjectsSortMode.None).Length==1,"Duplicate audio manager.");
                    Require(FindObjectsByType<CampaignState>(FindObjectsSortMode.None).Length==1,"Duplicate campaign state.");
                    Require(FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"Duplicate event system.");
                    Require(Screen.width==width && Screen.height==height && Screen.fullScreenMode==windowMode,"Scene transition changed resolution.");
                    Require(MoveFinder.TryFindMove(board.Model,out var a,out var b) && board.TrySwap(a,b),"Normal swap rejected.");
                }))yield break;
                yield return Settle();if(failed)yield break;
                board.Session.RestartLevel();yield return null;
                var type=(ItemType)(cycle%4);
                if(!Check(()=>Require(board.TryUseItem(type,new GridPosition(3,3)),"Item failed in soak.")))yield break;
                yield return Settle();if(failed)yield break;
                board.Session.RestartLevel();yield return null;
                if(!Check(()=>Require(state.Progress.items[(int)type]==6,"Completed item was not persisted across restart.")))yield break;
                if(cycle%10==0)
                {
                    var ui=FindFirstObjectByType<PlayerDialogs>();ui.Guide(cycle/10%4);yield return null;ui.Close();
                    measureFrames=false;yield return Resources.UnloadUnusedAssets();GC.Collect();yield return null;
                    if(!Check(()=>Sample(cycle,Time.realtimeSinceStartup-began)))yield break;
                }
                measureFrames=false;
                FindFirstObjectByType<CampaignHUD>().OpenMap();yield return WaitScene("WorldMap");if(failed)yield break;
                if(!Check(()=>Require(Screen.width==width && Screen.height==height && Screen.fullScreenMode==windowMode,"Map changed resolution.")))yield break;
                FindFirstObjectByType<WorldMapView>().Enter(cycle%2==0?46:49);yield return WaitScene("Game");if(failed)yield break;
                board=FindFirstObjectByType<BoardController>();
            }
            for(int i=0;i<4;i++)state.Progress.items[i]=7;state.Save();
            GamePreferences.Current.colorLabels=true;GamePreferences.Current.language="en";GamePreferences.Save();
            if(!Check(()=>{
                var data=PlaytestAnalysis.Read(File.ReadLines(PlaytestJournal.PathForJournal));data.WriteCsv(Path.Combine(output,"stages.csv"));
                Require(data.Rows.Count>0 && data.Rows.All(r=>r.Source=="automated"),"Automated attempts contaminated human cohort.");
                Require(data.Rows.Sum(r=>r.Wins)>=2,"Winning route telemetry missing.");
            }))yield break;
            Finish(0,$"PASS: {cycle} cycles / {Time.realtimeSinceStartup-began:F1}s; {cycle*2} scene transitions, {cycle*2} restarts, {cycle} normal swaps and {cycle} item uses. Resolution/singletons/inventory stable. Stages 46 and 49 won by normal routes. Bilingual guide and accessibility captures; v1/v2 migration and corrupt-primary recovery; source-separated CSV. See stability.csv and audio.txt for measurements.");
        }
        private void Sample(int cycle,float seconds)
        {
            long managed=GC.GetTotalMemory(true),native=Profiler.GetTotalAllocatedMemoryLong();
            if(cycle==10){baselineManaged=managed;baselineUnity=native;}
            Require(managed-baselineManaged<32L*1024*1024,"Managed memory grew beyond 32 MB after warmup.");
            Require(native-baselineUnity<96L*1024*1024,"Unity allocation grew beyond 96 MB after warmup.");
            frameTimes.Sort();float p95=frameTimes.Count>0?frameTimes[(int)((frameTimes.Count-1)*.95f)]:0,max=frameTimes.Count>0?frameTimes[frameTimes.Count-1]:0;
            string row=FormattableString.Invariant($"{cycle},{seconds:F1},{managed/1048576.0:F2},{native/1048576.0:F2},{Resources.FindObjectsOfTypeAll<GameObject>().Length},{p95:F2},{max:F2}");
            File.AppendAllText(Path.Combine(output,"stability.csv"),row+"\n");Debug.Log("QUALITY STABILITY "+row);frameTimes.Clear();
        }
        private void VerifyMigrationAndRecovery()
        {
            foreach(int version in new[]{1,2})
            {
                string path=Path.Combine(output,"migration-v"+version+".json");
                var old=new CampaignProgress{version=version,completed=new bool[version==1?100:50],bestScores=new int[version==1?100:50]};
                old.completed[0]=true;old.bestScores[0]=1234;File.WriteAllText(path,JsonUtility.ToJson(old));
                var migrated=CampaignProgress.Load(path);Require(migrated.version==3 && migrated.IsComplete(1) && migrated.bestScores[0]==1234 && migrated.items.All(n=>n==3),"Migration failed.");
                migrated.items[0]=2;Require(migrated.Save(path),"Migrated save failed.");Require(CampaignProgress.Load(path).items[0]==2,"Migration granted stock twice.");
            }
            string recovery=Path.Combine(output,"recovery.json");var good=new CampaignProgress();good.RecordWin(1,3456);good.items[2]=8;
            Require(good.Save(recovery) && good.Save(recovery),"Recovery fixture failed.");File.WriteAllText(recovery,"{broken");
            var restored=CampaignProgress.Load(recovery,out var status);Require(status==ProgressLoadStatus.Recovered && restored.bestScores[0]==3456 && restored.items[2]==8,"Backup did not restore exact data.");
        }
        private void ExportAudio()
        {
            var audio=GameAudio.Ensure();var notes=new List<string>();
            foreach(string cue in new[]{"music","cascade","special","win","complete"})
            {
                var clip=cue=="music"?audio.MusicClip:audio.EffectClip(cue);var data=new float[clip.samples];clip.GetData(data,0);
                float peak=data.Max(v=>Mathf.Abs(v));Require(peak<=(cue=="music"?.141f:.231f),"Audio peak exceeded mix budget.");
                notes.Add(FormattableString.Invariant($"{cue}: {clip.length:F3}s; peak={peak:F4}; rms={Math.Sqrt(data.Average(v=>(double)v*v)):F4}; seam delta={Math.Abs(data[0]-data[data.Length-1]):F5}"));
                using(var stream=new BinaryWriter(File.Create(Path.Combine(output,cue+".wav"))))
                {
                    stream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));stream.Write(36+data.Length*2);stream.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));stream.Write(16);stream.Write((short)1);stream.Write((short)1);stream.Write(clip.frequency);stream.Write(clip.frequency*2);stream.Write((short)2);stream.Write((short)16);stream.Write(System.Text.Encoding.ASCII.GetBytes("data"));stream.Write(data.Length*2);foreach(float value in data)stream.Write((short)(value*32767));
                }
            }
            File.WriteAllLines(Path.Combine(output,"audio.txt"),notes);
        }
        private IEnumerator Replay(int stage)
        {
            string route=null;
            if(!Check(()=>{foreach(string line in File.ReadAllLines(Arg("-puzzleWinningRoutes")))if(line.StartsWith(stage+"|"))route=line.Substring(line.IndexOf('|')+1);Require(route!=null,"Missing route.");}))yield break;
            foreach(string move in route.Split(';'))
            {
                var v=Array.ConvertAll(move.Split(','),int.Parse);
                if(!Check(()=>Require(board.TrySwap(new GridPosition(v[0],v[1]),new GridPosition(v[2],v[3])),"Move rejected.")))yield break;
                yield return Settle();if(failed)yield break;
            }
            Check(()=>Require(board.Session.Progress.Outcome==LevelOutcome.Won,"Route did not win."));
        }
        private IEnumerator Settle()
        {float until=Time.realtimeSinceStartup+25;while(board.IsBusy && Time.realtimeSinceStartup<until)yield return null;yield return null;Check(()=>Require(!board.IsBusy,"Item timed out."));}
        private IEnumerator WaitScene(string name)
        {float until=Time.realtimeSinceStartup+30;while(SceneManager.GetActiveScene().name!=name && Time.realtimeSinceStartup<until)yield return null;yield return null;yield return null;Check(()=>Require(SceneManager.GetActiveScene().name==name,"Scene timeout."));}
        private IEnumerator Capture(string name,int width=1280,int height=800)
        {
            string directory=Arg("-puzzleCaptureFolder");if(string.IsNullOrEmpty(directory))yield break;
            Directory.CreateDirectory(directory);
            var canvas=FindFirstObjectByType<PlayerDialogs>().GetComponent<Canvas>();var camera=Camera.main;
            var target=new RenderTexture(width,height,24);var picture=new Texture2D(width,height,TextureFormat.RGB24,false);
            var mode=canvas.renderMode;var priorCamera=canvas.worldCamera;float distance=canvas.planeDistance,aspect=camera.aspect;
            var priorTarget=camera.targetTexture;var priorActive=RenderTexture.active;int order=canvas.sortingOrder;
            try
            {
                target.Create();camera.targetTexture=target;camera.aspect=(float)width/height;
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;canvas.sortingOrder=100;
                Canvas.ForceUpdateCanvases();FindFirstObjectByType<CandyLayout>()?.ApplyLayout();FindFirstObjectByType<BoardView>()?.FitCamera();
                foreach(var graphic in canvas.GetComponentsInChildren<Graphic>(true))graphic.SetAllDirty();
                Canvas.ForceUpdateCanvases();yield return null;yield return null;
                Check(()=>{
                    RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
                    RenderTexture.active=target;picture.ReadPixels(new Rect(0,0,width,height),0,0);picture.Apply();File.WriteAllBytes(Path.Combine(directory,name),picture.EncodeToPNG());
                });
            }
            finally
            {
                RenderTexture.active=priorActive;camera.targetTexture=priorTarget;camera.aspect=aspect;
                canvas.renderMode=mode;canvas.worldCamera=priorCamera;canvas.planeDistance=distance;canvas.sortingOrder=order;
                target.Release();Destroy(target);Destroy(picture);Canvas.ForceUpdateCanvases();FindFirstObjectByType<CandyLayout>()?.ApplyLayout();
            }
        }
        private bool Check(Action action){try{action();return true;}catch(Exception error){failed=true;Finish(1,"FAIL: "+error);return false;}}
        private static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        private static bool Has(string name)=>Array.IndexOf(Environment.GetCommandLineArgs(),name)>=0;
        private static string Arg(string name){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,name);return i>=0 && i+1<args.Length?args[i+1]:null;}
        private static void Finish(int code,string report){string path=Arg("-puzzleSmokeReport");if(path!=null)File.WriteAllText(path,report);Debug.Log(report);Application.Quit(code);}
    }
}
#endif
