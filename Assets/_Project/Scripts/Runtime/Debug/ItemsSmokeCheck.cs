#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.Diagnostics
{
    // Opt-in native checks always use isolated progress and preference files.
    public sealed partial class ItemsSmokeCheck : MonoBehaviour
    {
        private bool failed;
        private Mouse mouse;
        private Keyboard keyboard;
        private BoardController board;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {if(Has("-puzzleItemsSmokeTest") || Has("-puzzleItemsReloadTest") || Has("-puzzleFrozenSmokeTest") || Has("-puzzleUpgradeSmokeTest"))new GameObject("ItemsSmokeCheck").AddComponent<ItemsSmokeCheck>();}
        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);Application.runInBackground=true;
            if(!Check(()=>Require(Arg("-puzzleProgressPath")!=null && Arg("-puzzleSettingsPath")!=null,"Isolated paths required.")))yield break;
            yield return WaitScene("WorldMap");if(failed)yield break;
            var state=CampaignState.Instance;
            if(Has("-puzzleItemsReloadTest"))
            {
                if(!Check(()=>{Require(state.Progress.items.SequenceEqual(new[]{3,3,3,3}),"Inventory failed independent reload.");Require(state.Progress.itemRewardsClaimed[4] && !state.Progress.itemRewardsClaimed[5],"Reward flags failed reload.");}))yield break;
                Finish(0,"PASS: inventory and first-clear reward flags survived independent launch; unfinished item was refunded on normal quit.");yield break;
            }
            mouse=InputSystem.AddDevice<Mouse>();keyboard=InputSystem.AddDevice<Keyboard>();
            if(Has("-puzzleUpgradeSmokeTest")){yield return RunUpgrades();yield break;}
            if(Has("-puzzleFrozenSmokeTest")){yield return RunFrozenCheck();yield break;}
            for(int n=1;n<5;n++)state.Progress.RecordWin(n,2000);
            FindFirstObjectByType<WorldMapView>().Enter(5);yield return WaitScene("Game");if(failed)yield break;
            board=FindFirstObjectByType<BoardController>();var s=board.Session;var ui=FindFirstObjectByType<ItemToolbar>();
            var dialogs=ui.GetComponent<PlayerDialogs>();
            yield return Capture("ItemsKo.png");
            yield return Click((RectTransform)ui.transform.Find("ScoreCard/ItemBar/Hammer"));
            if(!Check(()=>Require(dialogs.IsVisible && board.IsPaused && s.ItemCount(ItemType.Hammer)==3,"Item dialog/pause/stock invalid.")))yield break;
            yield return Capture("HammerHelpKo.png");
            yield return KeyPress(Key.Escape);
            if(!Check(()=>Require(!dialogs.IsVisible && !board.IsPaused && s.ItemCount(ItemType.Hammer)==3,"Cancel spent an item.")))yield break;
            yield return Click((RectTransform)ui.transform.Find("ScoreCard/ItemBar/Hammer"));
            yield return Click((RectTransform)dialogs.transform.Find("PlayerDialog/Card/Footer/itemChoose"));
            yield return KeyPress(Key.Escape);
            if(!Check(()=>Require(!board.SelectedItem.HasValue && !ui.GetComponent<SettingsPopup>().IsVisible,"Esc targeting opened settings.")))yield break;
            yield return Click((RectTransform)ui.transform.Find("ScoreCard/ItemBar/Hammer"));
            yield return Click((RectTransform)dialogs.transform.Find("PlayerDialog/Card/Footer/itemChoose"));
            var target=new GridPosition(0,0);
            for(int y=0;y<8;y++)for(int x=0;x<8;x++)if(board.Model.GetCell(new GridPosition(x,y)).FrostHealth>0)target=new GridPosition(x,y);
            int moves=s.Progress.MovesRemaining;
            int ice=board.Model.GetCell(target).FrostHealth,candy=board.Model.GetPiece(target).Id;
            yield return ClickCell(target);yield return Settle();if(failed)yield break;
            if(!Check(()=>Require(s.ItemCount(ItemType.Hammer)==2 && s.Progress.MovesRemaining==moves && s.Progress.Score==0 && board.Model.GetCell(target).FrostHealth==ice-1 && board.Model.GetPiece(target).Id==candy,"Hammer did not consume exactly one / thaw the same candy without move or candy score.")))yield break;
            yield return Click((RectTransform)ui.transform.Find("ScoreCard/ItemBar/Bomb"));
            yield return Click((RectTransform)dialogs.transform.Find("PlayerDialog/Card/Footer/itemChoose"));
            yield return MouseAt(CellScreen(new GridPosition(3,3)),false);
            if(!Check(()=>Require(FindObjectsByType<PieceView>(FindObjectsSortMode.None).Count(p=>p.IsSelected)==9,"Bomb hover did not preview exactly nine cells.")))yield break;
            yield return Capture("BombTargetKo.png");
            yield return ClickCell(new GridPosition(3,3));yield return Settle();if(failed)yield break;
            foreach(var type in new[]{ItemType.Shuffle,ItemType.ExtraMoves})
            {
                yield return Click((RectTransform)ui.transform.Find("ScoreCard/ItemBar/"+type));
                yield return Click((RectTransform)dialogs.transform.Find("PlayerDialog/Card/Footer/itemUse"));
                yield return Settle();if(failed)yield break;
            }
            if(!Check(()=>{Require(s.Campaign.items.SequenceEqual(new[]{2,2,2,2}),"Four-item stock mismatch.");Require(s.Progress.MovesRemaining==moves+5 && s.Progress.MovesUsed==0,"Bonus moves / move journal count invalid.");Require(MatchFinder.FindMatches(board.Model).Count==0 && MoveFinder.HasAnyMove(board.Model),"Item left unstable board.");}))yield break;
            s.RestartLevel();yield return null;
            yield return Replay(5);if(failed)yield break;yield return new WaitForSecondsRealtime(.8f);
            if(!Check(()=>Require(s.Campaign.items.SequenceEqual(new[]{3,3,3,3}) && s.LastItemRewards.Sum()==4,"Milestone reward not granted.")))yield break;
            yield return Capture("ItemRewardKo.png");
            GamePreferences.Current.language="en";GamePreferences.Save();yield return Capture("ItemRewardEn.png");
            s.RestartLevel();yield return Replay(5);if(failed)yield break;
            if(!Check(()=>Require(s.Campaign.items.SequenceEqual(new[]{3,3,3,3}) && s.LastItemRewards.Sum()==0,"Replay duplicated reward.")))yield break;
            s.NextLevel();yield return null;yield return KeyPress(Key.P);
            if(!Check(()=>Require(s.Progress.Outcome==LevelOutcome.Won && !s.Campaign.itemRewardsClaimed[5] && s.LastItemRewards.Sum()==0,"P awarded items.")))yield break;
            for(int n=7;n<50;n++)s.Campaign.RecordWin(n,2000);
            s.SelectLevel(50);yield return null;
            yield return Capture("ItemsEnFinal.png");yield return Capture("ItemsEnPortrait.png",800,1000);yield return Capture("ItemsEnWide.png",1600,900);
            ui.ShowDetails(ItemType.ExtraMoves);yield return Capture("MovesHelpEn.png");yield return Capture("MovesHelpPortrait.png",800,1000);dialogs.Close();
            s.Campaign.items[0]=0;GamePreferences.Save();ui.ShowDetails(ItemType.Hammer);yield return null;
            if(!Check(()=>Require(!dialogs.transform.Find("PlayerDialog/Card/Footer/itemEmpty").GetComponent<Button>().interactable,"Empty stock is usable.")))yield break;
            yield return Capture("EmptyItemEn.png");dialogs.Close();s.Campaign.items[0]=3;state.Save();
            if(!Check(()=>Require(CampaignProgress.Load(CampaignProgress.SavePath).items.SequenceEqual(new[]{3,3,3,3}),"Stock was not persisted.")))yield break;
            // Quit during an actual item animation. A separate launch verifies the refund on disk.
            if(!Check(()=>Require(board.TryUseItem(ItemType.Bomb,new GridPosition(3,3)) && board.IsBusy,"Could not begin quit/refund check.")))yield break;
            Finish(0,"PASS: pointer-driven four item dialogs/use; free targeting cancel and Esc; effects, ice, no move cost, +5; persistent stock across restart; stage 5 normal clear reward and replay deduplication; P excludes rewards; empty inventory blocked; Korean/English wide/portrait captures.");
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
        private IEnumerator KeyPress(Key key)
        {InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;yield return null;}
        private Vector2 CellScreen(GridPosition p){var view=board.GetComponent<BoardView>();return view.BoardCamera.WorldToScreenPoint(view.transform.TransformPoint(view.CellToLocal(p)));}
        private IEnumerator ClickCell(GridPosition p){yield return MouseAt(CellScreen(p),false);yield return MouseAt(CellScreen(p),true);yield return MouseAt(CellScreen(p),false);}
        private IEnumerator Click(RectTransform rect)
        {Canvas.ForceUpdateCanvases();Vector2 point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));yield return MouseAt(point,false);yield return MouseAt(point,true);yield return MouseAt(point,false);}
        private IEnumerator MouseAt(Vector2 position,bool pressed)
        {InputSystem.QueueStateEvent(mouse,new MouseState{position=position}.WithButton(MouseButton.Left,pressed));yield return null;yield return null;}
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
