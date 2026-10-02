#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.Diagnostics
{
    public sealed class ExpansionSmokeCheck : MonoBehaviour
    {
        private bool failed;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        { if(Array.IndexOf(Environment.GetCommandLineArgs(),"-puzzleExpansionSmokeTest")>=0)new GameObject("ExpansionSmokeCheck").AddComponent<ExpansionSmokeCheck>(); }
        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);Application.runInBackground=true;
            if(!Check(()=>Require(!string.IsNullOrEmpty(Arg("-puzzleProgressPath")) && !string.IsNullOrEmpty(Arg("-puzzleSettingsPath")),"Use isolated save paths.")))yield break;
            float began=Time.realtimeSinceStartup;
            yield return WaitScene("WorldMap");if(failed)yield break;
            float startup=Time.realtimeSinceStartup-began;
            var map=FindFirstObjectByType<WorldMapView>();var dialogs=map.GetComponent<PlayerDialogs>();
            var settings=map.GetComponent<SettingsPopup>();
            settings.Open();dialogs.Preferences();yield return Capture("PreferencesKo.png");
            const string body="PlayerDialog/Card/Viewport/Content/";
            var music=dialogs.transform.Find(body+"music/Volume").GetComponent<Slider>();
            var mute=dialogs.transform.Find(body+"music/Mute").GetComponent<Toggle>();
            music.value=20;dialogs.transform.Find(body+"effects/Volume").GetComponent<Slider>().value=40;
            mute.isOn=true;yield return Capture("PreferencesMuted.png");
            if(!Check(()=>Require(GamePreferences.Current.musicMuted && GamePreferences.Current.musicVolume==.2f && !music.interactable,"Mute lost saved volume.")))yield break;
            mute.isOn=false;
            dialogs.transform.Find(body+"Tabs/experienceTab").GetComponent<Button>().onClick.Invoke();yield return null;
            yield return Capture("LanguageKo.png");
            dialogs.transform.Find(body+"Language/Choices/en").GetComponent<Button>().onClick.Invoke();yield return null;
            dialogs.transform.Find(body+"ReducedEffects/Switch").GetComponent<Toggle>().isOn=true;
            yield return Capture("LanguageEn.png");yield return Capture("LanguagePortrait.png",800,1000);
            dialogs.transform.Find(body+"Tabs/otherTab").GetComponent<Button>().onClick.Invoke();yield return null;
            yield return Capture("MoreEn.png");
            dialogs.transform.Find(body+"credits").GetComponent<Button>().onClick.Invoke();yield return Capture("CreditsEn.png");
            dialogs.transform.Find("PlayerDialog/Card/Footer/back").GetComponent<Button>().onClick.Invoke();yield return null;
            if(!Check(()=>Require(dialogs.transform.Find(body+"reset") && Time.timeScale==0,"Credits did not return to settings.")))yield break;
            dialogs.Preferences();
            yield return Capture("PreferencesEn.png");yield return Capture("PreferencesPortrait.png",800,1000);
            if(!Check(()=>{
                var loaded=DisplaySettings.Load(DisplaySettings.SavePath);
                Require(loaded.language=="en" && loaded.musicVolume==.2f && loaded.effectsVolume==.4f && loaded.reducedEffects,"Preferences did not persist.");
                Require(settings.OpenButton.GetComponentInChildren<TMPro.TMP_Text>().text=="Settings","Scene labels did not translate.");
            }))yield break;
            dialogs.Close();if(!Check(()=>Require(Time.timeScale==0,"Nested popup lost parent pause.")))yield break;settings.Close();
            yield return Capture("MapEn.png");
            dialogs.Message("credits",Localization.Get("creditsBody"));yield return Capture("CreditsEn.png");dialogs.Close();
            map.StageButtons[0].onClick.Invoke();yield return Capture("PreviewEn.png");dialogs.Close();
            dialogs.Preview(CampaignState.Instance.Catalog.Get(50),CampaignState.Instance.Progress,()=>{});
            yield return Capture("PreviewGoalsEn.png");yield return Capture("PreviewGoalsPortrait.png",800,1000);yield return Capture("PreviewGoalsWide.png",1600,900);dialogs.Close();
            GamePreferences.Current.language="ko";GamePreferences.Current.reducedEffects=false;GamePreferences.Save();
            var campaign=CampaignState.Instance;
            dialogs.Preview(campaign.Catalog.Get(5),campaign.Progress,()=>{});yield return Capture("PreviewGoalsKo.png");dialogs.Close();
            for(int i=1;i<5;i++)campaign.Progress.RecordWin(i,2000);
            map.Enter(5);yield return WaitScene("Game");if(failed)yield break;
            var board=FindFirstObjectByType<BoardController>();
            yield return Capture("FrostTutorial.png");yield return Capture("FrostWide.png",1600,900);yield return Capture("FrostPortrait.png",800,1000);
            var hint=board.GetComponent<BoardGuidance>();hint.SendMessage("OnApplicationFocus",true);hint.HintDelay=.15f;
            yield return new WaitForSeconds(.3f);
            if(!Check(()=>Require(hint.HintVisible,"Idle hint was not shown.")))yield break;
            yield return Capture("FrostHint.png");
            yield return Replay(5);if(failed)yield break;
            if(!Check(()=>Require(board.Session.Progress.FrostCleared>=board.Session.Progress.Rules.FrostTarget,"Frost goal was not completed.")))yield break;
            if(!Check(()=>{var goals=FindFirstObjectByType<GoalPanel>();Require(goals && goals.CompletedGoalCount==goals.GoalCount,"Completed goals were not marked.");}))yield break;
            yield return new WaitForSecondsRealtime(.18f);yield return Capture("FrostCelebration.png");
            yield return new WaitForSecondsRealtime(1.1f);yield return Capture("FrostWon.png");
            var resultOverlay=FindFirstObjectByType<ResultPopup>().transform.Find("ResultOverlay").gameObject;
            resultOverlay.SetActive(false);yield return Capture("GoalsComplete.png");resultOverlay.SetActive(true);
            for(int i=6;i<50;i++)campaign.Progress.RecordWin(i,2000);
            board.Session.SelectLevel(50);GamePreferences.Current.language="en";GamePreferences.Current.reducedEffects=true;GamePreferences.Save();
            yield return Capture("FinalStageEn.png");
            yield return Capture("GoalsPortraitEn.png",800,1000);
            yield return Replay(50);if(failed)yield break;
            yield return new WaitForSeconds(.6f);yield return Capture("CampaignCompleteEn.png");
            yield return Capture("CampaignCompletePortrait.png",800,1000);
            if(!Check(()=>{
                Require(campaign.Progress.CompletedCount==50,"Campaign was not complete.");
                Require(!FindFirstObjectByType<ResultPopup>().NextButton.gameObject.activeSelf,"Final next button was visible.");
                Require(File.Exists(Path.Combine(Path.GetDirectoryName(CampaignProgress.SavePath),"playtest.jsonl")),"Playtest journal missing.");
            }))yield break;
            FindFirstObjectByType<CampaignHUD>().OpenMap();yield return WaitScene("WorldMap");if(failed)yield break;
            dialogs=FindFirstObjectByType<PlayerDialogs>();dialogs.Confirm("reset",Localization.Get("resetConfirm"),()=>campaign.ResetProgress());
            yield return Capture("ResetConfirmEn.png");dialogs.Close();
            if(!Check(()=>Require(campaign.Progress.CompletedCount==50,"Cancel reset changed progress.")))yield break;
            if(!Check(()=>{
                Require(campaign.ResetProgress(),"Reset failed.");
                Require(CampaignProgress.Load(CampaignProgress.SavePath).CompletedCount==0,"Primary was not reset.");
                Require(CampaignProgress.Load(CampaignProgress.SavePath+".bak").CompletedCount==0,"Backup was not reset.");
            }))yield break;
            Finish(0,$"PASS: local startup {startup:F2}s; persisted audio/language/effects; nested pause; previews/credits; Frost and hints; stages 5/50 actual winning routes; bilingual completion; telemetry; confirmed reset and backup. Captures at 16:10,16:9,portrait.");
        }
        private IEnumerator Replay(int stage)
        {
            string route=null;
            if(!Check(()=>{foreach(string line in File.ReadAllLines(Arg("-puzzleWinningRoutes")))if(line.StartsWith(stage+"|"))route=line.Substring(line.IndexOf('|')+1);Require(route!=null,"Missing route "+stage);} ))yield break;
            var board=FindFirstObjectByType<BoardController>();
            foreach(string move in route.Split(';'))
            {
                var v=Array.ConvertAll(move.Split(','),int.Parse);
                if(!Check(()=>Require(board.TrySwap(new GridPosition(v[0],v[1]),new GridPosition(v[2],v[3])),"Move rejected.")))yield break;
                float until=Time.realtimeSinceStartup+30;while(board.IsBusy && Time.realtimeSinceStartup<until)yield return null;
                if(!Check(()=>Require(!board.IsBusy,"Cascade timed out.")))yield break;
            }
            Check(()=>Require(board.Session.Progress.Outcome==LevelOutcome.Won,"Route did not win "+stage));
        }
        private IEnumerator WaitScene(string name)
        {
            float until=Time.realtimeSinceStartup+30;while(SceneManager.GetActiveScene().name!=name && Time.realtimeSinceStartup<until)yield return null;
            yield return null;yield return null;Check(()=>Require(SceneManager.GetActiveScene().name==name,"Scene timeout: "+name));
        }
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
        private static string Arg(string name){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,name);return i>=0 && i+1<args.Length?args[i+1]:null;}
        private static void Finish(int code,string report){string path=Arg("-puzzleSmokeReport");if(!string.IsNullOrEmpty(path))File.WriteAllText(path,report);Debug.Log(report);Application.Quit(code);}
    }
}
#endif
