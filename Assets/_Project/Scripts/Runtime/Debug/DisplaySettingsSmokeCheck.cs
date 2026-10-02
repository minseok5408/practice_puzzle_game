#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Text;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace PuzzleGame.Runtime.Diagnostics
{
    // Native display checks use the real player window, not an offscreen capture size.
    public sealed class DisplaySettingsSmokeCheck : MonoBehaviour
    {
        private readonly StringBuilder trace=new StringBuilder();
        private bool failed,monitoring;
        private Vector2Int expectedSize;
        private FullScreenMode expectedMode;
        private int observedFrames,transitions;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if(Has("-puzzleDisplaySmokeTest") || Has("-puzzleDisplayReloadTest"))
                new GameObject("DisplaySettingsSmokeCheck").AddComponent<DisplaySettingsSmokeCheck>();
        }

        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);Application.runInBackground=true;
            if(!Check(()=>Require(!string.IsNullOrEmpty(Arg("-puzzleSettingsPath")) && !string.IsNullOrEmpty(Arg("-puzzleProgressPath")),"Use isolated settings/progress paths.")))yield break;
            yield return WaitScene("WorldMap");if(failed)yield break;
            var saved=DisplaySettings.Load(DisplaySettings.SavePath);
            if(!Check(()=>Require(saved!=null,"Seed settings before running this check.")))yield break;
            yield return WaitDisplay(saved.width,saved.height,saved.windowed);if(failed)yield break;
            Watch();trace.AppendLine("Restored on launch: "+Describe());
            yield return Travel(2);if(failed)yield break;
            if(Has("-puzzleDisplayReloadTest"))
            {Finish("PASS: saved display restored on independent launch and preserved through map/game/map.");yield break;}

            foreach(var request in new[]{new Vector3Int(1280,720,1),new Vector3Int(960,600,1),new Vector3Int(1280,800,0),new Vector3Int(960,600,1)})
            {
                monitoring=false;
                var popup=FindFirstObjectByType<SettingsPopup>();popup.Open();
                int choice=-1;
                for(int i=0;i<popup.AvailableResolutions.Count;i++)if(popup.AvailableResolutions[i]==new Vector2Int(request.x,request.y))choice=i;
                if(!Check(()=>Require(choice>=0,"Missing test resolution.")))yield break;
                popup.ResolutionDropdown.value=choice;popup.WindowedToggle.isOn=request.z==1;popup.Apply();
                float until=Time.realtimeSinceStartup+8;
                while(popup.IsApplying && Time.realtimeSinceStartup<until)yield return null;
                if(!Check(()=>Require(!popup.IsApplying,"Display apply timed out.")))yield break;
                yield return WaitDisplay(request.x,request.y,request.z==1);if(failed)yield break;
                if(!Check(()=>{
                    var disk=DisplaySettings.Load(DisplaySettings.SavePath);
                    Require(disk.width==request.x && disk.height==request.y && disk.windowed==(request.z==1),"Saved a stale display size.");
                }))yield break;
                popup.Close();Watch();trace.AppendLine("Applied in "+SceneManager.GetActiveScene().name+": "+Describe());
                // Odd count alternates the scene in which the next setting is applied.
                yield return Travel(3);if(failed)yield break;
                GamePreferences.Current.language=GamePreferences.Current.language=="ko"?"en":"ko";GamePreferences.Save();
                yield return Observe(.15f);if(failed)yield break;
            }

            // A resizable window is allowed to differ from the last saved preference.
            // Loading a scene must never issue another native resolution request.
            monitoring=false;Screen.SetResolution(1024,768,FullScreenMode.Windowed);
            yield return WaitDisplay(1024,768,true);if(failed)yield break;
            Watch();trace.AppendLine("Manual window resize: "+Describe());
            yield return Travel(4);if(failed)yield break;
            if(!Check(()=>{
                var disk=DisplaySettings.Load(DisplaySettings.SavePath);
                Require(disk.width==960 && disk.height==600 && disk.windowed,"Scene navigation overwrote the saved display preference.");
            }))yield break;
            yield return CheckEasterEgg();if(failed)yield break;
            Finish("PASS: native window/fullscreen settings and saved values preserved on scene changes; manual resize remains stable; language changes do not resize the window; P key clears, saves and unlocks the next stage.");
        }

        private IEnumerator CheckEasterEgg()
        {
            yield return Travel(1);if(failed)yield break;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                var board=FindFirstObjectByType<BoardController>();var popup=FindFirstObjectByType<SettingsPopup>();
                popup.Open();yield return PressP(keyboard);
                if(!Check(()=>Require(!board.Session.Progress.IsFinished,"P cleared while settings were open.")))yield break;
                popup.Close();yield return PressP(keyboard);
                if(!Check(()=>{
                    Require(board.Session.Progress.Outcome==LevelOutcome.Won && board.Session.Progress.GoalsMet,"P did not clear the current stage.");
                    var disk=CampaignProgress.Load(CampaignProgress.SavePath);
                    Require(disk.UnlockedThrough>=2 && disk.bestScores[0]>=board.Session.Progress.Rules.TargetScore,"P did not save/unlock.");
                }))yield break;
                FindFirstObjectByType<ResultPopup>().NextButton.onClick.Invoke();yield return null;
                if(!Check(()=>Require(board.Session.Definition.Number==2 && !board.Session.Progress.IsFinished,"Next stage after P did not start normally.")))yield break;
                trace.AppendLine("P key: paused ignored; stage 1 cleared and saved; stage 2 ready.");
            }
            finally{InputSystem.RemoveDevice(keyboard);}
            yield return Travel(1);
        }

        private static IEnumerator PressP(Keyboard keyboard)
        {
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.P));yield return null;yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;yield return null;
        }

        private IEnumerator Travel(int count)
        {
            string before=File.ReadAllText(DisplaySettings.SavePath);
            for(int i=0;i<count;i++)
            {
                string destination;
                if(SceneManager.GetActiveScene().name=="WorldMap")
                {destination="Game";FindFirstObjectByType<WorldMapView>().Enter(1);}
                else{destination="WorldMap";FindFirstObjectByType<CampaignHUD>().OpenMap();}
                yield return WaitScene(destination);if(failed)yield break;
                yield return Observe(.35f);if(failed)yield break;
                if(!Check(()=>Require(File.ReadAllText(DisplaySettings.SavePath)==before,"Scene changed saved settings.")))yield break;
                transitions++;trace.AppendLine("  "+destination+": "+Describe());
            }
        }

        private IEnumerator WaitScene(string scene)
        {
            float until=Time.realtimeSinceStartup+20;
            while(SceneManager.GetActiveScene().name!=scene && !failed && Time.realtimeSinceStartup<until)yield return null;
            yield return null;yield return null;
            Check(()=>Require(SceneManager.GetActiveScene().name==scene,"Scene timeout: "+scene));
        }

        private IEnumerator WaitDisplay(int width,int height,bool window)
        {
            var mode=window?FullScreenMode.Windowed:FullScreenMode.FullScreenWindow;
            float until=Time.realtimeSinceStartup+5;
            while((Screen.width!=width || Screen.height!=height || Screen.fullScreenMode!=mode) && Time.realtimeSinceStartup<until)yield return null;
            Check(()=>Require(Screen.width==width && Screen.height==height && Screen.fullScreenMode==mode,
                $"Requested {width}x{height} {mode}; actual {Describe()}."));
        }

        private IEnumerator Observe(float seconds)
        {float until=Time.realtimeSinceStartup+seconds;while(!failed && Time.realtimeSinceStartup<until)yield return null;}
        private void Watch(){expectedSize=new Vector2Int(Screen.width,Screen.height);expectedMode=Screen.fullScreenMode;monitoring=true;}
        private void Update()
        {
            if(!monitoring || failed)return;
            observedFrames++;
            Check(()=>Require(Screen.width==expectedSize.x && Screen.height==expectedSize.y && Screen.fullScreenMode==expectedMode,
                $"Display changed during scene navigation: expected {expectedSize} {expectedMode}, actual {Describe()}."));
        }
        private static string Describe()=>$"{Screen.width}x{Screen.height} {Screen.fullScreenMode}";
        private bool Check(Action action){if(failed)return false;try{action();return true;}catch(Exception e){failed=true;Finish("FAIL: "+e);return false;}}
        private static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        private static bool Has(string name)=>Array.IndexOf(Environment.GetCommandLineArgs(),name)>=0;
        private static string Arg(string name){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,name);return i>=0 && i+1<args.Length?args[i+1]:null;}
        private void Finish(string report)
        {
            monitoring=false;report+=$"\nTransitions: {transitions}; frames observed: {observedFrames}\n"+trace;
            string path=Arg("-puzzleSmokeReport");if(!string.IsNullOrEmpty(path))File.WriteAllText(path,report);
            Debug.Log(report);Application.Quit(failed?1:0);
        }
    }
}
#endif
