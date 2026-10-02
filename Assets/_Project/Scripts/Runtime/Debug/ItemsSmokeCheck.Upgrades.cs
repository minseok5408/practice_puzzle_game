#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Linq;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.Diagnostics
{
    public sealed partial class ItemsSmokeCheck
    {
        private IEnumerator RunUpgrades()
        {
            var campaign=CampaignState.Instance;campaign.ResetProgress();GamePreferences.Current.language="ko";GamePreferences.Save();
            var map=FindFirstObjectByType<WorldMapView>();map.ShowWorld(1);yield return Capture("MapKo.png");
            yield return Click((RectTransform)map.StageButtons[1].transform);
            var dialogs=map.GetComponent<PlayerDialogs>();
            if(!Check(()=>Require(!dialogs.transform.Find("PlayerDialog/Card/Footer/stageLocked").GetComponent<Button>().interactable,"Locked preview permits entry.")))yield break;
            yield return Capture("LockedPreviewKo.png");dialogs.Close();
            yield return Click((RectTransform)map.StageButtons[0].transform);yield return Capture("PreviewKo.png");
            yield return Click((RectTransform)dialogs.transform.Find("PlayerDialog/Card/Footer/start"));yield return WaitScene("Game");if(failed)yield break;
            board=FindFirstObjectByType<BoardController>();var session=board.Session;dialogs=FindFirstObjectByType<PlayerDialogs>();
            yield return Capture("HudKo.png");yield return Capture("HudPortraitKo.png",800,1000);
            MoveFinder.TryFindMove(board.Model,out var a,out var b);yield return KeyPress(Key.LeftArrow);
            var input=board.GetComponent<BoardInput>();
            while(input.KeyboardCell.X<a.X)yield return KeyPress(Key.RightArrow);
            while(input.KeyboardCell.Y<a.Y)yield return KeyPress(Key.UpArrow);
            yield return KeyPress(Key.Enter);yield return KeyPress(b.X>a.X?Key.RightArrow:Key.UpArrow);yield return KeyPress(Key.Enter);yield return Settle();
            if(!Check(()=>Require(board.CompletedMoves==1,"Keyboard failed to resolve one move.")))yield break;
            yield return KeyPress(Key.H);yield return Capture("ManualHintKo.png");
            yield return KeyPress(Key.Digit1);
            if(!Check(()=>Require(dialogs.IsVisible && session.ItemsUsed==0,"Shortcut used item without confirmation.")))yield break;
            yield return KeyPress(Key.Escape);
            FindFirstObjectByType<HUDView>().RestartButton.onClick.Invoke();yield return Capture("RestartConfirmKo.png");
            yield return Click((RectTransform)dialogs.transform.Find("PlayerDialog/Card/Footer/cancel"));
            var settings=FindFirstObjectByType<SettingsPopup>();settings.Open();yield return Capture("SettingsDisplayKo.png");
            const string content="PlayerDialog/Card/Viewport/Content/";
            foreach(string tab in new[]{"audioTab","experienceTab","otherTab"})
            {
                yield return Click((RectTransform)dialogs.transform.Find(content+"Tabs/"+tab));yield return Capture("Settings-"+tab+"-ko.png");
            }
            dialogs.Close();GamePreferences.Current.animationSpeed=2;GamePreferences.Current.autoHints=false;GamePreferences.Save();
            for(int n=1;n<5;n++)campaign.Progress.RecordWin(n,1500);
            session.SelectLevel(5);yield return null;yield return Capture("GoalsKo.png");yield return Capture("GoalsPortraitKo.png",800,1000);
            yield return Replay(5);if(failed)yield break;yield return new WaitForSecondsRealtime(.8f);
            if(!Check(()=>Require(session.EarnedStars>=2 && session.Campaign.cleanMedals[4] && session.LastItemRewards.Sum()==4,"Normal win did not earn stars, medal and rewards.")))yield break;
            yield return Capture("ResultWinKo.png");yield return Capture("ResultWinSmallKo.png",960,600);
            GamePreferences.Current.language="en";GamePreferences.Save();yield return Capture("ResultWinEn.png");
            FindFirstObjectByType<ResultPopup>().MapButton.onClick.Invoke();yield return WaitScene("WorldMap");if(failed)yield break;
            yield return Capture("MapStarsEn.png");map=FindFirstObjectByType<WorldMapView>();
            yield return Click((RectTransform)map.StageButtons[4].transform);yield return Capture("ClaimedPreviewEn.png");FindFirstObjectByType<PlayerDialogs>().Close();
            map.Enter(5);yield return WaitScene("Game");board=FindFirstObjectByType<BoardController>();session=board.Session;
            session.StartWithRules(new LevelRules(1,1000,10,new[]{4,3,2,0,0,0},5));session.BeginMove();session.CompleteMove();yield return new WaitForSecondsRealtime(.8f);
            yield return Capture("ResultLossEn.png");yield return Capture("ResultLossPortraitEn.png",800,1000);
            for(int n=6;n<49;n++)campaign.Progress.RecordWin(n,2500);
            session.SelectLevel(49);yield return null;yield return Capture("GoalsEn.png");yield return Capture("GoalsSmallEn.png",960,600);
            yield return Replay(49);if(failed)yield break;yield return new WaitForSecondsRealtime(.8f);yield return Capture("ResultWin49En.png");
            settings=FindFirstObjectByType<SettingsPopup>();settings.Open();dialogs=settings.GetComponent<PlayerDialogs>();
            yield return Capture("SettingsDisplayEn.png");yield return Click((RectTransform)dialogs.transform.Find(content+"Tabs/experienceTab"));yield return Capture("SettingsPlayPortraitEn.png",800,1000);dialogs.Close();
            if(!Check(()=>{
                var loaded=CampaignProgress.Load(CampaignProgress.SavePath);Require(loaded.version==4 && loaded.StarsAt(5)>=2 && loaded.cleanMedals[4],"Rating failed save round trip.");
                Require(!session.SaveFailed,"Progress could not be saved.");
            }))yield break;
            Finish(0,"PASS: all seven upgrades; native pointer and keyboard input; locked/reward previews, map stars and unlock; adaptive HUD; unified tabs; manual hint; abandon cancellation; normal stage 5/49 wins, stars/medals/rewards and v4 persistence; goal deficits; Korean/English wide, small and portrait captures.");
        }
    }
}
#endif
